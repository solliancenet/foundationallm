"""
The KnowledgeManagementAgent class is responsible for executing a completion request
over text, whether that be from a user prompt or a RAG pattern, over a vector store.
"""
from typing import List
from langchain_community.callbacks import get_openai_callback
from langchain_core.documents import Document
from langchain_core.language_models import BaseLanguageModel
from langchain_core.prompts import PromptTemplate
from langchain_core.runnables import RunnablePassthrough, RunnableLambda
from langchain_core.output_parsers import StrOutputParser
from foundationallm.config import Configuration
from foundationallm.langchain.message_history import build_message_history
from foundationallm.langchain.agents.agent_base import AgentBase
from foundationallm.langchain.retrievers import RetrieverFactory
from foundationallm.models.metadata import ConversationHistory
from foundationallm.models.orchestration import KnowledgeManagementCompletionRequest, CompletionResponse
from foundationallm.resources import ResourceProvider

class KnowledgeManagementAgent(AgentBase):
    """
    Agent for pass-through user_prompt or RAG pattern over a vector store.
    """

    def __init__(
            self,
            completion_request: KnowledgeManagementCompletionRequest,
            llm: BaseLanguageModel,
            config: Configuration,
            resource_provider: ResourceProvider):
        """
        Initializes a generic knowledge management agent.

        Parameters
        ----------
        completion_request : CompletionRequest
            The completion request object containing the user prompt to execute, message history,
            and agent and data source metadata.
        llm: BaseLanguageModel
            The language model class to use for embedding and completion.
        config : Configuration
            Application configuration class for retrieving configuration settings.
        resource_provider : ResourceProvider
            Resource provider for retrieving embedding and indexing profiles.        
        """       

        self.llm = llm.get_completion_model(completion_request.agent.language_model)
        self.retrievers = []
        for indexing_profile in completion_request.agent.indexing_profile_object_ids:
            retriever_factory = RetrieverFactory(
                            indexing_profile_object_id = indexing_profile,
                            text_embedding_profile_object_id= completion_request.agent.text_embedding_profile_object_id,
                            config = config,
                            resource_provider = resource_provider)
            self.retrievers.append(retriever_factory.get_retriever())
                        
        self.retriever = next((r for r in self.retrievers), None)
        self.agent_prompt = resource_provider.get_resource(completion_request.agent.prompt_object_id).prefix

        conversation_history: ConversationHistory = completion_request.agent.conversation_history
        self.message_history_enabled = conversation_history.enabled
        self.message_history_count = conversation_history.max_history        
        self.message_history = completion_request.message_history        
        self.full_prompt = ""

    def __format_docs(self, docs:List[Document]) -> str:
        """
        Generates a formatted string from a list of documents for use
        as the context for the completion request.
        """
        return "\n\n".join(doc.page_content for doc in docs)
    
    def __record_full_prompt(self, prompt: str) -> str:
        """
        Records the full prompt for the completion request.

        Parameters
        ----------
        prompt : str
            The prompt that is populated with context.
        
        Returns
        -------
        str
            Returns the full prompt.
        """
        self.full_prompt = prompt
        return prompt

    def run(self, prompt: str) -> CompletionResponse:
        """
        Executes a completion request by querying the vector index with the user prompt.

        Parameters
        ----------
        prompt : str
            The prompt for which a summary completion is begin generated.
        
        Returns
        -------
        CompletionResponse
            Returns a CompletionResponse with the generated summary, the user_prompt,
            generated full prompt with context
            and token utilization and execution cost details.
        """
        with get_openai_callback() as cb:  
            prompt_builder = self.agent_prompt
            if self.message_history_enabled == True:
                prompt_builder = prompt_builder + build_message_history(self.message_history, self.message_history_count)
            prompt_builder = prompt_builder + "\n\nQuestion: {question}\n\nContext: {context}\n\nAnswer:"            
            prompt_template = PromptTemplate.from_template(prompt_builder)

            # Compose LCEL chain
            chain = (
                { "context": self.retriever | self.__format_docs, "question": RunnablePassthrough() }
                | prompt_template
                | RunnableLambda(self.__record_full_prompt)
                | self.llm
                | StrOutputParser()
            )

            return CompletionResponse(
                completion = chain.invoke(prompt),
                user_prompt = prompt,
                full_prompt = self.full_prompt.text,
                completion_tokens = cb.completion_tokens,
                prompt_tokens = cb.prompt_tokens,
                total_tokens = cb.total_tokens,
                total_cost = cb.total_cost
            )
