import pytest
from unittest.mock import patch, Mock, call
from foundationallm.storage import BlobStorageManager
from azure.storage.blob import BlobProperties


@pytest.fixture
def all_blobs():
    blobs = [
        BlobProperties(name="/Datasets/Financial/Finances2021.csv"),
        BlobProperties(name="/Datasets/Financial/TaxReturns2021.csv"),
        BlobProperties(name="/Datasets/Financial/Finances2022.csv"),
        BlobProperties(name="/Datasets/Financial/TaxReturns2022.csv"),
    ]
    for blob in blobs:
        blob.content_settings.content_type = "text/csv"

    return blobs


class BlobStorageManagerTests:
    """
    DataSourceHubStorageManagerTests validates DataSourceHubStorageManager's behavior.
    """
    def test_list_blobs(self, all_blobs):
        """
        list_blobs() fetches Blobs from a virtual path and can filter them based on a provided pattern.
        
        If no Blobs are found, an empty list is returned.
        """
        with patch(
            "foundationallm.storage.blob_storage_manager.BlobServiceClient"
        ) as blob_service_client:
            container_client = Mock()
            blob_service_client.from_connection_string.return_value.get_container_client.return_value = (
                container_client
            )
            container_client.list_blobs.return_value = all_blobs
            blob_storage_manager = BlobStorageManager(
                "SOME_CONNECTION_STRING", "SOME_CONTAINER"
            )

            # No pattern matching
            assert blob_storage_manager.list_blobs("/Datasets/Financial/") == all_blobs
            # __get_full_path() is called
            container_client.list_blobs.assert_called_once_with(
                name_starts_with="Datasets/Financial"
            )

            # Pattern matching
            assert (
                len(blob_storage_manager.list_blobs("/Datasets/Financial/", "*2021*"))
                == 2
            )

    def test_file_exists(self):
        """
        file_exists() determines whether a given Blob exists in Blob Storage.
        """
        with patch(
            "foundationallm.storage.blob_storage_manager.BlobServiceClient"
        ) as blob_service_client:
            container_client = Mock()
            blob_service_client.from_connection_string.return_value.get_container_client.return_value = (
                container_client
            )
            blob_storage_manager = BlobStorageManager(
                "SOME_CONNECTION_STRING", "SOME_CONTAINER"
            )
            get_blob_client = Mock()
            container_client.get_blob_client = get_blob_client
            get_blob_client.return_value.exists.return_value = True

            assert blob_storage_manager.file_exists(
                "/Datasets/Financial/Finances2021.csv"
            )
            get_blob_client.assert_called_once_with(
                "Datasets/Financial/Finances2021.csv"
            )
            get_blob_client.return_value.exists.assert_called_once()

    def test_read_file_content(self):
        """
        read_file_content() can download a file as a stream or return the raw bytes.
        
        If the file cannot be located, the function returns None.
        """
        with patch(
            "foundationallm.storage.blob_storage_manager.BlobServiceClient"
        ) as blob_service_client:
            container_client = Mock()
            blob_service_client.from_connection_string.return_value.get_container_client.return_value = (
                container_client
            )
            blob_storage_manager = BlobStorageManager(
                "SOME_CONNECTION_STRING", "SOME_CONTAINER"
            )
            blob_client = Mock()
            container_client.get_blob_client.return_value = blob_client

            # Non-existent file
            blob_client.exists.return_value = False
            assert (
                blob_storage_manager.read_file_content(
                    "/Prompts/FinancialTeam/default.json"
                )
                is None
            )

            # Clean-up
            blob_client.exists.return_value = True

            # Read as Stream
            blob_client.download_blob.return_value.readinto.side_effect = (
                lambda stream: stream.write(b'{"Name": "Anomaly Prompt"}')
            )
            stream_read = blob_storage_manager.read_file_content(
                "/Prompts/FinancialTeam/anomaly.json", True
            )
            # Called once to verify existence and again to stream the Blob
            assert (
                sum(
                    method_call == call("Prompts/FinancialTeam/anomaly.json")
                    for method_call in container_client.get_blob_client.call_args_list
                )
                == 2
            )
            assert stream_read == b'{"Name": "Anomaly Prompt"}'

            # Read as Bytes
            container_client.download_blob.return_value.content_as_bytes.return_value = (
                b'{"Name": "DataFrame Analysis Prompt"}'
            )
            assert (
                blob_storage_manager.read_file_content(
                    "/Prompts/FinancialTeam/dataframe.json", False
                )
                == b'{"Name": "DataFrame Analysis Prompt"}'
            )
            # ContainerClient.download_blob() is different from BlobClient.download_blob()
            container_client.download_blob.assert_called_once_with(
                "Prompts/FinancialTeam/dataframe.json"
            )

    def test_write_file_content(self):
        """
        write_file_content() uploads bytes/string to the provided Blob Storage virtual path.
        
        It allows the caller to provide lease settings and indicate whether existing files should be overwritten.
        """
        with patch(
            "foundationallm.storage.blob_storage_manager.BlobServiceClient"
        ) as blob_service_client:
            container_client = Mock()
            blob_service_client.from_connection_string.return_value.get_container_client.return_value = (
                container_client
            )
            blob_storage_manager = BlobStorageManager(
                "SOME_CONNECTION_STRING", "SOME_CONTAINER"
            )
            blob_storage_manager.write_file_content(
                "/DataSources/FinancialTeam/SQLConfig.json", '{"name": "SQL Server"}'
            )
            container_client.get_blob_client.assert_called_with(
                "DataSources/FinancialTeam/SQLConfig.json"
            )
            container_client.get_blob_client.return_value.upload_blob.assert_called_with(
                '{"name": "SQL Server"}', overwrite=True, lease=None
            )

    def test_delete_file(self):
        """
        delete_file() deletes a Blob and its snapshots.
        
        It does not validate if the provided virtual path exists.
        """
        with patch(
            "foundationallm.storage.blob_storage_manager.BlobServiceClient"
        ) as blob_service_client:
            container_client = Mock()
            blob_service_client.from_connection_string.return_value.get_container_client.return_value = (
                container_client
            )
            blob_storage_manager = BlobStorageManager(
                "SOME_CONNECTION_STRING", "SOME_CONTAINER"
            )
            blob_storage_manager.delete_file(
                "/DataSources/FinancialTeam/CSVAccessConfig.json"
            )
            container_client.delete_blob.assert_called_once_with(
                "DataSources/FinancialTeam/CSVAccessConfig.json",
                delete_snapshots="include",
            )