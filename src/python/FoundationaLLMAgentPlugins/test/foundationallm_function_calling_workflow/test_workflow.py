"""
Test for FoundationaLLM Azure OpenAI Router Workflow.
"""
import asyncio
import json
import os
import sys
import uuid

sys.path.append('src')
from foundationallm_agent_plugins import (
    FoundationaLLMAgentToolPluginManager,
    FoundationaLLMAgentWorkflowPluginManager
)
from foundationallm.config import Configuration, UserIdentity
from foundationallm.models.agents import AgentBase, AgentWorkflowBase

user_prompt = "Who is Paul?"
user_prompt_rewrite = None
operation_id = str(uuid.uuid4())

user_identity_json = {"name": "Experimental Test", "user_name":"sw@foundationaLLM.ai","upn":"sw@foundationaLLM.ai"}
full_request_json_file_name = 'test/full_request.json' # full original langchain request, contains agent, tools, exploded objects
print(os.environ['FOUNDATIONALLM_APP_CONFIGURATION_URI'])

user_identity = UserIdentity.from_json(user_identity_json)
config = Configuration()

with open(full_request_json_file_name, 'r') as f:
    request_json = json.load(f)

agent = AgentBase(**request_json["agent"])    
objects = request_json["objects"]
workflow = AgentWorkflowBase.from_object(request_json["agent"]["workflow"])

workflow_plugin_manager = FoundationaLLMAgentWorkflowPluginManager()
tool_plugin_manager = FoundationaLLMAgentToolPluginManager()

# prepare tools
tools = []
parsed_user_prompt = user_prompt

explicit_tool = next((tool for tool in agent.tools if parsed_user_prompt.startswith(f'[{tool.name}]:')), None)
if explicit_tool is not None:
    tools.append(tool_plugin_manager.create_tool(explicit_tool, objects, user_identity, config))
    parsed_user_prompt = parsed_user_prompt.split(':', 1)[1].strip()
else:
    # Populate tools list from agent configuration
    for tool in agent.tools:
        tools.append(tool_plugin_manager.create_tool(tool, objects, user_identity, config))

# create the workflow
workflow = workflow_plugin_manager.create_workflow(
    agent.workflow,
    objects,
    tools,
    user_identity,
    config
)

# Get message history
#request.objects['message_history'] = request.message_history[:agent.conversation_history_settings.max_history*2]
#if agent.conversation_history_settings.enabled:
#    messages = self._build_conversation_history_message_list(request.message_history, agent.conversation_history_settings.max_history*2)
#else:
#    messages = []
messages = []
response = asyncio.run(
    workflow.invoke_async(
        operation_id=operation_id,
        user_prompt=parsed_user_prompt,
        user_prompt_rewrite=user_prompt_rewrite,
        message_history=messages
    )
)
print(response)
