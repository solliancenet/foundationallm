"""
Class: SearchServiceRetriever
Description: LangChain retriever for Azure AI Search.
"""
from typing import List, Optional

from langchain_openai import OpenAIEmbeddings , AzureOpenAIEmbeddings
from langchain_core.callbacks import (
    AsyncCallbackManagerForRetrieverRun,
    CallbackManagerForRetrieverRun,
)
from langchain_core.documents import Document
from langchain_core.retrievers import BaseRetriever

from azure.search.documents import SearchClient
from azure.search.documents.models import VectorizedQuery
from azure.core.credentials import AzureKeyCredential

class SearchServiceFilterRetriever(BaseRetriever):
    """
    LangChain retriever for Azure AI Search.
    Properties:
        endpoint: str -> Azure AI Search endpoint
        index_name: str -> Azure AI Search index name
        top_n : int -> number of results to return from vector search
        embedding_field_name: str -> name of the field containing the embedding vector
        text_field_name: str -> name of the field containing the raw text
        credential: AzureKeyCredential -> Azure AI Search credential
        embedding_model: OpenAIEmbeddings -> OpenAIEmbeddings model

    Searches embedding and text fields in the index for the top_n most relevant documents.

    Default FFLM document structure (overridable by setting the embedding and text field names):
        {
            "Id": "<GUID>",
            "Embedding": [0.1, 0.2, 0.3, ...], # embedding vector of the Text
            "Text": "text of the chunk",
            "Description": "General description about the source of the text",
            "AdditionalMetadata": "JSON string of metadata"
            "ExternalSourceName": "name and location the text came from, url, blob storage url"
            "IsReference": "true/false if the document is a reference document"
        }
    """
    endpoint: str
    index_name: str
    filters : List[str]
    top_n : int
    embedding_field_name: Optional[str] = "Embedding"
    text_field_name: Optional[str] = "Text"
    credential: AzureKeyCredential
    embedding_model: OpenAIEmbeddings

    class Config:
        """Configuration for this pydantic object."""
        arbitrary_types_allowed = True

    def __get_embeddings(self, text: str) -> List[float]:
        """
        Returns embeddings vector for a given text.
        """
        embedding = self.embedding_model.embed_query(text)
        return embedding

    def _get_relevant_documents(
        self, query: str, *, run_manager: CallbackManagerForRetrieverRun
    ) -> List[Document]:
        """
        Performs a synchronous hybrid search on Azure AI Search index
        """

        results_list = []

        search_client = SearchClient(self.endpoint, self.index_name, self.credential)

        for filter in self.filters:

            try:
                vector_query = VectorizedQuery(vector=self.__get_embeddings(query),
                                                k_nearest_neighbors=3,
                                                fields=self.embedding_field_name)

                if (filter == "search.ismatch('*', 'metadata', 'simple', 'all')"):
                        results = search_client.search(
                        search_text=query,
                        vector_queries=[vector_query],
                        top=self.top_n,
                        select=[self.text_field_name]
                    )
                else:
                    results = search_client.search(
                        search_text=query,
                        filter=filter,
                        vector_queries=[vector_query],
                        top=self.top_n,
                        select=[self.text_field_name]
                    )

                for result in results:
                    try:
                        results_list.append(Document(
                            page_content=result[self.text_field_name]
                        ))
                    except Exception as e:
                        print(e)

            except Exception as e:
                print(e)

            if ( filter == "search.ismatch('*', 'metadata', 'simple', 'all')"):
                break

        return results_list

    async def _aget_relevant_documents(
        self, query: str, *, run_manager: AsyncCallbackManagerForRetrieverRun
    ) -> List[Document]:
        """
        Performs an asynchronous hybrid search on Azure AI Search index
        NOTE: This functionality is not currently supported in the underlying Azure SDK.
        """
        raise Exception(f"Asynchronous search not supported.")
