import pytest
from unittest.mock import patch
from foundationallm.config import Configuration
from foundationallm.hubs.agent import (
    AgentRepository,
    AgentHubStorageManager
)

@pytest.fixture
def test_config():
    return Configuration()

@pytest.fixture
def agent_repository(test_config):
    return AgentRepository(config=test_config)

class AgentRepositoryTests:
    """
    AgentRepositoryTests verifies that AgentRepository appropriately fetches agent metadata
        from storage providers.
        
    This is an integration test class and expects the following environment variable to be set:
        foundationallm-app-configuration-uri.
        
    This test class also expects a valid Azure credential (DefaultAzureCredential) session.
    """
    def test_get_metadata_values(self, agent_repository):
        """
        get_metadata_values() queries all files matching the provided pattern and deserializes them.
        """
        with (
            patch.object(AgentHubStorageManager, "list_blobs") as list_blobs,
            patch.object(
                AgentHubStorageManager, "read_file_content"
            ) as read_file_content,
        ):
            list_blobs.return_value = ["Anomaly-Agent-File-Name"]
            read_file_content.return_value = '{"name": "AnomalyAgent", "description": "Responds to anomalies from the SQL DB", "type": "sql"}'
            
            # `None` is an invalid input the Blob SDK - use empty string
            agent_repository.get_metadata_values()
            list_blobs.assert_called_with(path="")

            metadata = agent_repository.get_metadata_values("Anomaly*")
            assert metadata[0].name == "AnomalyAgent"
            list_blobs.assert_called_with(path="Anomaly*")
            
    def test_get_metadata_by_name(self, agent_repository):
        """
        get_metadata_by_name() fetches a specific metadata file from the storage backend and deserializes it.
        
        It should return `None` if the file could not be located.
        """
        with (
            patch.object(AgentHubStorageManager, "file_exists") as file_exists,
            patch.object(
                AgentHubStorageManager, "read_file_content"
            ) as read_file_content,
        ):
            file_exists.side_effect = [True, False]
            read_file_content.side_effect = [
                '{"name": "Default", "description": "Default Q/A agent", "type": "conversational"}',
                None
            ]
            
            assert agent_repository.get_metadata_by_name("Default").name == "Default"
            file_exists.assert_called_with("Default.json")
            read_file_content.assert_called_with("Default.json")
            
            # Non-existent agent file
            assert agent_repository.get_metadata_by_name("Anomaly") is None
