/* eslint-disable prettier/prettier */

import { getMsalInstance } from '@/js/auth';
import type {
	AgentDataSource,
	AgentIndex,
	AgentGatekeeper,
	CreateAgentRequest
} from './types';
import { mockGetAgentIndexesResponse, mockGetAgentDataSourcesResponse } from './mock';

async function wait(milliseconds: number = 1000): Promise<void> {
	return await new Promise<void>((resolve) => setTimeout(() => resolve(), milliseconds));
}

export default {
	mockLoadTime: 1000,

	apiVersion: '1.0',
	apiUrl: null,
	setApiUrl(apiUrl) {
		this.apiUrl = apiUrl;
	},

	instanceId: null,
	setInstanceId(instanceId) {
		this.instanceId = instanceId;
	},

	async getBearerToken() {
		if (this.bearerToken) return this.bearerToken;

		const msalInstance = await getMsalInstance();
		const accounts = msalInstance.getAllAccounts();
		const account = accounts[0];
		const bearerToken = await msalInstance.acquireTokenSilent({ account });

		this.bearerToken = bearerToken.accessToken;
		return this.bearerToken;
	},

	async fetch(url: string, opts: any = {}) {
		const options = opts;
		options.headers = opts.headers || {};

		// if (options?.query) {
		// 	url += '?' + (new URLSearchParams(options.query)).toString();
		// }

		const bearerToken = await this.getBearerToken();
		options.headers['Authorization'] = `Bearer ${bearerToken}`;

		return await $fetch(`${this.apiUrl}${url}`, options);
	},

	async getConfigValue(key: string) {
		return await $fetch(`/api/config/`, {
			params: {
				key
			}
		});
	},

	async getAgentDataSources(): Promise<AgentDataSource[]> {
		const data = JSON.parse(await this.fetch(`/instances/${this.instanceId}/providers/FoundationaLLM.Vectorization/contentsourceprofiles?api-version=${this.apiVersion}`));
		return data.map(source => ({ ...source, Formats: ['pdf', 'txt'] }));
	},

	async getAgentIndexes(): Promise<AgentIndex[]> {
		return JSON.parse(await this.fetch(`/instances/${this.instanceId}/providers/FoundationaLLM.Vectorization/indexingprofiles?api-version=${this.apiVersion}`));
	},

	async getAgentGatekeepers(): Promise<AgentGatekeeper[]> {
		await wait(this.mockLoadTime);
		return [];
	},

	async createAgent(request: CreateAgentRequest): Promise<void> {
		return await this.fetch(`/instances/${this.instanceId}/providers/FoundationaLLM.Agent/agents/${request.name}?api-version=${this.apiVersion}`, {
			method: 'POST',
			body: request,
		});
	},
}
