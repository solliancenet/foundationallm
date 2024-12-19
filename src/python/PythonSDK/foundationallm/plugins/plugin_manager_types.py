from enum import Enum

class PluginManagerTypes(str, Enum):
    """Enumerator of the Plugin Manager Types."""
    TOOLS = "tools"
    WORKFLOWS = "workflows"
