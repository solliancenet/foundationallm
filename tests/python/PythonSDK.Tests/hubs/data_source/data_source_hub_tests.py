import pytest
from foundationallm.hubs.data_source import DataSourceHub

@pytest.fixture
def data_source_hub():
    return DataSourceHub()
    
class DataSourceHubTests:
    """
    DataSourceHubTests is responsible for testing the listing of data sources to respond to a user prompt 
        with the DataSourceHub acting as the system under test.
        
    This is an integration test class and expects the following environment variable to be set:
        foundationallm-app-configuration-uri
        
    This test class also expects a valid Azure credential (DefaultAzureCredential) session.
    """
    def test_list_method_returns_at_least_one_data_source(self, data_source_hub):
        """
        The lightweight agent consists of a dictionary containing the name and description of the data source ONLY.
        
        While this test is using the DataSourceHub as the system under test, it is in fact testing the
        HubBase ABC class where the generic list method is implemented.
        """
        agents_list = data_source_hub.list()
        assert len(agents_list) > 0
        
    def test_list_method_contains_about_fllm_data_source(self, data_source_hub):
        """
        While this test is using the DataSourceHub as the system under test, it is in fact testing the
        HubBase ABC class where the generic list method is implemented.
        """
        agent_name_list = [x["name"] for x in data_source_hub.list()]
        assert "about-foundationallm" in agent_name_list
        