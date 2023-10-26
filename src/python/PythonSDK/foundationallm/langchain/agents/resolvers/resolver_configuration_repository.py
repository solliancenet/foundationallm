from foundationallm.config import Configuration
from .resolver_configuration_storage_manager import ResolverConfigurationStorageManager
from langchain.prompts import PromptTemplate
from langchain.prompts.loading import load_prompt_from_config
from langchain.base_language import BaseLanguageModel
import json
from foundationallm.models.metadata import LanguageModel
from foundationallm.langchain.language_models import LanguageModelFactory

class ResolverConfigurationRepository:
    """
    ResolverConfigurationRepository is responsible for retrieving internal agent configuration details.
    """
    def __init__(self, config: Configuration = None):
        self.config = config
        self.repository = ResolverConfigurationStorageManager(config=config)
        
    def get_agent_resolver_prompt(self) -> PromptTemplate:
        prompt_json = self.repository.read_file_content("agent-resolver/prompt.json")
        prompt_as_dict = json.loads(prompt_json)
        prompt_template = load_prompt_from_config(prompt_as_dict)        
        return prompt_template
    
    def get_agent_resolver_llm_details(self) -> BaseLanguageModel:
        llm_json = self.repository.read_file_content("agent-resolver/language-model.json")
        llm_obj = LanguageModel.model_validate_json(llm_json)
        factory = LanguageModelFactory(language_model=llm_obj, config=self.config)
        llm_imp =  factory.get_llm()
        llm = llm_imp.get_language_model()
        return llm