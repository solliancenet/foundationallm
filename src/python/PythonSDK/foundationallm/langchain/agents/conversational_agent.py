from langchain.agents import AgentType, initialize_agent
from langchain.memory import ConversationBufferMemory
from langchain_community.callbacks import get_openai_callback
from langchain_community.tools import DuckDuckGoSearchRun
from langchain_core.tools import Tool

from foundationallm.config import Configuration
from foundationallm.langchain.agents import AgentBase
from foundationallm.langchain.language_models import LanguageModelBase
from foundationallm.models.orchestration import CompletionRequest, CompletionResponse

class ConversationalAgent(AgentBase):
    """
    Default agent with basic conversational capabilities.
    """

    def __init__(
            self,
            completion_request: CompletionRequest,
            llm: LanguageModelBase,
            config: Configuration):
        """
        Initializes a DefaultAgent

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
        self.message_history = completion_request.message_history
        self.llm = llm.get_completion_model(completion_request.language_model)

        self.search = DuckDuckGoSearchRun()
        self.tools = [
            Tool(
                name="Current Search",
                func=self.search.run,
                description="useful for when you need to answer questions about current events or the current state of the world"
            ),
        ]

        self.memory = ConversationBufferMemory(memory_key="chat_history", return_messages=True)

        self.agent = initialize_agent(
            tools = self.tools,
            llm = self.llm,
            agent = AgentType.CONVERSATIONAL_REACT_DESCRIPTION,
            verbose = True,
            agent_kwargs={
                'prefix': self.prompt_prefix
            },
            memory = self.memory
        )

    @property
    def prompt_template(self) -> str:
        """
        Property for viewing the agent's prompt template.
        
        Returns
        str
            Returns the prompt template for the agent.
        """
        return self.agent.agent.llm_chain.prompt.template

    def run(self, prompt: str) -> CompletionResponse:
        """
        Executes a completion request using a default agent.

        Parameters
        ----------
        prompt : str
            The prompt for which a completion is begin generated.
        
        Returns
        -------
        CompletionResponse
            Returns a CompletionResponse with the generated completion, the user_prompt,
            and token utilization and execution cost details.
        """
        with get_openai_callback() as cb:
            return CompletionResponse(
                completion = self.agent.run(prompt),
                user_prompt= prompt,
                completion_tokens = cb.completion_tokens,
                prompt_tokens = cb.prompt_tokens,
                total_tokens = cb.total_tokens,
                total_cost = cb.total_cost
            )
