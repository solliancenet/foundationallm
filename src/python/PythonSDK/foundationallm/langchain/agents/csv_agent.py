# import os
    
# from langchain.agents import create_csv_agent
# from langchain.llms import OpenAI
# from langchain.chat_models import ChatOpenAI
# from langchain.agents.agent_types import AgentType
# from pydantic import BaseModel


# from foundationallm.models import

# class CSVAgent(BaseModel):
#     """
#     Create a LangChain CSV agent for querying the contents of a comma-separated values file.
#     """
    
#     def __init__(self, source_csv_file_url):

#         self.source_csv_file = source_csv_file_url
#         self.prompt_prefix = """
# You are an analytics agent named Khalil.
# You help users answer their questions about survey data. If the user asks you to answer any other question besides questions about the data, politely suggest that go ask a human as you are a very focused agent.
# You are working with a pandas dataframe in Python that contains the survey data. The name of the dataframe is `df`.    
# You should use the tools below to answer the question posed of you:"""

#         self.agent = create_csv_agent(
#             OpenAI(temperature=0),
#             self.source_csv_file,
#             verbose=True,
#             agent_type=AgentType.ZERO_SHOT_REACT_DESCRIPTION,
#             prefix = self.prompt_prefix
#         )

#     def run(self, prompt: Prompt):

#         return {
#             "info": self.agent.run(prompt.prompt)
#         }
