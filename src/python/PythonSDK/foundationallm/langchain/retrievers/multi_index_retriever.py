"""
Class: MultiIndexRetriever
Description: LangChain retriever for multi-retriever search.
"""
import json
from typing import List, Optional, Union, Tuple
from langchain_openai import OpenAIEmbeddings
from langchain_core.callbacks import (
    AsyncCallbackManagerForRetrieverRun,
    CallbackManagerForRetrieverRun,
)
from langchain_core.documents import Document
from langchain_core.retrievers import BaseRetriever
from azure.search.documents import SearchClient
from azure.search.documents.models import VectorizedQuery
from azure.core.credentials import AzureKeyCredential
from azure.identity import DefaultAzureCredential
from foundationallm.models.orchestration import Citation
from .citation_retrieval_base import CitationRetrievalBase

class MultiIndexRetriever(BaseRetriever, CitationRetrievalBase):
    """
    LangChain multi retriever.
    Properties:
        retrievers: List[BaseRetriever] -> List of retrievers to use for completion requests.

    Searches embedding and text fields in the list of retrievers indexes for the top_n most relevant documents.

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

    top_n: int = 10
    retrievers: List[BaseRetriever] = []
    search_results: Optional[Tuple[str, Document]] = [] # Tuple of document id and document

    def add_retriever(self, retriever: BaseRetriever):
        """
        Add a retriever to the list of retrievers to use for completion requests.

        Parameters
        ----------
        retriever : BaseRetriever
            The retriever to add to the list of retrievers.
        """
        self.retrievers.append(retriever)

    def get_document_citations(self):

        citations = []
        added_ids = set()  # Avoid duplicates
        for result in self.search_results:  # Unpack the tuple
            result_id = result.id
            metadata = result.metadata
            if metadata is not None and 'multipart_id' in metadata and metadata['multipart_id']:
                if result_id not in added_ids:
                    title = (metadata['multipart_id'][-1]).split('/')[-1]
                    filepath = '/'.join(metadata['multipart_id'])
                    citations.append(Citation(id=result_id, title=title, filepath=filepath))
                    added_ids.add(result_id)

        return citations

    def _get_relevant_documents(
        self, query: str, *, run_manager: CallbackManagerForRetrieverRun
    ) -> List[Document]:
        """
        Performs a synchronous hybrid search on Azure AI Search index
        """

        total_results = []

        for retriever in self.retrievers:

            results = retriever._get_relevant_documents(
                query=query,
                run_manager=run_manager
            )

            total_results.extend(results)

        #sort by relevance/score/metric
        total_results.sort(key=lambda x: x.score, reverse=True)

        #take top n of search_results
        self.search_results = total_results[:self.top_n]

        return self.search_results

    def format_docs(self, docs:List[Document]) -> str:
        """
        Generates a formatted string from a list of documents for use
        as the context for the completion request.
        """
        return "\n\n".join(doc.page_content for doc in docs)
