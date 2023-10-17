from unittest.mock import Base
from langchain.base_language import BaseLanguageModel
from foundationallm.config import Configuration
from foundationallm.models.orchestration import CompletionRequest
from foundationallm.langchain.agents import AgentBase
from foundationallm.langchain.agents import AnomalyDetectionAgent
from foundationallm.langchain.agents import CSVAgent
from foundationallm.langchain.agents import SqlDbAgent
from foundationallm.langchain.agents import SummaryAgent
from foundationallm.langchain.agents import BlobStorageAgent
from foundationallm.langchain.agents import ConversationalAgent


class AgentFactory:
    """
    Factory to determine which agent to use.
    """
    
    def __init__(self, completion_request: CompletionRequest, llm: BaseLanguageModel, config: Configuration):
        """
        Initializes an AgentFactory for selecting which agent to use for completion.

        Parameters
        ----------
        completion_request : CompletionRequest
            The completion request object containing the user prompt to execute, message history,
            and agent and data source metadata.
        config : Configuration
            Application configuration class for retrieving configuration settings.
        """
        self.completion_request = completion_request
        self.agent = completion_request.agent
        self.llm = llm
        self.config = config
                        
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
                return AnomalyDetectionAgent(self.completion_request, llm=self.llm, config=self.config)
            case 'csv':
                return CSVAgent(self.completion_request, llm=self.llm, config=self.config)
            case 'sql':
                return SqlDbAgent(self.completion_request, llm=self.llm, config=self.config)
            case 'summary':
                return SummaryAgent(self.completion_request, llm=self.llm, config=self.config)
            case 'blob':
                return BlobStorageAgent(self.completion_request, llm=self.llm, config=self.config)
            case _:
                return ConversationalAgent(self.completion_request, llm=self.llm, config=self.config)