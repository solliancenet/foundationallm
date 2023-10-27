import json
import re
from typing import Union
from langchain.agents.agent import AgentOutputParser
from langchain.schema import AgentAction, AgentFinish, OutputParserException

class FoundationaLLMAgentResolverAgentOutputParser(AgentOutputParser):
    """Output parser for the Agent Resolver Agent output."""
    
    def parse(self, text: str) -> str:
        """
        The first line of the response is the agent name. The prompt already
            includes the Final Answer:label.
        If not found, return the default value.
        otherwise, return the parsed final answer from the completion.
        """        
        final_answer = text.split("\n")[0].strip()       
        return final_answer
          

