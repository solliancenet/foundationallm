export type AgentDataSource = {};

export type AgentIndex = {
	Name: string;
	Description: string;
	Indexer: string;
	Settings: {
		IndexName: string;
		TopN?: string;
		Filters?: string;
		EmbeddingFieldName?: string;
		TextFieldName?: string;
	};
	ConfigurationReferences: {
		APIKey: string;
		AuthenticationType: string;
		Endpoint: string;
	};
};

export type AgentGatekeeper = {};

export type MockCreateAgentRequest = {
	type: 'knowledge' | 'analytics';
	storageSource: number;
	indexSource: number;
	processing: {
		chunkSize: number;
		overlapSize: number;
	};
	trigger: {
		frequency: 'auto' | 'manual' | 'scheduled';
	};
	conversation_history: {
		enabled: boolean;
		max_history: number;
	};
	gatekeeper: {
		use_system_setting: boolean;
		options: {
			content_safety: number;
			data_protection: number;
		};
	};
	prompt: string;
};

export type CreateAgentRequest = {
	name: string;
	type: 'knowledge-management' | 'analytics';
	// description: string;
	// "language_model": {
	//   "type": "openai",
	//   "provider": "microsoft",
	//   "temperature": 0,
	//   "use_chat": true,
	//   "api_endpoint": "FoundationaLLM:AzureOpenAI:API:Endpoint",
	//   "api_key": "FoundationaLLM:AzureOpenAI:API:Key",
	//   "api_version": "FoundationaLLM:AzureOpenAI:API:Version",
	//   "version": "FoundationaLLM:AzureOpenAI:API:Completions:ModelVersion",
	//   "deployment": "FoundationaLLM:AzureOpenAI:API:Completions:DeploymentName"
	// },
	indexing_profile: string;
	embedding_profile: string;
	sessions_enabled: boolean;
	orchestrator: string;
	conversation_history: {
		enabled: boolean;
		max_history: number;
	};
	gatekeeper: {
		use_system_setting: boolean;
		options: {
			content_safety: number;
			data_protection: number;
		};
	};
	prompt: string;
};