import pytest
from unittest.mock import patch
from foundationallm.config import Configuration
from foundationallm.hubs.agent import AgentHubStorageManager
from foundationallm.storage import BlobStorageManager
from azure.storage.blob import BlobProperties

@pytest.fixture
def test_config():
    return Configuration()

@pytest.fixture
def agent_hub_storage_manager(test_config):
    return AgentHubStorageManager(config=test_config)

class AgentHubStorageManagerTests:
    """
    AgentHubStorageManagerTests validates AgentHubStorageManager's behavior.
        
    This is an integration test class and expects the following environment variable to be set:
        foundationallm-app-configuration-uri.
        
    This test class also expects a valid Azure credential (DefaultAzureCredential) session.
    """
    def test_list_blobs(self, agent_hub_storage_manager):
        """
        list_blobs() queries the files in the provided Blob Storage virtual path.
        
        It truncates the file path from the returned blob names.
        
        This function will raise an exception if an invalid Blob Storage path is provided.
        """
        with patch.object(BlobStorageManager, "list_blobs") as list_blobs:
            list_blobs.return_value = [BlobProperties(name="/agents/default.json")]
            assert agent_hub_storage_manager.list_blobs("/agents") == ["default.json"]
            list_blobs.assert_called_once_with(path="/agents")
