import pytest
from unittest.mock import patch
from foundationallm.config import Configuration
from foundationallm.hubs.data_source import (
    DataSourceRepository,
    DataSourceHubStorageManager,
)
from foundationallm.hubs.data_source.data_source_repository import SQLDataSourceMetadata


@pytest.fixture
def test_config():
    return Configuration()


@pytest.fixture
def data_source_repository(test_config):
    return DataSourceRepository(config=test_config)


class DataSourceRepositoryTests:
    """
    DataSourceRepositoryTests verifies that DataSourceRepository appropriately handles fetching agent metadata
        from storage providers (Azure Blob Storage and mocked).
        
    This is an integration test class and expects the following environment variable to be set:
        foundationallm-app-configuration-uri.
        
    This test class also expects a valid Azure credential (DefaultAzureCredential) session.
    """
    def test_anomaly_data_source_deserializes_properly(self, data_source_repository):
        """
        This test deserializes data stored in Blob Storage.
        
        It verifies that the name property is set correctly.
        """
        ds = data_source_repository.get_metadata_by_name("anomaly-ds")
        print(ds)
        assert ds.name == "anomaly-ds"

    def test_blob_data_source_has_data_description(self, data_source_repository):
        """
        This test is similar to the previous test, but verifies that the data_description field is set.
        """
        ds = data_source_repository.get_metadata_by_name("hai-ds")
        print(ds)
        assert ds.data_description == "Survey data"

    def test_get_metadata_values(self, data_source_repository):
        """
        This test asserts that get_metadata_values() deserializes data correctly and calls the necessary methods.
        
        It does not query Blob Storage.
        """
        with (
            patch.object(DataSourceHubStorageManager, "list_blobs") as list_blobs,
            patch.object(
                DataSourceHubStorageManager, "read_file_content"
            ) as read_file_content,
        ):
            # Valid JSON
            read_file_content.return_value = '{"name": "SQL", "description": "Corporate SQL DB", "underlying_implementation": "sql", "dialect": "mssql"}'

            # Custom pattern
            metadata = data_source_repository.get_metadata_values(["ds-sql"])
            # Only one valid JSON file should be returned
            assert type(metadata[0]) == SQLDataSourceMetadata
            # We supplied the pattern manually
            list_blobs.assert_not_called()

            # No blobs - return empty list
            list_blobs.return_value = []
            assert data_source_repository.get_metadata_values() == []

    def test_get_metadata_by_name(self, data_source_repository):
        """
        This test asserts that get_metadata_by_name() deserializes data correctly and calls the necessary methods.
        
        It does not query Blob Storage.
        """
        with (
            patch.object(DataSourceHubStorageManager, "list_blobs") as list_blobs,
            patch.object(
                DataSourceHubStorageManager, "read_file_content"
            ) as read_file_content,
        ):
            read_file_content.return_value = '{"name": "SQL", "description": "Corporate SQL DB", "underlying_implementation": "sql", "dialect": "mssql"}'

            assert (
                type(data_source_repository.get_metadata_by_name("ds-sql")) == SQLDataSourceMetadata
            )
            read_file_content.assert_called_with("ds-sql.json")
