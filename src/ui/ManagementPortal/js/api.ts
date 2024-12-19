import { $fetch } from 'ofetch';
import type {
	ResourceProviderGetResult,
	Agent,
	DataSource,
	AppConfigUnion,
	AgentIndex,
	// AgentGatekeeper,
	AgentAccessToken,
	ResourceProviderUpsertResult,
	AIModel,
	FilterRequest,
	CreateAgentRequest,
	CheckNameResponse,
	Prompt,
	TextPartitioningProfile,
	TextEmbeddingProfile,
	CreatePromptRequest,
	CreateTextPartitioningProfileRequest,
	ExternalOrchestrationService,
	// Role,
	RoleAssignment,
} from './types';
import { convertToDataSource, convertToAppConfigKeyVault, convertToAppConfig } from '@/js/types';

// async function wait(milliseconds: number = 1000): Promise<void> {
// 	return await new Promise<void>((resolve) => setTimeout(() => resolve(), milliseconds));
// }

export default {
	apiVersion: '2024-02-16',
	apiUrl: null as string | null,
	setApiUrl(apiUrl: string) {
		this.apiUrl = apiUrl;
	},

	instanceId: null as string | null,
	setInstanceId(instanceId: string) {
		this.instanceId = instanceId;
	},

	/**
	 * Retrieves the bearer token for authentication.
	 * If the bearer token is already available, it will be returned immediately.
	 * Otherwise, it will acquire a new bearer token using the MSAL instance.
	 * @returns The bearer token.
	 */
	async getBearerToken() {
		// When the scope is specific on aquireTokenSilent this seems to be instant
		// otherwise we would have to store the token and check if it has expired here
		// to determine if we need to fetch it again
		const token = await useNuxtApp().$authStore.getApiToken();
		return token.accessToken;
	},

	async fetch(url: string, opts: any = {}) {
		const options = opts;
		options.headers = opts.headers || {};

		// if (options?.query) {
		// 	url += '?' + (new URLSearchParams(options.query)).toString();
		// }

		const bearerToken = await this.getBearerToken();
		options.headers.Authorization = `Bearer ${bearerToken}`;

		return await $fetch(`${this.apiUrl}${url}`, options);
	},

	async getConfigValue(key: string) {
		return await $fetch(`/api/config/`, {
			params: {
				key,
			},
		});
	},

	/*
		Data Sources
	 */
	async checkDataSourceName(name: string, type: string): Promise<CheckNameResponse> {
		const payload = {
			name,
			type,
		};

		return await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.DataSource/dataSources/checkname?api-version=${this.apiVersion}`,
			{
				method: 'POST',
				body: payload,
			},
		);
	},

	async getDefaultDataSource(): Promise<DataSource | null> {
		const payload: FilterRequest = {
			default: true,
		};

		const data = await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.DataSource/dataSources/filter?api-version=${this.apiVersion}`,
			{
				method: 'POST',
				body: payload,
			},
		);

		if (data && data.length > 0) {
			return data[0] as DataSource;
		} else {
			return null;
		}
	},

	async getAgentDataSources(
		addDefaultOption: boolean = false,
	): Promise<ResourceProviderGetResult<DataSource>[]> {
		const data = (await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.DataSource/dataSources?api-version=${this.apiVersion}`,
		)) as ResourceProviderGetResult<DataSource>[];
		if (addDefaultOption) {
			const defaultDataSource: DataSource = {
				name: 'Select default data source',
				type: 'DEFAULT',
				object_id: '',
				resolved_configuration_references: {},
				configuration_references: {},
			};
			const defaultDataSourceResult: ResourceProviderGetResult<DataSource> = {
				resource: defaultDataSource,
				actions: [],
				roles: [],
			};
			data.unshift(defaultDataSourceResult);
		}
		return data;
	},

	async getDataSource(dataSourceId: string): Promise<ResourceProviderGetResult<DataSource>> {
		const [data] = (await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.DataSource/dataSources/${dataSourceId}?api-version=${this.apiVersion}`,
		)) as ResourceProviderGetResult<DataSource>[];
		let dataSource = data.resource as DataSource;
		dataSource.resolved_configuration_references = {};
		// Retrieve all the app config values for the data source.
		const appConfigFilter = `FoundationaLLM:DataSources:${dataSource.name}:*`;
		const appConfigResults = await this.getAppConfigs(appConfigFilter);

		// If set the resolved_configuration_references property on the data source with the app config values.
		if (appConfigResults) {
			for (const appConfigResult of appConfigResults) {
				const appConfig = appConfigResult.resource;
				const propertyName = appConfig.name.split(':').pop();
				dataSource.resolved_configuration_references[propertyName as string] = String(
					appConfig.value,
				);
			}
		} else {
			for (const [configName /* configValue */] of Object.entries(
				dataSource.configuration_references,
			)) {
				const resolvedValue = await this.getAppConfig(
					dataSource.configuration_references[
						configName as keyof typeof dataSource.configuration_references
					],
				);
				if (resolvedValue) {
					dataSource.resolved_configuration_references[configName] = String(resolvedValue.value);
				} else {
					dataSource.resolved_configuration_references[configName] = '';
				}
			}
		}
		dataSource = convertToDataSource(dataSource);
		data.resource = dataSource;
		return data;
	},

	async upsertDataSource(request): Promise<any> {
		const dataSource = convertToDataSource(request);
		for (const [propertyName, propertyValue] of Object.entries(
			dataSource.resolved_configuration_references || {},
		)) {
			if (!propertyValue) {
				continue;
			}

			const appConfigKey = `FoundationaLLM:DataSources:${dataSource.name}:${propertyName}`;
			const keyVaultSecretName =
				`foundationallm-datasources-${dataSource.name}-${propertyName}`.toLowerCase();
			const metadata = dataSource.configuration_reference_metadata?.[propertyName];

			const appConfigResult = await this.getAppConfig('FoundationaLLM:Configuration:KeyVaultURI');
			const keyVaultUri = appConfigResult.resource;

			let appConfig: AppConfigUnion = {
				name: appConfigKey,
				display_name: appConfigKey,
				description: '',
				key: appConfigKey,
				value: propertyValue,
			};

			if (metadata && metadata.isKeyVaultBacked) {
				appConfig = convertToAppConfigKeyVault({
					...appConfig,
					key_vault_uri: keyVaultUri.value,
					key_vault_secret_name: keyVaultSecretName,
				});
			} else {
				appConfig = convertToAppConfig(appConfig);
			}

			await this.upsertAppConfig(appConfig);

			dataSource.configuration_references[propertyName] = appConfigKey;
		}

		// Remove any any configuration_references whose values are null or empty strings.
		for (const [propertyName, propertyValue] of Object.entries(
			dataSource.configuration_references,
		)) {
			if (!propertyValue) {
				delete dataSource.configuration_references[propertyName];
			}
		}

		return await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.DataSource/dataSources/${dataSource.name}?api-version=${this.apiVersion}`,
			{
				method: 'POST',
				body: JSON.stringify(dataSource),
				headers: {
					'Content-Type': 'application/json',
				},
			},
		);
	},

	async deleteDataSource(dataSourceId: string): Promise<any> {
		return await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.DataSource/dataSources/${dataSourceId}?api-version=${this.apiVersion}`,
			{
				method: 'DELETE',
			},
		);
	},

	/*
		App Configuration
	 */
	async getAppConfig(key: string): Promise<ResourceProviderGetResult<AppConfigUnion>> {
		const data = await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Configuration/appConfigurations/${key}?api-version=${this.apiVersion}`,
		);
		return data[0] as ResourceProviderGetResult<AppConfigUnion>;
	},

	async getAppConfigs(filter?: string): Promise<ResourceProviderGetResult<AppConfigUnion>[]> {
		return (await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Configuration/appConfigurations/${filter}?api-version=${this.apiVersion}`,
		)) as ResourceProviderGetResult<AppConfigUnion>[];
	},

	async upsertAppConfig(request): Promise<any> {
		return await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Configuration/appConfigurations/${request.key}?api-version=${this.apiVersion}`,
			{
				method: 'POST',
				body: request,
			},
		);
	},

	/*
		Indexes
	 */
	async getAgentIndexes(
		addDefaultOption: boolean = false,
	): Promise<ResourceProviderGetResult<AgentIndex>[]> {
		const data = (await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Vectorization/indexingProfiles?api-version=${this.apiVersion}`,
		)) as ResourceProviderGetResult<AgentIndex>[];
		// If data is empty, return an empty array.
		if (!data) {
			return [];
		}
		// Retrieve all the app config values for the vectorization resources.
		const appConfigFilter = `FoundationaLLM:Vectorization:*`;
		const appConfigResults = await this.getAppConfigs(appConfigFilter);
		if (appConfigResults) {
			// Store all of the app config values (resource property of each result) in a map for easy access.
			const appConfigValues = new Map<string, AppConfigUnion>();
			for (const appConfigResult of appConfigResults) {
				appConfigValues.set(appConfigResult.resource.name, appConfigResult.resource);
			}

			// Loop through the text embedding profiles and replace the app config keys with the real values.
			for (const indexingProfile of data) {
				if (indexingProfile.resource.configuration_references === undefined) {
					continue;
				}
				indexingProfile.resource.resolved_configuration_references = {
					...indexingProfile.resource.configuration_references,
				};
				if (appConfigValues.has(indexingProfile.resource.configuration_references?.APIKey)) {
					indexingProfile.resource.resolved_configuration_references.APIKey =
						appConfigValues.get(indexingProfile.resource.configuration_references.APIKey)?.value ||
						'';
				}
				if (
					appConfigValues.has(indexingProfile.resource.configuration_references?.AuthenticationType)
				) {
					indexingProfile.resource.resolved_configuration_references.AuthenticationType =
						appConfigValues.get(
							indexingProfile.resource.configuration_references.AuthenticationType,
						)?.value || '';
				}
				if (appConfigValues.has(indexingProfile.resource.configuration_references?.Endpoint)) {
					indexingProfile.resource.resolved_configuration_references.Endpoint =
						appConfigValues.get(indexingProfile.resource.configuration_references.Endpoint)
							?.value || '';
				}
			}
		}

		if (addDefaultOption) {
			const defaultAgentIndex: AgentIndex = {
				name: 'Select default index source',
				object_id: '',
				settings: {},
				configuration_references: {},
			};
			const defaultAgentIndexResult: ResourceProviderGetResult<AgentIndex> = {
				resource: defaultAgentIndex,
				actions: [],
				roles: [],
			};
			data.unshift(defaultAgentIndexResult);
		}
		return data;
	},

	async getDefaultAgentIndex(): Promise<AgentIndex | null> {
		const payload: FilterRequest = {
			default: true,
		};

		const data = await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Vectorization/indexingProfiles/filter?api-version=${this.apiVersion}`,
			{
				method: 'POST',
				body: payload,
			},
		);

		if (data && data.length > 0) {
			return data[0] as AgentIndex;
		} else {
			return null;
		}
	},

	/*
		Text Embedding Profiles
	 */
	async getTextEmbeddingProfiles(): Promise<ResourceProviderGetResult<TextEmbeddingProfile>[]> {
		const data = (await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Vectorization/textEmbeddingProfiles?api-version=${this.apiVersion}`,
		)) as ResourceProviderGetResult<TextEmbeddingProfile>[];
		// If data is empty, return an empty array.
		if (!data) {
			return [];
		}
		// Retrieve all the app config values for the vectorization resources.
		const appConfigFilter = `FoundationaLLM:Vectorization:*`;
		const appConfigResults = await this.getAppConfigs(appConfigFilter);
		if (appConfigResults) {
			// Store all of the app config values (resource property of each result) in a map for easy access.
			const appConfigValues = new Map<string, AppConfigUnion>();
			for (const appConfigResult of appConfigResults) {
				appConfigValues.set(appConfigResult.resource.name, appConfigResult.resource);
			}

			// Loop through the text embedding profiles and replace the app config keys with the real values.
			for (const textEmbeddingProfile of data) {
				if (textEmbeddingProfile.resource.configuration_references === undefined) {
					continue;
				}
				textEmbeddingProfile.resource.resolved_configuration_references = {
					...textEmbeddingProfile.resource.configuration_references,
				};
				if (appConfigValues.has(textEmbeddingProfile.resource.configuration_references?.APIKey)) {
					textEmbeddingProfile.resource.resolved_configuration_references.APIKey =
						appConfigValues.get(textEmbeddingProfile.resource.configuration_references.APIKey)
							?.value || '';
				}
				if (
					appConfigValues.has(textEmbeddingProfile.resource.configuration_references?.APIVersion)
				) {
					textEmbeddingProfile.resource.resolved_configuration_references.APIVersion =
						appConfigValues.get(textEmbeddingProfile.resource.configuration_references.APIVersion)
							?.value || '';
				}
				if (
					appConfigValues.has(
						textEmbeddingProfile.resource.configuration_references?.AuthenticationType,
					)
				) {
					textEmbeddingProfile.resource.resolved_configuration_references.AuthenticationType =
						appConfigValues.get(
							textEmbeddingProfile.resource.configuration_references.AuthenticationType,
						)?.value || '';
				}
				if (
					appConfigValues.has(
						textEmbeddingProfile.resource.configuration_references?.DeploymentName,
					)
				) {
					textEmbeddingProfile.resource.resolved_configuration_references.DeploymentName =
						appConfigValues.get(
							textEmbeddingProfile.resource.configuration_references.DeploymentName,
						)?.value || '';
				}
				if (appConfigValues.has(textEmbeddingProfile.resource.configuration_references?.Endpoint)) {
					textEmbeddingProfile.resource.resolved_configuration_references.Endpoint =
						appConfigValues.get(textEmbeddingProfile.resource.configuration_references.Endpoint)
							?.value || '';
				}
			}
		}
		return data;
	},

	/*
		Agents
	 */
	async checkAgentName(name: string, agentType: string): Promise<CheckNameResponse> {
		const payload = {
			name,
			type: agentType,
		};

		return (await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Agent/agents/checkname?api-version=${this.apiVersion}`,
			{
				method: 'POST',
				body: payload,
			},
		)) as CheckNameResponse;
	},

	async getAgents(): Promise<ResourceProviderGetResult<Agent>[]> {
		const agents = (await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Agent/agents?api-version=${this.apiVersion}`,
		)) as ResourceProviderGetResult<Agent>[];
		// Sort the agents by name.
		agents.sort((a, b) => a.resource.name.localeCompare(b.resource.name));
		return agents;
	},

	async getAgent(agentId: string): Promise<ResourceProviderGetResult<Agent>> {
		const [agentGetResult]: ResourceProviderGetResult<Agent>[] = await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Agent/agents/${agentId}?api-version=${this.apiVersion}`,
		);

		const agent = agentGetResult.resource as Agent;

		const orchestratorTypeToKeyMap: { [key: string]: string } = {
			LangChain: 'AzureOpenAI',
			AzureOpenAIDirect: 'AzureOpenAI',
			AzureAIDirect: 'AzureAI',
		};

		const orchestratorTypeKey =
			orchestratorTypeToKeyMap[agent.orchestration_settings?.orchestrator];

		// Retrieve all the app config values for the agent
		const appConfigFilter = `FoundationaLLM:${orchestratorTypeKey}:${agent.name}:API:*`;
		const appConfigResults = await this.getAppConfigs(appConfigFilter);

		// Replace the orchestrator endpoint config keys with the real values
		if (appConfigResults) {
			for (const appConfigResult of appConfigResults) {
				const appConfig = appConfigResult.resource;
				const propertyName = appConfig.name.split(':').pop();
				agent.orchestration_settings.endpoint_configuration[propertyName as string] = String(
					appConfig.value,
				);
			}
		} else {
			for (const [configName /* configValue */] of Object.entries(agent.orchestration_settings)) {
				const resolvedValue = await this.getAppConfig(
					agent.orchestration_settings.endpoint_configuration[
						configName as keyof typeof agent.orchestration_settings.endpoint_configuration
					],
				);
				if (resolvedValue && resolvedValue.resource) {
					agent.orchestration_settings.endpoint_configuration[configName] = String(
						resolvedValue.resource.value,
					);
				} else {
					agent.orchestration_settings.endpoint_configuration[configName] = '';
				}
			}
		}

		agentGetResult.resource = agent;

		return agentGetResult;
	},

	async upsertAgent(agentId: string, agentData: CreateAgentRequest): Promise<any> {
		return await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Agent/agents/${agentId}?api-version=${this.apiVersion}`,
			{
				method: 'POST',
				body: agentData,
			},
		);
	},

	async createAgent(request: CreateAgentRequest): Promise<any> {
		return await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Agent/agents/${request.name}?api-version=${this.apiVersion}`,
			{
				method: 'POST',
				body: request,
			},
		);
	},

	async deleteAgent(agentId: string): Promise<any> {
		return await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Agent/agents/${agentId}?api-version=${this.apiVersion}`,
			{
				method: 'DELETE',
			},
		);
	},

	/*
		Prompts
	 */
	async getPrompts(): Promise<ResourceProviderGetResult<Prompt>[] | null> {
		try {
			const data = await this.fetch(
				`/instances/${this.instanceId}/providers/FoundationaLLM.Prompt/prompts?api-version=${this.apiVersion}`,
			);
			return data as ResourceProviderGetResult<Prompt>[];
		} catch (error) {
			return null;
		}
	},

	async getPromptByName(promptName: string): Promise<ResourceProviderGetResult<Prompt> | null> {
		// Attempt to retrieve the prompt. If it doesn't exist, return an empty object.
		try {
			const data = await this.fetch(
				`/instances/${this.instanceId}/providers/FoundationaLLM.Prompt/prompts/${promptName}?api-version=${this.apiVersion}`,
			);
			return data[0];
		} catch (error) {
			return null;
		}
	},

	async getPrompt(promptId: string): Promise<ResourceProviderGetResult<Prompt> | null> {
		// Attempt to retrieve the prompt. If it doesn't exist, return an empty object.
		try {
			const data = await this.fetch(`${promptId}?api-version=${this.apiVersion}`);
			return data[0];
		} catch (error) {
			return null;
		}
	},

	async createOrUpdatePrompt(agentId: string, request: CreatePromptRequest): Promise<any> {
		return await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Prompt/prompts/${agentId}?api-version=${this.apiVersion}`,
			{
				method: 'POST',
				body: request,
			},
		);
	},

	async getTextPartitioningProfile(
		profileId: string,
	): Promise<ResourceProviderGetResult<TextPartitioningProfile>> {
		const data = await this.fetch(`${profileId}?api-version=${this.apiVersion}`);
		return data[0];
	},

	async createOrUpdateTextPartitioningProfile(
		agentId: string,
		request: CreateTextPartitioningProfileRequest,
	): Promise<any> {
		return await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Vectorization/textPartitioningProfiles/${agentId}?api-version=${this.apiVersion}`,
			{
				method: 'POST',
				body: request,
			},
		);
	},

	async getExternalOrchestrationServices(
		resolveApiKey: boolean = false,
	): Promise<ResourceProviderGetResult<ExternalOrchestrationService>[]> {
		const data = (await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Configuration/apiEndpointConfigurations?api-version=${this.apiVersion}`,
		)) as ResourceProviderGetResult<ExternalOrchestrationService>[];

		// Retrieve all the app config values for the external orchestration services..
		const appConfigFilter = `FoundationaLLM:ExternalAPIs:*`;
		const appConfigResults = await this.getAppConfigs(appConfigFilter);

		// Loop through the external orchestration services and replace the app config keys with the real values.
		for (const externalOrchestrationService of data) {
			externalOrchestrationService.resource.resolved_api_key = '';

			if (resolveApiKey) {
				// Find a matching app config for the API Key. The app config name should be in the format FoundationaLLM:ExternalAPIs:<ServiceName>:APIKey
				const apiKeyAppConfig = appConfigResults.find(
					(appConfig) =>
						appConfig.resource.name ===
						`FoundationaLLM:ExternalAPIs:${externalOrchestrationService.resource.name}:APIKey`,
				);
				if (apiKeyAppConfig) {
					externalOrchestrationService.resource.resolved_api_key = apiKeyAppConfig.resource.value;
				}
			}
		}

		// Return the updated external orchestration services.
		return data;
	},

	async getBranding(): Promise<any> {
		return await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Configuration/appConfigurations/FoundationaLLM:Branding:*`,
		);
	},

	async saveBranding(key: String, params: any): Promise<any> {
		return await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Configuration/appConfigurations/${key}`,
			{
				method: 'POST',
				body: params,
			},
		);
	},

	/*
		AI Models
	 */
	async getAIModels(): Promise<ResourceProviderGetResult<AIModel>[]> {
		const data = (await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.AIModel/aiModels?api-version=${this.apiVersion}`,
		)) as ResourceProviderGetResult<AIModel>[];

		return data;
	},

	/*
		Role Assignments
	 */
	async getRoleAssignments(scope): RoleAssignment[] {
		const assignments = (await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Authorization/roleAssignments/filter?api-version=${this.apiVersion}`,
			{
				method: 'POST',
				body: JSON.stringify({
					scope: `/instances/${this.instanceId}${scope ? `/${scope}` : ''}`,
				}),
			},
		)) as RoleAssignment[];

		assignments.forEach((assignment) => {
			if (assignment.resource.scope === `/instances/${this.instanceId}`) {
				assignment.resource.scope_name = scope ? 'Instance (Inherited)' : 'Instance';
			} else if (assignment.resource.scope === `/instances/${this.instanceId}/${scope}`) {
				assignment.resource.scope_name = 'This resource';
			}
		});

		return assignments;
	},

	async getRoleAssignment(roleAssignmentId): RoleAssignment[] {
		return (await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Authorization/roleAssignments/${roleAssignmentId}?api-version=${this.apiVersion}`,
			{
				method: 'POST',
				body: JSON.stringify({
					scope: `/instances/${this.instanceId}`,
				}),
			},
		)) as RoleAssignment[];
	},

	async createRoleAssignment(request: Object): Promise<any> {
		if (!request.scope) {
			request.scope = `/instances/${this.instanceId}`;
		}

		return await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Authorization/roleAssignments/${request.name}?api-version=${this.apiVersion}`,
			{
				method: 'POST',
				body: JSON.stringify(request),
			},
		);
	},

	async deleteRoleAssignment(roleAssignmentId): void {
		return await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Authorization/roleAssignments/${roleAssignmentId}?api-version=${this.apiVersion}`,
			{
				method: 'DELETE',
			},
		);
	},

	/*
		Role Definitions
	 */
	async getRoleDefinitions(): RoleAssignment[] {
		return (await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Authorization/roleDefinitions?api-version=${this.apiVersion}`,
		)) as Object[];
	},

	async getRoleDefinition(roleAssignmentId): RoleAssignment {
		return (await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Authorization/roleDefinitions/${roleAssignmentId}?api-version=${this.apiVersion}`,
		)) as RoleAssignment[];
	},

	/*
		Users
	 */
	async getUsers(params) {
		const defaults = {
			name: '',
			ids: [],
			page_number: 1,
			page_size: null,
		};

		return await this.fetch(
			`/instances/${this.instanceId}/identity/users/retrieve?api-version=${this.apiVersion}`,
			{
				method: 'POST',
				body: JSON.stringify({
					...defaults,
					...params,
				}),
			},
		);
	},

	async getUser(userId) {
		return await this.fetch(
			`/instances/${this.instanceId}/identity/users/${userId}?api-version=${this.apiVersion}`,
		);
	},

	/*
		Groups
	 */
	async getGroups(params) {
		const defaults = {
			name: '',
			ids: [],
			page_number: 1,
			page_size: null,
		};

		return await this.fetch(
			`/instances/${this.instanceId}/identity/groups/retrieve?api-version=${this.apiVersion}`,
			{
				method: 'POST',
				body: JSON.stringify({
					...defaults,
					...params,
				}),
			},
		);
	},

	async getGroup(groupId) {
		return await this.fetch(
			`/instances/${this.instanceId}/identity/groups/${groupId}?api-version=${this.apiVersion}`,
		);
	},

	/*
		Combined User+Groups
	 */
	async getObjects(params = { ids: [] }) {
		return await this.fetch(
			`/instances/${this.instanceId}/identity/objects/retrievebyids?api-version=${this.apiVersion}`,
			{
				method: 'POST',
				body: JSON.stringify(params),
			},
		);
	},

	/*
		Private Storage
	 */
	async getPrivateStorageFiles(agentName) {
		return (await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Agent/agents/${agentName}/files?api-version=${this.apiVersion}`,
		)) as Object[];
	},

	async uploadToPrivateStorage(agentName, fileName, file: FormData): Promise<any> {
		return await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Agent/agents/${agentName}/files/${fileName}?api-version=${this.apiVersion}`,
			{
				method: 'POST',
				body: file,
			},
		);
	},

	async deleteFileFromPrivateStorage(agentName, fileName): Promise<any> {
		return await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Agent/agents/${agentName}/files/${fileName}?api-version=${this.apiVersion}`,
			{
				method: 'DELETE',
			},
		);
	},

	/*
		Agent Workflows
	 */
	async getAgentWorkflows(): Promise<any> {
		return await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Agent/workflows?api-version=${this.apiVersion}`,
		);
	},

	/*
		Agent Access Tokens
	 */
	async getAgentAccessTokens(agentName: string) {
		return (await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Agent/agents/${agentName}/agentAccessTokens?api-version=${this.apiVersion}`,
		)) as ResourceProviderGetResult<AgentAccessToken>[];
	},

	async createAgentAccessToken(
		agentName: string,
		body: AgentAccessToken,
	): Promise<ResourceProviderUpsertResult> {
		return (await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Agent/agents/${agentName}/agentAccessTokens/${body.id}?api-version=${this.apiVersion}`,
			{
				method: 'POST',
				body,
			},
		)) as ResourceProviderUpsertResult;
	},

	async deleteAgentAccessToken(agentName: string, accessTokenId: string): Promise<any> {
		return await this.fetch(
			`/instances/${this.instanceId}/providers/FoundationaLLM.Agent/agents/${agentName}/agentAccessTokens/${accessTokenId}?api-version=${this.apiVersion}`,
			{
				method: 'DELETE',
			},
		);
	},
};
