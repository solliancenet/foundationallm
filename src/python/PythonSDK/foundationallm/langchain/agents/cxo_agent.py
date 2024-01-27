from operator import itemgetter

from azure.core.credentials import AzureKeyCredential

from langchain.chains import ConversationalRetrievalChain
from langchain.memory import ConversationBufferMemory
from langchain_community.callbacks import get_openai_callback
from langchain_community.vectorstores.chroma import Chroma
from langchain_core.prompts import PromptTemplate

from foundationallm.config import Configuration
from foundationallm.langchain.agents import AgentBase
from foundationallm.langchain.language_models import LanguageModelBase
from foundationallm.models.orchestration import CompletionRequest, CompletionResponse
from foundationallm.langchain.retrievers import SearchServiceFilterRetriever

class CXOAgent(AgentBase):
    """
    Agent for providing CXO analysis.
    """

    def __init__(
            self,
            completion_request: CompletionRequest,
            llm: LanguageModelBase,
            config: Configuration):
        """
        Initializes a CXO agent.

        Note: The CXO agent supports a single file.

        Parameters
        ----------
        completion_request : CompletionRequest
            The completion request object containing the user prompt to execute, message history,
            and agent and data source metadata.
        llm : LanguageModelBase
            The language model to use for executing the completion request.
        config : Configuration
            Application configuration class for retrieving configuration settings.
        """
        self.prompt_prefix = completion_request.agent.prompt_prefix
        self.prompt_suffix = completion_request.agent.prompt_suffix
        self.message_history = completion_request.message_history
        self.ds_config = completion_request.data_sources[0].configuration

        self.vector_store_address = config.get_value(self.ds_config.endpoint)
        self.vector_store_password = config.get_value(self.ds_config.key_secret)

        self.llm = llm.get_completion_model(completion_request.language_model)
        self.embeddings = llm.get_embedding_model(completion_request.embedding_model)

        # Load the CSV file
        company = self.ds_config.company
        retriever_mode = self.ds_config.retriever_mode

        self.index_name = self.ds_config.index_name
        temp_sources = self.ds_config.sources
        self.filters = []

        for source in temp_sources:
            self.filters.append(f"search.ismatch('{source}', 'metadata', 'simple', 'all')")

        if ( retriever_mode == "azure" ):
            local_path = f"{company}-financials"
            self.retriever = self.get_azure_retiever(local_path, embedding_field_name="content_vector", text_field_name="content", top_n=self.ds_config.top_n)

        if ( retriever_mode == "chroma" ):
            local_path = f"/temp/{company}"
            self.retriever = self.get_chroma_retiever(local_path)

        max_message_histroy = 1 * -2
        self.message_history = self.message_history[max_message_histroy:]
        memory = ConversationBufferMemory(memory_key="chat_history", return_messages=True)
        # Add previous messages to the memory
        for i in range(0, len(self.message_history), 2):
            history_pair = itemgetter(i,i+1)(self.message_history)
            for message in history_pair:
                if message.sender.lower() == 'user':
                    user_input = message.text
                else:
                    ai_output = message.text
            memory.save_context({"input": user_input}, {"output": ai_output})

        prompt = PromptTemplate(
            template=self.prompt_prefix,
            input_variables=["context", "question"]
        )

        self.llm_chain = ConversationalRetrievalChain.from_llm(
            llm=self.llm,
            retriever=self.retriever,
            return_source_documents=False,
            memory=memory,
            chain_type="stuff",
            combine_docs_chain_kwargs={"prompt": prompt},
            verbose=True
        )

        print('done with init')

    def run(self, prompt: str) -> CompletionResponse:
        """
        Executes a query against the contents of a CSV file.

        Parameters
        ----------
        prompt : str
            The prompt for which a completion is begin generated.

        Returns
        -------
        CompletionResponse
            Returns a CompletionResponse with the CSV file query completion response,
            the user_prompt, and token utilization and execution cost details.
        """

        with get_openai_callback() as cb:
            return CompletionResponse(
                completion = self.llm_chain.invoke(prompt, return_only_outputs=True)['answer'],
                user_prompt = prompt,
                completion_tokens = cb.completion_tokens,
                prompt_tokens = cb.prompt_tokens,
                total_tokens = cb.total_tokens,
                total_cost = cb.total_cost
            )

    def get_azure_retiever(self, path, top_n=5, embedding_field_name="Embedding", text_field_name="Text"):

        credential = AzureKeyCredential(self.vector_store_password)

        return SearchServiceFilterRetriever(
                    endpoint=self.vector_store_address,
                    index_name=self.index_name,
                    filters=self.filters,
                    top_n=top_n,
                    embedding_field_name=embedding_field_name,
                    text_field_name=text_field_name,
                    credential=credential,
                    embedding_model=self.embeddings
            )

    def get_chroma_retiever(self, path):

        prsstdb = Chroma(
                persist_directory=path,
                embedding_function=self.embeddings
            )

        return prsstdb.as_retriever()
