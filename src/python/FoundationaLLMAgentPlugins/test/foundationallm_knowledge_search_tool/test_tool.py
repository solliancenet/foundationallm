# Ensure the terminal working directory is FoundationaLLMAgentPlugins.
# Note: This tests a tool directly and ignores the workflow.
import asyncio
import json
import os
import sys
from langchain_core.messages import AIMessage, HumanMessage
from langgraph.prebuilt import create_react_agent
from foundationallm.config import Configuration, UserIdentity
from foundationallm.langchain.language_models import LanguageModelFactory
from foundationallm.models.agents import AgentBase, AgentTool, AgentWorkflowBase
from foundationallm.models.constants import (
    AIModelResourceTypeNames,
    PromptResourceTypeNames,
    ResourceObjectIdPropertyNames,
    ResourceObjectIdPropertyValues,
    ResourceProviderNames)
from foundationallm.models.resource_providers.prompts import MultipartPrompt
from foundationallm.utils import ObjectUtils

sys.path.append('src')
from foundationallm_agent_plugins import FoundationaLLMAgentToolPluginManager # type: ignore

user_identity_json = {"name": "Experimental Test", "user_name":"sw@foundationaLLM.ai","upn":"sw@foundationaLLM.ai"}
full_request_json_file_name = 'test/full_request.json' # full original langchain request, contains agent, tools, exploded objects
print(os.environ['FOUNDATIONALLM_APP_CONFIGURATION_URI'])

user_identity = UserIdentity.from_json(user_identity_json)
config = Configuration()

with open(full_request_json_file_name, 'r') as f:
    request_json = json.load(f)

agent = AgentBase(**request_json["agent"])    
agent_tool = AgentTool(**request_json["agent"]["tools"][0])
exploded_objects_json = request_json["objects"]
workflow = AgentWorkflowBase.from_object(request_json["agent"]["workflow"])

foundationallmagent_tool_plugin_manager = FoundationaLLMAgentToolPluginManager()
# The AgentTool has the configured description the LLM will use to make a tool choice.
knowledgesearch_tool = foundationallmagent_tool_plugin_manager.create_tool(agent_tool, exploded_objects_json, user_identity, config)

#-------------------------------------------------------------------------------
# Direct tool invocation
response, content_artifacts = asyncio.run(knowledgesearch_tool._arun('Who is Paul?'))
print("**** RESPONSE ****")
print(response)
print("**** CONTENT ARTIFACTS ****")
print(content_artifacts)
print("DONE")

#-------------------------------------------------------------------------------
# Using LangGraph ReAct Agent
#ai_model_object_properties = workflow.get_resource_object_id_properties(
#    ResourceProviderNames.FOUNDATIONALLM_AIMODEL,
#    AIModelResourceTypeNames.AI_MODELS,
#    ResourceObjectIdPropertyNames.OBJECT_ROLE,
#    ResourceObjectIdPropertyValues.MAIN_MODEL
#)        
#ai_model_object_id = ai_model_object_properties.object_id        
#prompt_object_properties = workflow.get_resource_object_id_properties(
#    ResourceProviderNames.FOUNDATIONALLM_PROMPT,
#    PromptResourceTypeNames.PROMPTS,
#    ResourceObjectIdPropertyNames.OBJECT_ROLE,
#    ResourceObjectIdPropertyValues.MAIN_PROMPT
#)
#prompt_object_id = prompt_object_properties.object_id
#prompt = ObjectUtils.get_object_by_id(prompt_object_id, exploded_objects_json, MultipartPrompt)        
#language_model_factory = LanguageModelFactory(exploded_objects_json, config)        
#llm = language_model_factory.get_language_model(ai_model_object_id)
#
#user_prompt = "In the Dune books, who is Paul?"
##user_prompt = "What is 2 + 2 ? Answer like you were explaining it to a kindergartner."
#tools = [knowledgesearch_tool]
#
#graph = create_react_agent(llm, tools=tools, state_modifier=prompt.prefix)
#if agent.conversation_history_settings.enabled:
#    # Create a conversation history
#    messages = [        
#        HumanMessage(content='What is the value of Pi?'),
#        AIMessage(content='The value of Pi is 3.14.\n'), 
#        HumanMessage(content='What is the term for a word that is the same backwards as forwards?'),
#        AIMessage(content='That would be a palindrome.\n')
#    ]    
#else:
#    messages = []
#messages.append(HumanMessage(content=user_prompt))
#response = asyncio.run(
#    graph.ainvoke(
#    {'messages': messages},
#    config={"configurable": {"original_user_prompt": user_prompt, **({"recursion_limit": getattr(agent.workflow, 'graph_recursion_limit', 10)} if hasattr(agent.workflow, 'graph_recursion_limit') else {})}}
#    )
#)
##print("**** RESPONSE ****")
##print(response)
#last_message = response['messages'][-1]
#print("**** LAST MESSAGE ****")
#print(last_message.content)
#print("DONE")