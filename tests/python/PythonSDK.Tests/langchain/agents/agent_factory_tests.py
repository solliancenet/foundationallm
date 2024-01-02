from unittest.mock import Mock, patch, sentinel
from foundationallm.langchain.agents import (
    AgentFactory,
    AnomalyDetectionAgent,
    BlobStorageAgent,
    CSVAgent,
    GenericResolverAgent,
    SqlDbAgent,
    SummaryAgent,
    ConversationalAgent,
)


def instantiate_completion_request(agent_type):
    completion_request = Mock()
    completion_request.agent.type = agent_type
    return completion_request


def instantiate_agent_factory(completion_request):
    return AgentFactory(
        completion_request=completion_request,
        llm=sentinel.llm,
        config=sentinel.config,
        context=sentinel.context,
    )


class AgentFactoryTests:
    """
    AgentFactoryTests verifies that AgentFactory provisions the correct agent based on the completion endpoint request.
    """

    def test_get_anomaly_agent(self):
        with patch.object(
            AnomalyDetectionAgent, "__init__", return_value=None
        ) as constructor:
            completion_request = instantiate_completion_request("anomaly")
            agent_factory = instantiate_agent_factory(completion_request)
            assert type(agent_factory.get_agent()) == AnomalyDetectionAgent
            constructor.assert_called_once_with(
                completion_request, llm=sentinel.llm, config=sentinel.config
            )

    def test_get_csv_agent(self):
        with patch.object(CSVAgent, "__init__", return_value=None) as constructor:
            completion_request = instantiate_completion_request("csv")
            agent_factory = instantiate_agent_factory(completion_request)
            assert type(agent_factory.get_agent()) == CSVAgent
            constructor.assert_called_once_with(
                completion_request, llm=sentinel.llm, config=sentinel.config
            )

    def test_get_sql_db_agent(self):
        with patch.object(SqlDbAgent, "__init__", return_value=None) as constructor:
            completion_request = instantiate_completion_request("sql")
            agent_factory = instantiate_agent_factory(completion_request)
            assert type(agent_factory.get_agent()) == SqlDbAgent
            constructor.assert_called_once_with(
                completion_request,
                llm=sentinel.llm,
                config=sentinel.config,
                context=sentinel.context,
            )

    def test_get_summary_agent(self):
        with patch.object(SummaryAgent, "__init__", return_value=None) as constructor:
            completion_request = instantiate_completion_request("summary")
            agent_factory = instantiate_agent_factory(completion_request)
            assert type(agent_factory.get_agent()) == SummaryAgent
            constructor.assert_called_once_with(
                completion_request, llm=sentinel.llm, config=sentinel.config
            )

    def test_get_blob_storage_agent(self):
        with patch.object(
            BlobStorageAgent, "__init__", return_value=None
        ) as constructor:
            completion_request = instantiate_completion_request("blob-storage")
            agent_factory = instantiate_agent_factory(completion_request)
            assert type(agent_factory.get_agent()) == BlobStorageAgent
            constructor.assert_called_once_with(
                completion_request, llm=sentinel.llm, config=sentinel.config
            )

    def test_get_generic_resolver_agent(self):
        with patch.object(
            GenericResolverAgent, "__init__", return_value=None
        ) as constructor:
            completion_request = instantiate_completion_request("generic-resolver")
            agent_factory = instantiate_agent_factory(completion_request)
            assert type(agent_factory.get_agent()) == GenericResolverAgent
            constructor.assert_called_once_with(
                completion_request, llm=sentinel.llm, config=sentinel.config
            )

    def test_get_fallback_agent(self):
        with patch.object(
            ConversationalAgent, "__init__", return_value=None
        ) as constructor:
            completion_request = instantiate_completion_request("non-existent-agent")
            agent_factory = instantiate_agent_factory(completion_request)
            assert type(agent_factory.get_agent()) == ConversationalAgent
            constructor.assert_called_once_with(
                completion_request, llm=sentinel.llm, config=sentinel.config
            )