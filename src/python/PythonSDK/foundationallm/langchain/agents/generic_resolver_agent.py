import json
from typing import List
from pydantic import parse_obj_as

from langchain_community.callbacks import get_openai_callback
from langchain_core.language_models import BaseLanguageModel
from langchain_core.messages import AIMessage
from langchain_core.prompts import PromptTemplate

from foundationallm.langchain.agents.agent_base import AgentBase
from foundationallm.config import Configuration
from foundationallm.models.orchestration import CompletionRequest, CompletionResponse
from foundationallm.models import ListOption
from foundationallm.storage import BlobStorageManager
from foundationallm.langchain.data_sources.blob import BlobStorageConfiguration

class GenericResolverAgent(AgentBase):
    """
    The GenericResolverAgent is responsible for choosing one or more options from a list of options 
        consisting of a name and description from a JSON file in blob storage.
        
        This agent determines the best matches based on the incoming user_prompt.
        The user prompt may request one or more options from the list.   
    """
    def __init__(
            self,
            completion_request: CompletionRequest,
            llm: BaseLanguageModel,
            config: Configuration):
        
        ds_config = {}
        for ds in completion_request.data_sources:
            ds_config: BlobStorageConfiguration = ds.configuration
          
        self.user_prompt = completion_request.user_prompt
        self.llm = llm.get_completion_model(completion_request.language_model)
        self.connection_string = config.get_value(
                    ds_config.connection_string_secret
                    )
        self.container_name = ds_config.container
        self.file_names = ds_config.files
        # prompt template expects options list and user_prompt as inputs
        self.prompt_prefix = PromptTemplate.from_template(completion_request.agent.prompt_prefix)
        self.options_list = self.build_options_list(options_list = self.load_options())

    def load_options(self) -> List[ListOption]:
        options_list = []
        blob_storage_mgr = BlobStorageManager(blob_connection_string = self.connection_string,
                                              container_name=self.container_name)
        if "*" in self.file_names:
            blob_list: List[dict] = list(blob_storage_mgr.list_blobs(path=""))
            self.file_names = [blob["name"].split('/')[-1] for blob in blob_list]

        # Load specific files
        for file_name in self.file_names:
            file_content = blob_storage_mgr.read_file_content(file_name).decode("utf-8")
            obj_list = parse_obj_as(List[ListOption], json.loads(file_content))
            options_list.extend(obj_list)

        return options_list

    def build_options_list(self, options_list:List[ListOption]=None) -> str:
        """
        Builds a list of options using their name and descriptions for the resolver prompt.
        """
        if options_list is None or len(options_list)==0:
            return ""
        options_str = "\n\nOptions List:\n"
        for option in options_list:
            options_str +=  "Name: " + option.name + "\n"
            options_str +=  "Description: " + option.description + "\n\n"
        return options_str

    def run(self, prompt: str) -> CompletionResponse:
        """
        Evaluates a list of options against the incoming user prompt.

        Parameters
        ----------
        prompt : str
            The prompt that contains option information, message history, and user prompt.
        
        Returns
        -------
        CompletionResponse
            Returns a CompletionResponse with the name(s) of the selected option(s),
            the user_prompt, and token utilization and execution cost details.
        """
        try:
            with get_openai_callback() as cb:
                chain = self.prompt_prefix | self.llm
                completion_message:AIMessage = chain.invoke(
                                {"options": self.options_list, "user_prompt": prompt}
                                )
                return CompletionResponse(
                    completion = completion_message.content,
                    user_prompt = prompt,
                    completion_tokens = cb.completion_tokens,
                    prompt_tokens = cb.prompt_tokens,
                    total_tokens = cb.total_tokens,
                    total_cost = cb.total_cost
                )
        except Exception as e:
            return CompletionResponse(
                    completion = "A problem on my side prevented me from responding.",
                    user_prompt = prompt
                )
