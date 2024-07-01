import type { AgentIndex, AzureDataLakeDataSource } from './types';

export const mockGetAgentIndexesResponse: AgentIndex[] = [
	{
		name: 'AzureAISearch_Test_001',
		object_id: '47893247',
		description: 'Azure AI Search index for vectorization testing.',
		indexer: 'AzureAISearchIndexer',
		settings: {
			IndexName: 'fllm-test-001',
		},
		configuration_references: {
			APIKey: 'FoundationaLLM:Vectorization:AzureAISearchIndexingService:APIKey',
			AuthenticationType:
				'FoundationaLLM:Vectorization:AzureAISearchIndexingService:AuthenticationType',
			Endpoint: 'FoundationaLLM:Vectorization:AzureAISearchIndexingService:Endpoint',
		},
	},
	{
		name: 'sotu-index',
		object_id: '25637942',
		description: 'Azure AI Search index for the State of the Union agent.',
		indexer: 'AzureAISearchIndexer',
		settings: {
			IndexName: 'sotu',
			TopN: '3',
			Filters: '[]',
			EmbeddingFieldName: 'Embedding',
			TextFieldName: 'Text',
		},
		configuration_references: {
			APIKey: 'FoundationaLLM:Vectorization:AzureAISearchIndexingService:APIKey',
			AuthenticationType:
				'FoundationaLLM:Vectorization:AzureAISearchIndexingService:AuthenticationType',
			Endpoint: 'FoundationaLLM:Vectorization:AzureAISearchIndexingService:Endpoint',
		},
	},
];

export const mockGetAgentDataSourcesResponse: AgentIndex[] = [
	{
		name: 'AzureBlob_DataSource_1',
		object_id: '90871234981',
		Type: 'AzureDataLake',
		Container: {
			Name: 'documents',
			Formats: ['pdf'],
		},
		description: 'Azure AI Search index for vectorization testing.',
		indexer: 'AzureAISearchIndexer',
		settings: {
			IndexName: 'fllm-test-001',
		},
		configuration_references: {
			APIKey: 'FoundationaLLM:Vectorization:AzureAISearchIndexingService:APIKey',
			AuthenticationType:
				'FoundationaLLM:Vectorization:AzureAISearchIndexingService:AuthenticationType',
			Endpoint: 'FoundationaLLM:Vectorization:AzureAISearchIndexingService:Endpoint',
		},
	},
	{
		name: 'AzureBlob_DataSource_2',
		object_id: '8931729038',
		Type: 'SharePointOnline',
		Container: {
			Name: 'census_data',
			Formats: ['pdf', 'txt', 'doc'],
		},
		description: 'Azure AI Search index for the State of the Union agent.',
		indexer: 'AzureAISearchIndexer',
		settings: {
			IndexName: 'sotu',
			TopN: '3',
			Filters: '[]',
			EmbeddingFieldName: 'Embedding',
			TextFieldName: 'Text',
		},
		configuration_references: {
			APIKey: 'FoundationaLLM:Vectorization:AzureAISearchIndexingService:APIKey',
			AuthenticationType:
				'FoundationaLLM:Vectorization:AzureAISearchIndexingService:AuthenticationType',
			Endpoint: 'FoundationaLLM:Vectorization:AzureAISearchIndexingService:Endpoint',
		},
	},
	{
		name: 'AzureBlob_DataSource_3',
		object_id: '12873989',
		Type: 'AzureDataLake',
		Container: {
			Name: 'data',
			Formats: ['txt'],
		},
	},
];

export const mockAzureDataLakeDataSource1: AzureDataLakeDataSource = {
	type: 'azure-data-lake',
	name: 'mock-azure-data-lakehouse-source-1',
	object_id: 'FHIERKHFKJER-FBREHBFJKER-FGIHREFKVJLK',
	description: 'A mock azure data lake data source with a ConnectionString.',
	configuration_references: {
		AuthenticationType: 'ConnectionString',
		ConnectionString: 'connection-string',
		APIKey: '',
		Endpoint: '',
	},
};

export const mockAzureDataLakeDataSource2: AzureDataLakeDataSource = {
	type: 'azure-data-lake',
	name: 'mock-azure-data-lakehouse-source-2',
	object_id: 'FULHFILERF-FERHJLKFER-BFEIRUFBLKHER',
	description: 'A mock azure data lake data source with an AccountKey.',
	configuration_references: {
		AuthenticationType: 'AccountKey',
		ConnectionString: '',
		APIKey: 'this-is-not-a-real-key',
		Endpoint: 'https://solliance.com',
	},
};

export const mockRoles = [
	{
		"type": "FoundationaLLM.Authorization/roleDefinitions",
		"name": "17ca4b59-3aee-497d-b43b-95dd7d916f99",
		"object_id": "/providers/FoundationaLLM.Authorization/roleDefinitions/17ca4b59-3aee-497d-b43b-95dd7d916f99",
		"display_name": "Role Based Access Control Administrator",
		"description": "Manage access to FoundationaLLM resources by assigning roles using FoundationaLLM RBAC.",
		"assignable_scopes": [
			"/"
		],
		"permissions": [
			{
				"actions": [
					"FoundationaLLM.Authorization/roleAssignments/read",
					"FoundationaLLM.Authorization/roleAssignments/write",
					"FoundationaLLM.Authorization/roleAssignments/delete"
				],
				"not_actions": [],
				"data_actions": [],
				"not_data_actions": []
			}
		],
		"created_on": "2024-03-07T00:00:00.0000000Z",
		"updated_on": "2024-03-07T00:00:00.0000000Z",
		"created_by": null,
		"updated_by": null
	},
	{
		"type": "FoundationaLLM.Authorization/roleDefinitions",
		"name": "00a53e72-f66e-4c03-8f81-7e885fd2eb35",
		"object_id": "/providers/FoundationaLLM.Authorization/roleDefinitions/00a53e72-f66e-4c03-8f81-7e885fd2eb35",
		"display_name": "Reader",
		"description": "View all resources without the possiblity of making any changes.",
		"assignable_scopes": [
			"/"
		],
		"permissions": [
			{
				"actions": [
					"*/read"
				],
				"not_actions": [],
				"data_actions": [],
				"not_data_actions": []
			}
		],
		"created_on": "2024-03-07T00:00:00.0000000Z",
		"updated_on": "2024-03-07T00:00:00.0000000Z",
		"created_by": null,
		"updated_by": null
	},
	{
		"type": "FoundationaLLM.Authorization/roleDefinitions",
		"name": "a9f0020f-6e3a-49bf-8d1d-35fd53058edf",
		"object_id": "/providers/FoundationaLLM.Authorization/roleDefinitions/a9f0020f-6e3a-49bf-8d1d-35fd53058edf",
		"display_name": "Contributor",
		"description": "Full access to manage all resources without the possiblity of assigning roles in FoundationaLLM RBAC.",
		"assignable_scopes": [
			"/"
		],
		"permissions": [
			{
				"actions": [
					"*"
				],
				"not_actions": [
					"FoundationaLLM.Authorization/*/write",
					"FoundationaLLM.Authorization/*/delete"
				],
				"data_actions": [],
				"not_data_actions": []
			}
		],
		"created_on": "2024-03-07T00:00:00.0000000Z",
		"updated_on": "2024-03-07T00:00:00.0000000Z",
		"created_by": null,
		"updated_by": null
	},
	{
		"type": "FoundationaLLM.Authorization/roleDefinitions",
		"name": "fb8e0fd0-f7e2-4957-89d6-19f44f7d6618",
		"object_id": "/providers/FoundationaLLM.Authorization/roleDefinitions/fb8e0fd0-f7e2-4957-89d6-19f44f7d6618",
		"display_name": "User Access Administrator",
		"description": "Manage access to FoundationaLLM resources.",
		"assignable_scopes": [
			"/"
		],
		"permissions": [
			{
				"actions": [
					"*/read",
					"FoundationaLLM.Authorization/*"
				],
				"not_actions": [],
				"data_actions": [],
				"not_data_actions": []
			}
		],
		"created_on": "2024-03-07T00:00:00.0000000Z",
		"updated_on": "2024-03-07T00:00:00.0000000Z",
		"created_by": null,
		"updated_by": null
	},
	{
		"type": "FoundationaLLM.Authorization/roleDefinitions",
		"name": "1301f8d4-3bea-4880-945f-315dbd2ddb46",
		"object_id": "/providers/FoundationaLLM.Authorization/roleDefinitions/1301f8d4-3bea-4880-945f-315dbd2ddb46",
		"display_name": "Owner",
		"description": "Full access to manage all resources, including the ability to assign roles in FoundationaLLM RBAC.",
		"assignable_scopes": [
			"/"
		],
		"permissions": [
			{
				"actions": [
					"*"
				],
				"not_actions": [],
				"data_actions": [],
				"not_data_actions": []
			}
		],
		"created_on": "2024-03-07T00:00:00.0000000Z",
		"updated_on": "2024-03-07T00:00:00.0000000Z",
		"created_by": null,
		"updated_by": null
	}
];

export const mockRoleAssignmentsResponse = {
	"instance_id": "6fa496ce-d5c0-4e02-9223-06a98c9c0176",
	"role_assignments": [
		{
			"type": "FoundationaLLM.Authorization/roleAssignments",
			"name": "9c04b0-c2a9-4c55-ac8f-09ba13351ab9",
			"object_id": "/providers/FoundationaLLM.Authorization/roleAssignments/9c0453b0-c2a9-4c55-ac8f-09ba13351ab9",
			"display_name": 'Example 1',
			"description": "Contributor role on the FoundationaLLM instance.",
			"role_definition_id": "/providers/FoundationaLLM.Authorization/roleDefinitions/a9f0020f-6e3a-49bf-8d1d-35fd53058edf",
			"principal_id": "example-fe23ef05-5c5b-4a75-b59d-bba70129ece8",
			"principal_type": "Group",
			"scope": "/instances/6fa496ce-d5c0-4e02-9223-06a98c9c0176",
			"cost_center": null,
			"created_on": "2024-05-23T14:27:29.5114963+00:00",
			"updated_on": "2024-05-23T14:27:29.5114963+00:00",
			"created_by": "SYSTEM",
			"updated_by": "SYSTEM",
			"deleted": false
		},
		{
			"type": "FoundationaLLM.Authorization/roleAssignments",
			"name": "f11b77-6b2a-4b81-9e75-8b737a6ad81e",
			"object_id": "/providers/FoundationaLLM.Authorization/roleAssignments/f11bab77-6b2a-4b81-9e75-8b737a6ad81e",
			"display_name": 'Example 2',
			"description": "User Access Administrator role on the FoundationaLLM instance.",
			"role_definition_id": "/providers/FoundationaLLM.Authorization/roleDefinitions/fb8e0fd0-f7e2-4957-89d6-19f44f7d6618",
			"principal_id": "example-23413e7f-825a-4abf-a7db-3a0b6bdd166e",
			"principal_type": "Group",
			"scope": "/instances/6fa496ce-d5c0-4e02-9223-06a98c9c0176",
			"cost_center": null,
			"created_on": "2024-05-23T14:27:29.5114963+00:00",
			"updated_on": "2024-05-23T14:27:29.5114963+00:00",
			"created_by": "SYSTEM",
			"updated_by": "SYSTEM",
			"deleted": false
		},
		{
			"type": "FoundationaLLM.Authorization/roleAssignments",
			"name": "4521b9-1b0e-4ad9-a8cd-75727282cbe7",
			"object_id": "/providers/FoundationaLLM.Authorization/roleAssignments/4521bdb9-1b0e-4ad9-a8cd-75727282cbe7",
			"display_name": null,
			"description": "Role assignment reader for the Management API managed identity.",
			"role_definition_id": "/providers/FoundationaLLM.Authorization/roleDefinitions/00a53e72-f66e-4c03-8f81-7e885fd2eb35",
			"principal_id": "example-f8e51522-4291-4471-b029-2649b7311d01",
			"principal_type": "User",
			"scope": "/instances/6fa496ce-d5c0-4e02-9223-06a98c9c0176",
			"cost_center": null,
			"created_on": "2024-05-23T14:27:29.5114963+00:00",
			"updated_on": "2024-05-23T14:27:29.5114963+00:00",
			"created_by": "SYSTEM",
			"updated_by": "SYSTEM",
			"deleted": false
		},
		{
			"type": "FoundationaLLM.Authorization/roleAssignments",
			"name": "597646-7e39-46a0-aced-9c9be832b437",
			"object_id": "/providers/FoundationaLLM.Authorization/roleAssignments/59766546-7e39-46a0-aced-9c9be832b437",
			"display_name":'Example 4',
			"description": "Role assignment reader for the Core API managed identity.",
			"role_definition_id": "/providers/FoundationaLLM.Authorization/roleDefinitions/00a53e72-f66e-4c03-8f81-7e885fd2eb35",
			"principal_id": "example-d59f73e7-ff08-41bb-881c-fc3c63bdcf30",
			"principal_type": "User",
			"scope": "/instances/6fa496ce-d5c0-4e02-9223-06a98c9c0176",
			"cost_center": null,
			"created_on": "2024-05-23T14:27:29.5114963+00:00",
			"updated_on": "2024-05-23T14:27:29.5114963+00:00",
			"created_by": "SYSTEM",
			"updated_by": "SYSTEM",
			"deleted": false
		},
		{
			"type": "FoundationaLLM.Authorization/roleAssignments",
			"name": "afc3b8-7f91-4968-b6ac-e260a3a7157b",
			"object_id": "/providers/FoundationaLLM.Authorization/roleAssignments/afc304b8-7f91-4968-b6ac-e260a3a7157b",
			"display_name": 'Example 5',
			"description": "Role assignment reader for the Vectorization API managed identity.",
			"role_definition_id": "/providers/FoundationaLLM.Authorization/roleDefinitions/00a53e72-f66e-4c03-8f81-7e885fd2eb35",
			"principal_id": "example-bc7db642-9f96-49c1-b4ee-103d0178f432",
			"principal_type": "User",
			"scope": "/instances/6fa496ce-d5c0-4e02-9223-06a98c9c0176",
			"cost_center": null,
			"created_on": "2024-05-23T14:27:29.5114963+00:00",
			"updated_on": "2024-05-23T14:27:29.5114963+00:00",
			"created_by": "SYSTEM",
			"updated_by": "SYSTEM",
			"deleted": false
		},
		{
			"type": "FoundationaLLM.Authorization/roleAssignments",
			"name": "765716-3624-4feb-bc77-58676e096faf",
			"object_id": "/providers/FoundationaLLM.Authorization/roleAssignments/76578e16-3624-4feb-bc77-58676e096faf",
			"display_name": 'Example 6',
			"description": "Role assignment reader for the Orchestration API managed identity.",
			"role_definition_id": "/providers/FoundationaLLM.Authorization/roleDefinitions/00a53e72-f66e-4c03-8f81-7e885fd2eb35",
			"principal_id": "example-f7955eef-283e-438e-840f-5d5b2aa845b8",
			"principal_type": "User",
			"scope": "/instances/6fa496ce-d5c0-4e02-9223-06a98c9c0176",
			"cost_center": null,
			"created_on": "2024-05-23T14:27:29.5114963+00:00",
			"updated_on": "2024-05-23T14:27:29.5114963+00:00",
			"created_by": "SYSTEM",
			"updated_by": "SYSTEM",
			"deleted": false
		},
		{
			"type": "FoundationaLLM.Authorization/roleAssignments",
			"name": "4ff47b-1cd3-49d3-94cd-311ee68e3ac9",
			"object_id": "/providers/FoundationaLLM.Authorization/roleAssignments/4ff9547b-1cd3-49d3-94cd-311ee68e3ac9",
			"display_name": 'Example 7',
			"description": "Contributor role for Andrei local debug.",
			"role_definition_id": "/providers/FoundationaLLM.Authorization/roleDefinitions/a9f0020f-6e3a-49bf-8d1d-35fd53058edf",
			"principal_id": "example-e66c544b-a150-4135-8198-a80eaa312386",
			"principal_type": "User",
			"scope": "/instances/6fa496ce-d5c0-4e02-9223-06a98c9c0176",
			"cost_center": null,
			"created_on": "2024-03-22T15:33:08.2807765+00:00",
			"updated_on": "2024-03-22T15:33:08.2807765+00:00",
			"created_by": "SYSTEM",
			"updated_by": "SYSTEM",
			"deleted": false
		},
		{
			"type": "FoundationaLLM.Authorization/roleAssignments",
			"name": "ca46b0-f3e9-4d25-bca8-e36d226b80fd",
			"object_id": "/providers/FoundationaLLM.Authorization/roleAssignments/ca467bb0-f3e9-4d25-bca8-e36d226b80fd",
			"display_name": 'Example 8',
			"description": "Brian Local Dev - Contributor role on the FoundationaLLM instance.",
			"role_definition_id": "/providers/FoundationaLLM.Authorization/roleDefinitions/a9f0020f-6e3a-49bf-8d1d-35fd53058edf",
			"principal_id": "example-2930c230-b255-435a-8ef1-7788d99c2a36",
			"principal_type": "User",
			"scope": "/instances/6fa496ce-d5c0-4e02-9223-06a98c9c0176",
			"cost_center": null,
			"created_on": "2024-05-23T14:27:29.5114963+00:00",
			"updated_on": "2024-05-23T14:27:29.5114963+00:00",
			"created_by": "SYSTEM",
			"updated_by": "SYSTEM",
			"deleted": false
		},
		{
			"type": "FoundationaLLM.Authorization/roleAssignments",
			"name": "13bd64-d201-4846-9a49-4f8866dd6141",
			"object_id": "/instances/6fa496ce-d5c0-4e02-9223-06a98c9c0176/providers/FoundationaLLM.Authorization/roleAssignments/1336bd64-d201-4846-9a49-4f8866dd6141",
			"display_name": 'Example 9',
			"description": "Owner role for Joel Hulen",
			"role_definition_id": "/providers/FoundationaLLM.Authorization/roleDefinitions/1301f8d4-3bea-4880-945f-315dbd2ddb46",
			"principal_id": "example-dc1f7302-f63e-4f75-a7e8-8984f75a522d",
			"principal_type": "User",
			"scope": "/instances/6fa496ce-d5c0-4e02-9223-06a98c9c0176/providers/FoundationaLLM.Agent/agents/JoelTest",
			"cost_center": null,
			"created_on": "0001-01-01T00:00:00+00:00",
			"updated_on": "0001-01-01T00:00:00+00:00",
			"created_by": null,
			"updated_by": null,
			"deleted": false
		}
	]
};
