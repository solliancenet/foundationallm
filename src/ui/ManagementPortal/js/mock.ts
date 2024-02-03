import type { AgentIndex } from './types';

export const mockGetAgentIndexesResponse: AgentIndex[] = [
	{
		Name: 'AzureAISearch_Test_001',
		ObjectId: '47893247',
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
		ObjectId: '25637942',
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
		ObjectId: '90871234981',
		Type: 'AzureDataLake',
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
		ObjectId: '8931729038',
		Type: 'SharePointOnline',
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
	{
		Name: 'AzureBlob_DataSource_3',
		ObjectId: '12873989',
		Type: 'AzureDataLake',
		Container: {
			Name: 'data',
			Formats: [
				'txt',
			],
		},
	}
];
