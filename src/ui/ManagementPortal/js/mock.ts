import type { AgentIndex } from './types';

export const mockGetAgentIndexesResponse: AgentIndex[] = [
	{
		Name: 'AzureAISearch_Test_001',
		Description: 'Azure AI Search index for vectorization testing.',
		Indexer: 'AzureAISearchIndexer',
		Settings: {
			IndexName: 'fllm-test-001',
		},
		ConfigurationReferences: {
			APIKey: 'FoundationaLLM:Vectorization:AzureAISearchIndexingService:APIKey',
			AuthenticationType: 'FoundationaLLM:Vectorization:AzureAISearchIndexingService:AuthenticationType',
			Endpoint: 'FoundationaLLM:Vectorization:AzureAISearchIndexingService:Endpoint',
		},
	},
	{
		Name: 'sotu-index',
		Description: 'Azure AI Search index for the State of the Union agent.',
		Indexer: 'AzureAISearchIndexer',
		Settings: {
			IndexName: 'sotu',
			TopN: '3',
			Filters: '[]',
			EmbeddingFieldName: 'Embedding',
			TextFieldName: 'Text',
		},
		ConfigurationReferences: {
			APIKey: 'FoundationaLLM:Vectorization:AzureAISearchIndexingService:APIKey',
			AuthenticationType: 'FoundationaLLM:Vectorization:AzureAISearchIndexingService:AuthenticationType',
			Endpoint: 'FoundationaLLM:Vectorization:AzureAISearchIndexingService:Endpoint',
		},
	},
];

export const mockGetAgentDataSourcesResponse: AgentIndex[] = [
	{
		Name: 'AzureBlob_DataSource_1',
		Container: {
			Name: 'documents',
			Formats: [
				'pdf',
			],
		},
		Description: 'Azure AI Search index for vectorization testing.',
		Indexer: 'AzureAISearchIndexer',
		Settings: {
			IndexName: 'fllm-test-001',
		},
		ConfigurationReferences: {
			APIKey: 'FoundationaLLM:Vectorization:AzureAISearchIndexingService:APIKey',
			AuthenticationType: 'FoundationaLLM:Vectorization:AzureAISearchIndexingService:AuthenticationType',
			Endpoint: 'FoundationaLLM:Vectorization:AzureAISearchIndexingService:Endpoint',
		},
	},
	{
		Name: 'AzureBlob_DataSource_2',
		Container: {
			Name: 'census_data',
			Formats: [
				'pdf',
				'txt',
				'doc',
			],
		},
		Description: 'Azure AI Search index for the State of the Union agent.',
		Indexer: 'AzureAISearchIndexer',
		Settings: {
			IndexName: 'sotu',
			TopN: '3',
			Filters: '[]',
			EmbeddingFieldName: 'Embedding',
			TextFieldName: 'Text',
		},
		ConfigurationReferences: {
			APIKey: 'FoundationaLLM:Vectorization:AzureAISearchIndexingService:APIKey',
			AuthenticationType: 'FoundationaLLM:Vectorization:AzureAISearchIndexingService:AuthenticationType',
			Endpoint: 'FoundationaLLM:Vectorization:AzureAISearchIndexingService:Endpoint',
		},
	},
];
