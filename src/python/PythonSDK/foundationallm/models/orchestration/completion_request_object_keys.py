from enum import Enum

class CompletionRequestObjectKeys(str, Enum):
   """Enumerator of the Completion Request Object Keys."""
   AZUREAI_AGENT_ID = "AzureAI.AgentService.Agent.Id"
   AZUREAI_AGENT_THREAD_ID = "AzureAI.AgentService.Thread.Id"
   OPENAI_ASSISTANT_ID = "OpenAI.Assistants.Assistant.Id"
   OPENAI_THREAD_ID = "OpenAI.Assistants.Thread.Id"
   GATEWAY_API_ENDPOINT_CONFIGURATION = "GatewayAPIEndpointConfiguration"
   INSTANCE_ID = "FoundationaLLM.InstanceId"
   CONTEXT_API_ENDPOINT_CONFIGURATION = "ContextAPIEndpointConfiguration"
