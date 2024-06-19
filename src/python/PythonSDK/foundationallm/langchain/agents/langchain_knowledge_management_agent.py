from langchain_community.callbacks import get_openai_callback
from langchain_core.prompts import PromptTemplate
from langchain_core.runnables import RunnablePassthrough, RunnableLambda
from langchain_core.output_parsers import StrOutputParser
from foundationallm.langchain.agents import LangChainAgentBase
from foundationallm.langchain.exceptions import LangChainException
from foundationallm.langchain.retrievers import RetrieverFactory, CitationRetrievalBase
from foundationallm.models.orchestration import (
    CompletionResponse
)
from foundationallm.models.agents import KnowledgeManagementCompletionRequest
from foundationallm.models.resource_providers.vectorization import (
    AzureAISearchIndexingProfile,
    AzureOpenAIEmbeddingProfile
)
from msclap import CLAP
import requests
from typing import List, Optional
from foundationallm.storage import BlobStorageManager
import uuid
import os
from pathlib import Path

class LangChainKnowledgeManagementAgent(LangChainAgentBase):
    """
    The LangChain Knowledge Management agent.
    """            
    def get_prediction_from_audio_file(self, attachments: Optional[List[str]] = None) -> str:
        """Generates embeddings from an audio file. Expects a file path as input. Outputs JSON containing the response from a REST API."""
        if attachments is None:
            return None

        if len(attachments) == 0:
            return None
        
        file = attachments[0].lstrip('/')
        container_name = file.split('/')[0]
        blob_path = file.replace(container_name, '').lstrip('/')
        
        # Load and initialize CLAP
        clap_model = CLAP(version = '2023', use_cuda=True)
        
        try:
            storage_manager = BlobStorageManager(
                account_name=self.config.get_value('FoundationaLLM:Attachment:ResourceProviderService:Storage:AccountName'),
                container_name=file.split('/')[0],
                authentication_type=self.config.get_value('FoundationaLLM:Attachment:ResourceProviderService:Storage:AuthenticationType')
            )
        except Exception as e:
            raise e

        try:
            # Load the file from storage.
            if (storage_manager.file_exists(blob_path) == False):
                raise FileNotFoundError(f'The file {blob_path} was not found in blob storage.')
            blob = storage_manager.read_file_content(blob_path)
        except Exception as e:
            raise e

        output_dir = "/classify_tmp/audio_data"
        Path(output_dir).mkdir(parents=True, exist_ok=True)
        
        local_file_path = output_dir + str(uuid.uuid4()) + ".wav"
        try:
            # Save the file locally, so it can be reference by CLAP
            with open(local_file_path, "wb") as local_file:
                local_file.write(bytearray(blob))
        except Exception as e:
            raise e

        file_paths = [local_file_path]
        
        audio_embeddings = clap_model.get_audio_embeddings(file_paths, resample=True)
        data = audio_embeddings.numpy().tolist()

        base_url = self.config.get_value('FoundationaLLM:APIs:AudioClassificationAPI:APIUrl').rstrip('/')
        endpoint = self.config.get_value('FoundationaLLM:APIs:AudioClassificationAPI:Classification:PredictionEndpoint')
        #base_url = 'http://localhost:8865'.rstrip('/')
        #endpoint = '/classification/predict'
        api_endpoint = f'{base_url}{endpoint}'
        print(api_endpoint)

        # Create the embeddings payload.
        payload = {"embeddings": data}
        # TODO: Need to add in correct headers, including any auth headers.
        headers = {"charset": "utf-8", "Content-Type": "application/json"}
        # Make the REST API call.
        r = requests.post(api_endpoint, json=payload, headers=headers)
        # Check the response status code.
        if r.status_code != 200:
            raise Exception(f'Error: ({r.status_code}) {r.text}')
        # Return the JSON response.
        response = r.json()

        # Clean up the local file
        try:
            os.remove(local_file_path)
        except Exception as e:
            raise e

        return response['label']
    
    def invoke(self, request: KnowledgeManagementCompletionRequest) -> CompletionResponse:
        """
        Executes a completion request by querying the vector index with the user prompt.

        Parameters
        ----------
        request : KnowledgeManagementCompletionRequest
            The completion request to execute.

        Returns
        -------
        CompletionResponse
            Returns a CompletionResponse with the generated summary, the user_prompt,
            generated full prompt with context and token utilization and execution cost details.
        """
        self._validate_request(request)
        agent = request.agent

        prompt = self._get_prompt_from_object_id(agent.prompt_object_id, agent.orchestration_settings.agent_parameters)

        with get_openai_callback() as cb:
            try:
                prompt_builder = ''

                # Add the prefix, if it exists.
                if prompt.prefix is not None:
                    prompt_builder = f'{prompt.prefix}\n\n'

                # Add the message history, if it exists.
                conversation_history = agent.conversation_history
                if conversation_history is not None and conversation_history.enabled:
                    prompt_builder += self._build_conversation_history(
                        request.message_history,
                        conversation_history.max_history)

                classification_prediction = self.get_prediction_from_audio_file(request.attachments)
                if classification_prediction is not None:
                    prompt_builder += f"\n\CONTEXT: {classification_prediction}"
                else:
                    # Insert the context into the template.
                    prompt_builder += '{context}'

                # Add the suffix, if it exists.
                if prompt.suffix is not None:
                    prompt_builder += f'\n\n{prompt.suffix}'

                # Get the vector document retriever, if it exists.
                retriever = None
                if request.agent.vectorization is not None and not request.agent.inline_context:

                    text_embedding_profile = AzureOpenAIEmbeddingProfile.from_object(
                        agent.orchestration_settings.agent_parameters[
                            agent.vectorization.text_embedding_profile_object_id])

                    indexing_profiles = []

                    if (agent.vectorization.indexing_profile_object_ids is not None) and (text_embedding_profile is not None):

                        for profile_id in agent.vectorization.indexing_profile_object_ids:
                            indexing_profiles.append(
                                AzureAISearchIndexingProfile.from_object(
                                    agent.orchestration_settings.agent_parameters[profile_id]
                                )
                            )

                        retriever_factory = RetrieverFactory(
                                        indexing_profiles,
                                        text_embedding_profile,
                                        self.config,
                                        request.settings)
                        retriever = retriever_factory.get_retriever()

                # Insert the user prompt into the template.
                if retriever is not None:
                    prompt_builder += "\n\nQuestion: {question}"

                # Create the prompt template.
                prompt_template = PromptTemplate.from_template(prompt_builder)

                if retriever is not None:
                    chain_context = { "context": retriever | retriever.format_docs, "question": RunnablePassthrough() }
                else:
                    chain_context = { "context": RunnablePassthrough() }

                # Compose LCEL chain
                chain = (
                    chain_context
                    | prompt_template
                    | RunnableLambda(self._record_full_prompt)
                    | self._get_language_model(agent.orchestration_settings, request.settings)
                    | StrOutputParser()
                )
                
                completion = chain.invoke(request.user_prompt)
                citations = []
                if isinstance(retriever, CitationRetrievalBase):
                    citations = retriever.get_document_citations()

                return CompletionResponse(
                    completion = completion,
                    citations = citations,
                    user_prompt = request.user_prompt,
                    full_prompt = self.full_prompt.text,
                    completion_tokens = cb.completion_tokens,
                    prompt_tokens = cb.prompt_tokens,
                    total_tokens = cb.total_tokens,
                    total_cost = cb.total_cost
                )
            except Exception as e:
                raise LangChainException(f"An unexpected exception occurred when executing the completion request: {str(e)}", 500)
