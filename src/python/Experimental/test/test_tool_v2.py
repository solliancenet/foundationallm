import asyncio
import json
import os
import sys

from azure.identity import DefaultAzureCredential, get_bearer_token_provider

from langchain_core.messages import HumanMessage, AIMessage
from langchain_openai import AzureChatOpenAI
from langgraph.prebuilt import create_react_agent

from foundationallm.config import Configuration, UserIdentity
from foundationallm.models.agents import AgentTool
from foundationallm.telemetry import Telemetry


config = Configuration()
# Telemetry.configure_monitoring(config, f'FoundationaLLM:APIEndpoints:LangChainAPI:Essentials:AppInsightsConnectionString', __name__)

sys.path.append('../src')
from skunkworks_foundationallm import SkunkworksToolPluginManager # type: ignore

user_identity_json = {"name": "Skunkworks Test", "user_name":"sw@foundationaLLM.ai","upn":"sw@foundationaLLM.ai"}
agent_tool_json_file_name = 'skunkworks_agent_data_analysis_tool.json'
agent_prompt_json_file_name = 'skunkworks_agent_prompt.json'
exploded_objects_json_file_name = 'skunkworks_agent_exploded_objects.json'
print(os.environ['FOUNDATIONALLM_APP_CONFIGURATION_URI'])

user_identity = UserIdentity.from_json(user_identity_json)

with open(agent_tool_json_file_name, 'r') as f:
    agent_tool_json = json.load(f)
    agent_tool = AgentTool(**agent_tool_json)

with open(exploded_objects_json_file_name, 'r') as f:
    exploded_objects_json = json.load(f)

with open(agent_prompt_json_file_name, 'r') as f:
    agent_prompt_json = json.load(f)

skunkworks_tool_plugin_manager = SkunkworksToolPluginManager()
skunkworks_tool = skunkworks_tool_plugin_manager.create_tool(agent_tool, exploded_objects_json, user_identity, config)

# response = asyncio.run(skunkworks_tool._arun('Who are you?'))
# print(response)

llm_endpoint_url = 'https://openai-ftpisrjz2rvjc.openai.azure.com/'
llm_endpoint_version = '2024-07-01-preview'
llm_deployment_name = 'completions4o'
scope = 'https://cognitiveservices.azure.com/.default'
# Set up a Azure AD token provider.
token_provider = get_bearer_token_provider(
    DefaultAzureCredential(exclude_environment_credential=True),
    scope
)

llm = AzureChatOpenAI(
    azure_endpoint=llm_endpoint_url,
    api_version=llm_endpoint_version,
    openai_api_type='azure_ad',
    azure_ad_token_provider=token_provider,
    azure_deployment=llm_deployment_name,
    temperature=0.5,
    top_p=0.5
)

graph = create_react_agent(
    llm,
    tools=[skunkworks_tool],
    state_modifier=agent_prompt_json['prefix'])

user_prompt = "How is Miami performing?"
inputs = {'messages': [HumanMessage(content=user_prompt)]}

response = asyncio.run(
    graph.ainvoke(
        inputs,
        config={ 'configurable': { 'original_user_prompt': user_prompt }}))

def print_stream(stream):
    for s in stream:
        message = s["messages"][-1]
        if isinstance(message, tuple):
            print(message)
        else:
            message.pretty_print()

print_stream(graph.stream(inputs, stream_mode="values"))