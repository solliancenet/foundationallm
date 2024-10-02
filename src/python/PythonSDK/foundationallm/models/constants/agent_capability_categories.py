from enum import Enum

class AgentCapabilityCategories(str, Enum):
    """Enumerator of the Agent Capability Categories."""
    OPENAI_ASSISTANTS = "OpenAI.Assistants"
    TOOL_CALLING_AGENT = "LangChain.ToolCallingAgent"
    FOUNDATIONALLM_KNOWLEDGE_MANAGEMENT = "FoundationaLLM.KnowledgeManagement"
