from importlib import import_module
from logging import Logger
import os
import sys

from .external_module import ExternalModule
from .plugin_manager_types import PluginManagerTypes

from foundationallm.config import Configuration
from foundationallm.storage import BlobStorageManager

PLUGIN_MANAGER_CONFIGURATION_NAMESPACE = 'FoundationaLLM:APIEndpoints:LangChainAPI:Configuration:ExternalModules'
PLUGIN_MANAGER_STORAGE_ACCOUNT_NAME = f'{PLUGIN_MANAGER_CONFIGURATION_NAMESPACE}:Storage:AccountName'
PLUGIN_MANAGER_STORAGE_AUTHENTICATION_TYPE = f'{PLUGIN_MANAGER_CONFIGURATION_NAMESPACE}:Storage:AuthenticationType'
PLUGIN_MANAGER_STORAGE_CONTAINER = f'{PLUGIN_MANAGER_CONFIGURATION_NAMESPACE}:RootStorageContainer'
PLUGIN_MANAGER_MODULES = f'{PLUGIN_MANAGER_CONFIGURATION_NAMESPACE}:Modules'
PLUGIN_MANAGER_LOCAL_STORAGE_FOLDER_NAME = 'foundationallm_external_modules'

class PluginManager():
    """
    Manages the plugins in the system.
    """

    def __init__(self, config:Configuration, logger:Logger):
        """
        Initializes the plugin manager.

        Parameters
        ----------
        config : Configuration
            The configuration object for the system.
        logger : Logger
            The logger object used for logging.
        """
        self.config = config
        self.logger = logger
        self.external_modules: dict[str, ExternalModule] = {}
        self.modules_local_path = f'./{PLUGIN_MANAGER_LOCAL_STORAGE_FOLDER_NAME}'

        if not os.path.exists(self.modules_local_path):
            os.makedirs(self.modules_local_path)

        self.initialized = False
        valid_configuration = False

        try:
            storage_account_name = config.get_value(PLUGIN_MANAGER_STORAGE_ACCOUNT_NAME)
            storage_authentication_type = config.get_value(PLUGIN_MANAGER_STORAGE_AUTHENTICATION_TYPE)
            storage_container_name = config.get_value(PLUGIN_MANAGER_STORAGE_CONTAINER)
            modules_list = config.get_value(PLUGIN_MANAGER_MODULES)
            valid_configuration = True
        except:
            self.logger.exception('The plugin manager configuration is not set up correctly. No plugins will be loaded.')

        if valid_configuration:

            self.logger.info((
                f'Initializing plugin manager with the following configuration:\n',
                f'Storage account name:: {storage_account_name}\n',
                f'Storage authentication type: {storage_authentication_type}\n',
                f'Storage container name: {storage_container_name}\n',
                f'Modules list: {modules_list}\n',
                f'Modules local path: {self.modules_local_path}\n'
            ))

            try:

                self.storage_manager = BlobStorageManager(
                    account_name=storage_account_name,
                    container_name=storage_container_name,
                    authentication_type=storage_authentication_type
                )
                
                if modules_list is not None and modules_list.strip() != '':
                    for module_configuration in [x.split('|') for x in modules_list.split(',')]:
                        module_file = module_configuration[0]
                        module_name = module_configuration[1]
                        plugin_manager_type = module_configuration[2]
                        plugin_manager_class_name = module_configuration[3]

                        if (plugin_manager_type != PluginManagerTypes.TOOLS and plugin_manager_type != PluginManagerTypes.WORKFLOWS):
                            raise ValueError(f'The plugin manager type {plugin_manager_type} is not recognized.')

                        if module_name in self.external_modules:
                            self.external_modules[module_name].plugin_manager_class_name = plugin_manager_class_name
                        else:
                            self.external_modules[module_name] = ExternalModule(
                                module_file=module_file,
                                module_name=module_name,
                                plugin_manager_class_name=plugin_manager_class_name
                            )
                self.initialized = True
                self.logger.info('The plugin manager initialized successfully.')

            except Exception as e:
                self.logger.exception('An error occurred while initializing the plugin manager storage manager. No plugins will be loaded.')

    def load_external_modules(self):
        """
        Loads the external modules into the system.
        """
        if not self.initialized:
            self.logger.error('The plugin manager is not initialized. No plugins will be loaded.')
            return

        for module_name in self.external_modules.keys():

            external_module = self.external_modules[module_name]
            module_file_name = external_module.module_file
            local_module_file_name = f'{self.modules_local_path}/{module_file_name}'
            self.logger.info(f'Loading module from {module_file_name}')

            try:
                if (self.storage_manager.file_exists(module_file_name)):
                    self.logger.info(f'Copying module file to: {local_module_file_name}')
                    module_file_binary_content = self.storage_manager.read_file_content(module_file_name)
                    with open(local_module_file_name, 'wb') as f:
                        f.write(module_file_binary_content)

                sys.path.insert(0, local_module_file_name)
                external_module.module = import_module(external_module.module_name)

                self.logger.info(f'Module {module_name} loaded successfully.')
                external_module.module_loaded = True

                # Note the () at the end of the getattr call - this is to call the class constructor, not just get the class.
                external_module.plugin_manager = getattr(external_module.module, external_module.plugin_manager_class_name)()

            except Exception as e:
                self.logger.exception(f'An error occurred while loading module: {module_name}')

