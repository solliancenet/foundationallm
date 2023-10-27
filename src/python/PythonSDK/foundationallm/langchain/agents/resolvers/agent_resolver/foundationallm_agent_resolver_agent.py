from typing import List
from foundationallm.langchain.agents import AgentBase
from foundationallm.hubs.agent import AgentHubRequest, AgentMetadata
from foundationallm.models.orchestration import OrchestrationResponse
from foundationallm.config import Configuration
from foundationallm.langchain.agents.resolvers import ResolverConfigurationRepository
from foundationallm.langchain.message_history import build_message_history
from langchain.callbacks import get_openai_callback
from foundationallm.models.orchestration import CompletionResponse
from .foundationallm_agent_resolver_agent_output_parser import FoundationaLLMAgentResolverAgentOutputParser

class FoundationaLLMAgentResolverAgent(AgentBase):
    """
    The FoundationaLLMAgentResolverAgent is responsible for identifying the best-fit agent FoundationaLLM agent to 
        handle incoming user prompt. This agent evaluates the metadata of all configured agents in the system and
        uses the description of the agent to determine the best-fit.
    """
    def __init__(self, agent_request: AgentHubRequest, agent_metadata_list: List[AgentMetadata], config: Configuration):
        self.request = agent_request
        self.agent_metadata_list = agent_metadata_list
        resolver_repo = ResolverConfigurationRepository(config)        
        self.llm = resolver_repo.get_agent_resolver_llm_details()
        # prompt template expects agents(list), history and user_prompt as inputs
        prompt_template = resolver_repo.get_agent_resolver_prompt()
        agents = self.build_agent_choices_list()
        message_history = build_message_history(agent_request.message_history)
        if len(message_history) == 0:
            message_history = "Message History:\n\nNo message history available."
        self.formatted_prompt = prompt_template.format(agents=agents, history=message_history, user_prompt=agent_request.user_prompt)
       
    def build_agent_choices_list(self) -> str:
        """
        Builds a list of agent names and their descriptions for the resolver prompt.
        """
        if self.agent_metadata_list is None or len(self.agent_metadata_list)==0:
            return ""        
        agent_list = "\n\nAgent List:\n"
        for agent in self.agent_metadata_list:        
            agent_list +=  "Agent name: " + agent.name + "\n"
            agent_list +=  "Agent description: " + agent.description + "\n\n"        
        return agent_list
    
    def run(self, prompt: str) -> OrchestrationResponse:
        """
        Executes a prompt to evaluate a list of agents against the incoming user prompt.

        Parameters
        ----------
        prompt : str
            The prompt that contains agent information, message history, and user prompt.
        
        Returns
        -------
        CompletionResponse
            Returns a CompletionResponse with the name of the agent, the user_prompt,
            and token utilization and execution cost details.
        """
        try:           
            with get_openai_callback() as cb:                
                full_completion = self.llm(prompt=self.formatted_prompt)                
                parser = FoundationaLLMAgentResolverAgentOutputParser()
                completion = parser.parse(full_completion)               
                return CompletionResponse(
                    completion = completion,
                    user_prompt = prompt,
                    completion_tokens = cb.completion_tokens,
                    prompt_tokens = cb.prompt_tokens,
                    total_tokens = cb.total_tokens,
                    total_cost = cb.total_cost
                )
        except Exception as e:
            print(e)
            return CompletionResponse(
                    completion = "A problem on my side prevented me from responding.",
                    user_prompt = prompt
                ) 