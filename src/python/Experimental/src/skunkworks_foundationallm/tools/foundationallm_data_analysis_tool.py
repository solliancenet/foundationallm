# Platform imports
import asyncio
from typing import Any, Optional, List

# Azure imports
from azure.identity import DefaultAzureCredential, get_bearer_token_provider

# LangChain imports
from langchain_azure_dynamic_sessions import SessionsPythonREPLTool
from langchain_core.callbacks import AsyncCallbackManagerForToolRun, CallbackManagerForToolRun
from langchain_core.messages import HumanMessage, SystemMessage, BaseMessage
from langchain_core.runnables import RunnableConfig
from langchain_core.tools import ToolException
from langchain_openai import AzureOpenAI, AzureChatOpenAI

# FoundationaLLM imports
from foundationallm.config import Configuration, UserIdentity
from foundationallm.models.constants import (
    ResourceObjectIdPropertyNames,
    ResourceObjectIdPropertyValues,
    ResourceProviderNames,
    AIModelResourceTypeNames,
    PromptResourceTypeNames
)
from foundationallm.langchain.common import FoundationaLLMToolBase
from foundationallm.models.agents import AgentTool

class FoundationaLLMDataAnalysisTool(FoundationaLLMToolBase):

    response_format: str = 'content'

    def __init__(self, tool_config: AgentTool, objects: dict, user_identity:UserIdentity, config: Configuration):
        """ Initializes the FoundationaLLMDataAnalysisTool class with the tool configuration,
            exploded objects collection, user_identity, and platform configuration. """
        
        super().__init__(tool_config, objects, user_identity, config)
        
        self.code_interpreter_tool = SessionsPythonREPLTool(
            session_id='skunkworks-0001',
            response_format='content_and_artifact',
            pool_management_endpoint=tool_config.properties['pool_management_endpoint']
        )

        self.__create_code_gen_llm()
        self.__create_code_gen_prompt()

    def __create_code_gen_llm(self):
        """ Creates the main LLM instance and saves it to self.main_llm. """

        model_object_id = self.tool_config.get_resource_object_id_properties(
            ResourceProviderNames.FOUNDATIONALLM_AIMODEL,
            AIModelResourceTypeNames.AI_MODELS,
            ResourceObjectIdPropertyNames.OBJECT_ROLE,
            ResourceObjectIdPropertyValues.MAIN_MODEL
        )

        if model_object_id:

            main_llm_model_object_id = model_object_id.object_id
            main_llm_model_properties = self.objects[main_llm_model_object_id]
            main_llm_endpoint_object_id = main_llm_model_properties['endpoint_object_id']
            main_llm_deployment_name = main_llm_model_properties['deployment_name']
            main_llm_endpoint_properties = self.objects[main_llm_endpoint_object_id]
            main_llm_endpoint_url = main_llm_endpoint_properties['url']
            main_llm_endpoint_api_version = main_llm_endpoint_properties['api_version']

            scope = 'https://cognitiveservices.azure.com/.default'
            # Set up a Azure AD token provider.
            token_provider = get_bearer_token_provider(
                self.default_credential,
                scope
            )

            self.code_gen_llm = AzureChatOpenAI(
                azure_endpoint=main_llm_endpoint_url,
                api_version=main_llm_endpoint_api_version,
                openai_api_type='azure_ad',
                azure_ad_token_provider=token_provider,
                azure_deployment=main_llm_deployment_name,
                temperature=0.5,
                top_p=0.5,
                tool_choice=None
            )

    def __create_code_gen_prompt(self):
        
        prompt_object_id = self.tool_config.get_resource_object_id_properties(
            ResourceProviderNames.FOUNDATIONALLM_PROMPT,
            PromptResourceTypeNames.PROMPTS,
            ResourceObjectIdPropertyNames.OBJECT_ROLE,
            ResourceObjectIdPropertyValues.MAIN_PROMPT
        )

        if prompt_object_id:
            main_prompt_object_id = prompt_object_id.object_id
            main_prompt_properties = self.objects[main_prompt_object_id]
            main_prompt = main_prompt_properties['prefix']

            self.code_gen_prompt = main_prompt

    def __build_messages(
        self,
        prompt: str,
        runnable_config: RunnableConfig
    ) -> List[BaseMessage]:

        user_prompt = runnable_config['configurable']['original_user_prompt'] if 'original_user_prompt' in runnable_config['configurable'] else prompt
        
        messages = [
            SystemMessage(content=self.code_gen_prompt),
            HumanMessage(content=user_prompt)
        ] if self.code_gen_prompt else [HumanMessage(content=user_prompt)]

        return messages

    def _run(self,
            prompt: str,
            runnable_config: RunnableConfig = None,
            run_manager: Optional[CallbackManagerForToolRun] = None
            ) -> str:
        
        result = self.code_gen_llm.invoke(
            self.__build_messages(prompt, runnable_config))
        
        code_result = self.code_interpreter_tool.invoke(result.content)

        return code_result

    async def _arun(self,
        prompt: str = None,
        runnable_config: RunnableConfig = None,
        run_manager: Optional[AsyncCallbackManagerForToolRun] = None
        ) -> str:

        result = asyncio.run(
            self.code_gen_llm.ainvoke(self.__build_messages(prompt, runnable_config)))

        code_result = self.code_interpreter_tool.invoke(result.content)

        return code_result