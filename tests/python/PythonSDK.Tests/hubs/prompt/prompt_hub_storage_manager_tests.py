import pytest
from unittest.mock import patch
from foundationallm.config import Configuration
from foundationallm.hubs.prompt import PromptHubStorageManager
from foundationallm.storage import BlobStorageManager
from azure.storage.blob import BlobProperties


@pytest.fixture
def test_config():
    return Configuration()


@pytest.fixture
def prompt_hub_storage_manager(test_config):
    return PromptHubStorageManager(config=test_config)


class PromptHubStorageManagerTests:
    """
    PromptHubStorageManagerTests validates PromptHubStorageManager's behavior.

    This is an integration test class and expects the following environment variable to be set:
        foundationallm-app-configuration-uri.
    """

    def test_list_blobs(self, prompt_hub_storage_manager):
        """
        list_blobs() queries the files in the provided Blob Storage virtual path.

        It truncates the file path from the returned blob names.

        This function assumes that a valid Blob Storage path is provided.
        """
        with patch.object(BlobStorageManager, "list_blobs") as list_blobs:
            list_blobs.return_value = [BlobProperties(name="/prompts/default/default.txt")]
            assert prompt_hub_storage_manager.list_blobs("/prompts/default/") == ["/prompts/default/default.txt"]
            list_blobs.assert_called_once_with(path="/prompts/default/")
