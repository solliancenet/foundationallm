import pytest
from unittest.mock import patch, call
from foundationallm.config import Configuration
from foundationallm.hubs.prompt import PromptRepository, PromptHubStorageManager


@pytest.fixture
def test_config():
    return Configuration()


@pytest.fixture
def prompt_repository(test_config):
    return PromptRepository(config=test_config)


class PromptRepositoryTests:
    """
    PromptRepositoryTests validates PromptRepository's behavior.
    
    This is an integration test class and expects the following environment variable to be set:
        foundationallm-app-configuration-uri.
    """
    def test_get_metadata_by_name(self, prompt_repository):
        """
        get_metadata_by_name() accepts a hierarchical prompt name and pulls the relevant file from Blob Storage.

        If neither a prompt nor a prompt suffix can be located, the function throws ValueError.
        """
        with patch.object(PromptHubStorageManager, "read_file_content") as read_file_content:
            read_file_content.side_effect = [None, None, "You are an agent designed to detect anomalies.", None]
            with pytest.raises(ValueError) as value_error_info:
                prompt_repository.get_metadata_by_name("prompts.anomaly.default")
                assert value_error_info.value.args[0] == "Prompt 'prompts.anomaly.default' not found."
            # non-null prompt_prefix, null prompt_suffix
            prompt_metadata = prompt_repository.get_metadata_by_name("default")
            assert prompt_metadata.name == "default"
            assert prompt_metadata.prompt_prefix == "You are an agent designed to detect anomalies."
            assert prompt_metadata.prompt_suffix is None
            read_file_content.assert_has_calls([
                call("prompts/anomaly/default.txt"),
                call("prompts/anomaly/default_suffix.txt"),
                call("default.txt"),
                call("default_suffix.txt")
            ])
