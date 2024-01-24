import pytest
from unittest.mock import patch
from foundationallm.config import Configuration
from foundationallm.hubs.data_source import DataSourceHubStorageManager
from foundationallm.storage import BlobStorageManager
from azure.storage.blob import BlobProperties


@pytest.fixture
def test_config():
    return Configuration()


@pytest.fixture
def data_source_hub_storage_manager(test_config):
    return DataSourceHubStorageManager(config=test_config)


class DataSourceHubStorageManagerTests:
    """
    DataSourceHubStorageManagerTests validates DataSourceHubStorageManager's behavior.

    This is an integration test class and expects the following environment variable to be set:
        foundationallm-app-configuration-uri.

    This test class also expects a valid Azure credential (DefaultAzureCredential) session.
    """

    def test_list_blobs(self, data_source_hub_storage_manager):
        """
        list_blobs() queries the files in the provided Blob Storage virtual path.

        It truncates the file path from the returned blob names.

        This function assumes that a valid Blob Storage path is provided.
        """
        with patch.object(BlobStorageManager, "list_blobs") as list_blobs:
            list_blobs.return_value = [BlobProperties(name="/data_sources/sql.json")]
            assert data_source_hub_storage_manager.list_blobs() == ["sql.json"]
            list_blobs.assert_called_once_with(path="")
