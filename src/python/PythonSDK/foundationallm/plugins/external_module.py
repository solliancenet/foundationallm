from types import ModuleType
from typing import Union
from .workflows.workflow_plugin_manager_base import WorkflowPluginManagerBase
from .tools.tool_plugin_manager_base import ToolPluginManagerBase

class ExternalModule():
    """
    Encapsulates properties useful for configuring an external module.
        module_file: str - The name of the module file.
        module_name: str - The name of the module.
        module_loaded: bool - Indicates whether the module is loaded.
        module: ModuleType - The module object.
        plugin_manager_class_name: str - The name of the plugin manager class for the module.
        plugin_manager: Union[ToolPluginManager, WorkflowPluginManager] - The plugin manager for the module.
    """

    module_file: str
    module_name: str
    module_loaded: bool = False
    module: ModuleType = None
    plugin_manager_class_name: str = None
    plugin_manager: Union[ToolPluginManagerBase, WorkflowPluginManagerBase] = None

    def __init__(self, module_file: str, module_name: str, plugin_manager_class_name: str):
        """
        Initializes the external module.

        Parameters
        ----------
        module_file : str
            The name of the module file.
        module_name : str
            The name of the module.
        plugin_manager_class_name : str
            The name of the tool plugin manager class for the module.
        """
        self.module_file = module_file
        self.module_name = module_name
        self.plugin_manager_class_name = plugin_manager_class_name
