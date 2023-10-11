from langchain.base_language import BaseLanguageModel
from langchain.llms.openai import OpenAIChat

from foundationallm.config import Configuration
from foundationallm.langchain.language_models.chat_models import AzureChatOpenAILanguageModel

from foundationallm.models.orchestration import OrchestrationRequestBase
from foundationallm.langchain.agents import AgentBase
from foundationallm.langchain.agents import AnomalyDetectionAgent
from foundationallm.langchain.agents import CSVAgent
from foundationallm.langchain.agents import SqlDbAgent
from foundationallm.langchain.agents import SummaryAgent

class AgentFactory:
    """
    Factory to determine which agent to use.
    """
    
    def __init__(self, request: OrchestrationRequestBase, config: Configuration):
        self.request = request
        self.agent_type = request.agent.type
        self.language_model = request.agent.language_model
        self.config = config
    
    def get_llm(self) -> BaseLanguageModel: # TODO: Move to LLMFactory
        match self.language_model.type:
            case 'openai':
                if self.language_model.subtype == 'chat':
                    if self.language_model.provider == 'azure':
                        return AzureChatOpenAILanguageModel(config = self.config)
                    else:
                        return OpenAIChat
                        
    def get_agent(self) -> AgentBase:
        match self.agent_type:
            case 'anomaly':
                return AnomalyDetectionAgent(self.request, llm=self.get_llm(), app_config=self.config)
            case 'csv':
                return CSVAgent(self.request, llm=self.get_llm(), app_config=self.config)
            case 'sql':
                return SqlDbAgent(self.request, llm=self.get_llm(), app_config=self.config)
            case 'summary':
                return SummaryAgent(self.request, llm=self.get_llm(), app_config=self.config)