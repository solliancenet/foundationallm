from langchain.base_language import BaseLanguageModel
from langchain.llms.openai import OpenAIChat

from foundationallm.config import Configuration
from foundationallm.models.orchestration import CompletionRequest
from foundationallm.langchain.language_models import LanguageModelFactory
from foundationallm.langchain.agents import AgentBase
from foundationallm.langchain.agents import AnomalyDetectionAgent
from foundationallm.langchain.agents import CSVAgent
from foundationallm.langchain.agents import SqlDbAgent
from foundationallm.langchain.agents import SummaryAgent
from foundationallm.langchain.agents import ConversationalAgent

class AgentFactory:
    """
    Factory to determine which agent to use.
    """
    
    def __init__(self, completion_request: CompletionRequest, config: Configuration):
        """
        Initializes an AgentFactory for selecting which agent to use for completion.

        Parameters
        ----------
        completion_request : CompletionRequest
            The completion request object containing the user prompt to execute, message history,
            and agent and data source metadata.
        app_config : Configuration
            Application configuration class for retrieving configuration settings.
        """
        self.completion_request = completion_request
        self.agent = completion_request.agent
        self.language_model = completion_request.language_model
        self.config = config
    
    def get_llm(self) -> BaseLanguageModel:
        """
        Retrieves the language model to use for the completion.
        
        Returns
        -------
        BaseLanguageModel
            Returns the language model to use for the completion.
        """
        return LanguageModelFactory(completion_request=self.completion_request, config=self.config).get_llm()
                        
    def get_agent(self) -> AgentBase:
        """
        Retrieves the best agent for responding to the user prompt.
        
        Returns
        -------
        AgentBase
            Returns the best agent for responding to the user prompt.
            The default agent will be returned in cases where the other
            available agents are not suited to respond to the user prompt.
        """
        match self.agent.type:
            case 'anomaly':
                return AnomalyDetectionAgent(self.completion_request, llm=self.get_llm(), app_config=self.config)
            case 'csv':
                return CSVAgent(self.completion_request, llm=self.get_llm(), app_config=self.config)
            case 'sql':
                return SqlDbAgent(self.completion_request, llm=self.get_llm(), app_config=self.config)
            case 'summary':
                return SummaryAgent(self.completion_request, llm=self.get_llm(), app_config=self.config)
            case _:
                return ConversationalAgent(self.completion_request, llm=self.get_llm(), app_config=self.config)