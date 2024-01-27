from langchain_core.language_models import BaseLanguageModel

from foundationallm.config import Configuration, Context
from foundationallm.models.orchestration import CompletionRequest
from foundationallm.langchain.agents import AgentBase
from foundationallm.langchain.agents import (
    AnomalyDetectionAgent,
    CSVAgent,
    SqlDbAgent,
    SummaryAgent,
    BlobStorageAgent,
    GenericResolverAgent,
    CXOAgent,
    SearchServiceAgent
)

class AgentFactory:
    """
    Factory to determine which agent to use.
    """

    def __init__(
            self,
            completion_request: CompletionRequest,
            llm: BaseLanguageModel,
            config: Configuration,
            context: Context):
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
        self.context = context

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
                return AnomalyDetectionAgent(self.completion_request,
                                             llm=self.llm, config=self.config)
            case 'csv':
                return CSVAgent(self.completion_request, llm=self.llm,
                                config=self.config)
            case 'sql':
                return SqlDbAgent(self.completion_request, llm=self.llm,
                                  config=self.config, context=self.context)
            case 'summary':
                return SummaryAgent(self.completion_request, llm=self.llm,
                                    config=self.config)
            case 'blob-storage':
                return BlobStorageAgent(self.completion_request, llm=self.llm,
                                        config=self.config)
            case 'generic-resolver':
                return GenericResolverAgent(self.completion_request, llm=self.llm,
                                            config=self.config)
            case 'search-service':
                return SearchServiceAgent(self.completion_request, llm=self.llm,
                                           config=self.config)
            case 'cxo':
                return CXOAgent(self.completion_request,
                                             llm=self.llm, config=self.config)
            case _:
                raise ValueError(f'No agent found for the specified agent type: {self.agent.type}.')
