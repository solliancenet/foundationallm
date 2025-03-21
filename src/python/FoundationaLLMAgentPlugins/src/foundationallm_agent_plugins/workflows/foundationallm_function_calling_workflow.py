"""
Class: FoundationaLLMFunctionCallingWorkflow
Description: FoundationaLLM Function Calling workflow to invoke tools at a low level.
"""
import time
from typing import Dict, List, Optional
from logging import Logger
from opentelemetry.trace import Tracer
from langchain_core.messages import (
    AIMessage,    
    HumanMessage,
    SystemMessage   
)
from opentelemetry.trace import SpanKind
from foundationallm.config import Configuration, UserIdentity
from foundationallm.langchain.common import (
    FoundationaLLMWorkflowBase,
    FoundationaLLMToolBase
)
from foundationallm.langchain.language_models import LanguageModelFactory
from foundationallm.models.agents import ExternalAgentWorkflow
from foundationallm.models.constants import (
    AgentCapabilityCategories,
    ResourceObjectIdPropertyNames,
    ResourceObjectIdPropertyValues,
    ResourceProviderNames,
    AIModelResourceTypeNames,
    PromptResourceTypeNames
)
from foundationallm.models.messages import MessageHistoryItem
from foundationallm.models.orchestration import (
    CompletionResponse,
    ContentArtifact,
    FileHistoryItem,
    OpenAITextMessageContentItem,
    OpenAIImageFileMessageContentItem,
    OpenAIFilePathMessageContentItem
)
from foundationallm.telemetry import Telemetry
from foundationallm_agent_plugins.common.constants import(    
    CONTENT_ARTIFACT_TYPE_FILE,
    CONTENT_ARTIFACT_TYPE_WORKFLOW_EXECUTION
)

class FoundationaLLMFunctionCallingWorkflow(FoundationaLLMWorkflowBase):
    """
    FoundationaLLM workflow implementing a router pattern for tool invocation
    using Azure OpenAI completion models.
    """

    def __init__(self,
                 workflow_config: ExternalAgentWorkflow,
                 objects: Dict,
                 tools: List[FoundationaLLMToolBase],
                 user_identity: UserIdentity,
                 config: Configuration):
        """
        Initializes the FoundationaLLMWorkflowBase class with the workflow configuration.

        Parameters
        ----------
        workflow_config : ExternalAgentWorkflow
            The workflow assigned to the agent.
        objects : dict
            The exploded objects assigned from the agent.
        tools : List[FoundationaLLMToolBase]
            The tools assigned to the agent.
        user_identity : UserIdentity
            The user identity of the user initiating the request.
        config : Configuration
            The application configuration for FoundationaLLM.
        """
        super().__init__(workflow_config, objects, tools, user_identity, config)
        self.name = workflow_config.name
        self.logger : Logger = Telemetry.get_logger(self.name)
        self.tracer : Tracer = Telemetry.get_tracer(self.name)        
        self.default_error_message = workflow_config.properties.get(
            'default_error_message',
            'An error occurred while processing the request.') \
            if workflow_config.properties else 'An error occurred while processing the request.'
        
        # Create prompt first, then LLM
        self.__create_workflow_prompt()
        self.__create_workflow_llm()        

    async def invoke_async(self,
                           operation_id: str,
                           user_prompt:str,
                           user_prompt_rewrite: Optional[str],
                           message_history: List[MessageHistoryItem],
                           file_history: List[FileHistoryItem])-> CompletionResponse:
        """
        Invokes the workflow asynchronously.

        Parameters
        ----------
        operation_id : str
            The unique identifier of the FoundationaLLM operation.
        user_prompt : str
            The user prompt message.
        user_prompt_rewrite : str
            The user prompt rewrite message containing additional context to clarify the user's intent.
        message_history : List[BaseMessage]
            The message history.
        file_history : List[FileHistoryItem]
            The file history.
        """
        llm_prompt = user_prompt_rewrite or user_prompt

        # Convert message history to LangChain message types
        langchain_messages = []
        for msg in message_history:          
            # Convert MessageHistoryItem to appropriate LangChain message type
            if msg.sender == "User":
                langchain_messages.append(HumanMessage(content=msg.text))
            else:
                langchain_messages.append(AIMessage(content=msg.text))

        # If there are files attached to the request, add them to the system prompt
        if file_history:
            files_prompt = "\nYou have access to the following files:\n"
            for file in file_history:
                files_prompt += f'{file.order}. Original file name: "{file.original_file_name}" with the file path "{file.file_path}"\n'
            files_prompt += "\nIf the question is requesting information about a file and no file is specified, use a recency bias to determine which files to use, the most recent files being at the end of the list.\n"
            self.workflow_prompt += files_prompt

        self.logger.debug(f'Workflow prompt: {self.workflow_prompt}')
        print(f'Workflow prompt console: {self.workflow_prompt}')
        messages = [
            SystemMessage(content=self.workflow_prompt),
            *langchain_messages,
            HumanMessage(content=llm_prompt)
        ]
        messages_with_toolchain = messages.copy()             
        content_artifacts: List[ContentArtifact] = []
        completion_tokens = 0
        prompt_tokens = 0
        final_response = None
        max_loops = 10
        loop_count = 0

        with self.tracer.start_as_current_span(f'{self.name}_workflow', kind=SpanKind.INTERNAL):
            while loop_count < max_loops:
                loop_count += 1
                with self.tracer.start_as_current_span(f'{self.name}_workflow_llm_call', kind=SpanKind.INTERNAL):
                    router_start_time = time.time() 
                    llm_bound_tools = self.workflow_llm.bind_tools(self.tools)                   
                    response = await llm_bound_tools.ainvoke(messages_with_toolchain)
                    router_end_time = time.time()
                
                if response.tool_calls:
                    # create a deep copy of the messages for tool calling.                    
                    messages_with_toolchain.append(AIMessage(content=response.content))
                    for tool_call in response.tool_calls:
                        with self.tracer.start_as_current_span(f'{self.name}_tool_call', kind=SpanKind.INTERNAL) as tool_call_span:
                            tool_call_span.set_attribute('tool_call_id', tool_call['id'])
                            tool_call_span.set_attribute('tool_call_function', tool_call['name'])     
                            # Add tool call as AIMessage                       
                            messages_with_toolchain.append(AIMessage(
                                content=f'Calling tool {tool_call["name"]} with args: {tool_call["args"]}'
                            ))
                            # Get the tool from the tools list
                            tool = next((t for t in self.tools if t.name == tool_call['name']), None)
                            if tool:                                                            
                                tool_message = await tool.ainvoke(tool_call)                           
                                content_artifacts.extend(tool_message.artifact)
                                # Add tool response as AIMessage
                                messages_with_toolchain.append(AIMessage(content=str(tool_message)))
                            else:
                                tool_response = 'Tool not found'
                                messages_with_toolchain.append(AIMessage(content=tool_response))

                    # Ask the LLM to verify if the answer is correct if not, loop again with the current messages.
                    verification_messages = messages_with_toolchain.copy()
                    verification_messages.append(HumanMessage(content=f'Verify the requirements are met for this request: "{llm_prompt}", use the other messages only for context. If yes, answer with the single word "DONE". If not, generate more detailed instructions to satisfy the request.'))
                    verification_llm_response = await self.workflow_llm.ainvoke(verification_messages, tools=None)
                    verification_response = verification_llm_response.content
                    if verification_response.strip().upper() == 'DONE':
                        break # exit the loop if the requirements are met.
                    else:
                        messages_with_toolchain.append(AIMessage(content=verification_response))
                        continue # loop again if the requirements are not met.
        
            with self.tracer.start_as_current_span(f'{self.name}_final_llm_call', kind=SpanKind.INTERNAL):                    
                final_llm_response = await self.workflow_llm.ainvoke(messages_with_toolchain, tools=None)
                completion_tokens += final_llm_response.usage_metadata['input_tokens']
                prompt_tokens += final_llm_response.usage_metadata['output_tokens']
                final_response = final_llm_response.content
            
            workflow_content_artifact = self.__create_workflow_execution_content_artifact(
                llm_prompt,
                response.content,
                response.usage_metadata['input_tokens'],
                response.usage_metadata['output_tokens'],
                router_end_time - router_start_time)
            content_artifacts.append(workflow_content_artifact)
            
            # Initialize response_content with the result, taking final_response as priority.   
            response_content = []         
            final_response_content = OpenAITextMessageContentItem(
                value= final_response or response.content,
                agent_capability_category=AgentCapabilityCategories.FOUNDATIONALLM_KNOWLEDGE_MANAGEMENT
            )
            response_content.append(final_response_content)

            # Process any generated files.
            for artifact in content_artifacts:
                if artifact.type == CONTENT_ARTIFACT_TYPE_FILE:
                    # if the file path has an image extension, add it to the response_content as an OpenAIImageFileMessageContentItem.    
                    if any(artifact.filepath.lower().endswith(ext) for ext in ['.png', '.jpg', '.jpeg', '.gif', '.bmp', '.tiff', '.ico', '.webp']):
                        response_content.append(OpenAIImageFileMessageContentItem(
                            file_id = artifact.filepath,
                            agent_capability_category=AgentCapabilityCategories.FOUNDATIONALLM_KNOWLEDGE_MANAGEMENT
                        ))
                    else:
                        # if it's not an image, add it to the final_response_content as an annotation of type OpenAIFilePathMessageContentItem.
                        final_response_content.annotations.append(OpenAIFilePathMessageContentItem(
                            file_id = artifact.filepath,
                            agent_capability_category=AgentCapabilityCategories.FOUNDATIONALLM_KNOWLEDGE_MANAGEMENT
                        ))           

            retvalue = CompletionResponse(
                operation_id=operation_id,
                content = response_content,
                content_artifacts=content_artifacts,
                user_prompt=llm_prompt,
                full_prompt=self.workflow_prompt,
                completion_tokens=completion_tokens,
                prompt_tokens=prompt_tokens,
                total_tokens=prompt_tokens + completion_tokens,
                total_cost=0
            )
            return retvalue

    def __create_workflow_llm(self):
        """ Creates the workflow LLM instance and saves it to self.workflow_llm. """        
        language_model_factory = LanguageModelFactory(self.objects, self.config)        
        model_object_id = self.workflow_config.get_resource_object_id_properties(
            ResourceProviderNames.FOUNDATIONALLM_AIMODEL,
            AIModelResourceTypeNames.AI_MODELS,
            ResourceObjectIdPropertyNames.OBJECT_ROLE,
            ResourceObjectIdPropertyValues.MAIN_MODEL
        )        
        if model_object_id:            
            self.workflow_llm = language_model_factory.get_language_model(model_object_id.object_id)                  
        else:
            error_msg = 'No main model found in workflow configuration'
            self.logger.error(error_msg)
            raise ValueError(error_msg)

    def __create_workflow_prompt(self):
        """ Creates the workflow prompt instance and saves it to self.workflow_prompt. """        
        prompt_object_id = self.workflow_config.get_resource_object_id_properties(
            ResourceProviderNames.FOUNDATIONALLM_PROMPT,
            PromptResourceTypeNames.PROMPTS,
            ResourceObjectIdPropertyNames.OBJECT_ROLE,
            ResourceObjectIdPropertyValues.MAIN_PROMPT
        )       
        if prompt_object_id:
            main_prompt_object_id = prompt_object_id.object_id
            main_prompt_properties = self.objects[main_prompt_object_id]          
            self.workflow_prompt = main_prompt_properties['prefix']            
        else:
            error_msg = 'No main prompt found in workflow configuration'
            self.logger.error(error_msg)
            raise ValueError(error_msg)
    
    def __create_workflow_execution_content_artifact(
            self,
            original_prompt: str,
            intent: str,
            prompt_tokens: int = 0,
            completion_tokens: int = 0,
            completion_time_seconds: float = 0) -> ContentArtifact:

        content_artifact = ContentArtifact(id=self.workflow_config.name)
        content_artifact.source = self.workflow_config.name
        content_artifact.type = CONTENT_ARTIFACT_TYPE_WORKFLOW_EXECUTION
        content_artifact.content = original_prompt
        content_artifact.title = self.workflow_config.name
        content_artifact.filepath = None
        content_artifact.metadata = {
            'intent': intent,
            'prompt_tokens': str(prompt_tokens),
            'completion_tokens': str(completion_tokens),
            'completion_time_seconds': str(completion_time_seconds)
        }
        return content_artifact
