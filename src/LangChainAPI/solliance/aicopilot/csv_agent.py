import os
    
from langchain.agents import create_csv_agent
from langchain.llms import OpenAI
from langchain.chat_models import ChatOpenAI
from langchain.agents.agent_types import AgentType

from .prompt_model import PromptModel

class CSVAgent:

    def __init__(self):

        self.source_csv_file = "https://4qzigl76w7ebqpromptsa.blob.core.windows.net/hai-source/surveydata.csv?sp=r&st=2023-08-01T22:04:23Z&se=2023-08-31T06:04:23Z&spr=https&sv=2022-11-02&sr=c&sig=VX2C9O8VGNkG5BAU7C3vXxaAr9lhQooMyg5xdDM%2BKgM%3D"
        self.prompt_prefix = """
You are an analytics agent named Khalil.
You help users answer their questions about survey data. If the user asks you to answer any other question besides questions about the data, politely suggest that go ask a human as you are a very focused agent.
You are working with a pandas dataframe in Python that contains the survey data. The name of the dataframe is `df`.    
You should use the tools below to answer the question posed of you:"""

        self.agent = create_csv_agent(
            OpenAI(temperature=0),
            self.source_csv_file,
            verbose=True,
            agent_type=AgentType.ZERO_SHOT_REACT_DESCRIPTION,
            prefix = self.prompt_prefix
        )

    def run(self, prompt: PromptModel):

        return {
            "info": self.agent.run(prompt.prompt)
        }
