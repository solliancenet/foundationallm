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
	apiUrl: null as string | null,
	setApiUrl(apiUrl: string) {
		this.apiUrl = apiUrl;
	},

	instanceId: null as string | null,
	setInstanceId(instanceId: string) {
		this.instanceId = instanceId;
	},

	bearerToken: null,
	async getBearerToken() {
		if (this.bearerToken) return this.bearerToken;

		const token = await useNuxtApp().$authStore.getToken();
		this.bearerToken = token.accessToken;
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
		const response = await this.fetch(`/instances/${this.instanceId}/providers/FoundationaLLM.Vectorization/contentsourceprofiles?api-version=${this.apiVersion}`) as string;
		const data = JSON.parse(response) as AgentDataSource[];
		return data.map(source => ({ ...source, Formats: ['pdf', 'txt'] }));
	},

	async getAgentIndexes(): Promise<AgentIndex[]> {
		const response =  await this.fetch(`/instances/${this.instanceId}/providers/FoundationaLLM.Vectorization/indexingprofiles?api-version=${this.apiVersion}`) as string;
		const data = JSON.parse(response) as AgentIndex[];
		return data;
	},

	async getAgentGatekeepers(): Promise<AgentGatekeeper[]> {
		await wait(this.mockLoadTime);
		return [];
	},

	async getAgents(): Promise<AgentIndex[]> {
		return JSON.parse(await this.fetch(`/instances/${this.instanceId}/providers/FoundationaLLM.Agent/agents?api-version=${this.apiVersion}`));
	},

	async getAgent(agentId: string): Promise<any> {
		return JSON.parse(await this.fetch(`/instances/${this.instanceId}/providers/FoundationaLLM.Agent/agents/${agentId}?api-version=${this.apiVersion}`));
	},

	async updateAgent(agentId: string, request: CreateAgentRequest): Promise<any> {
		return await this.fetch(`/instances/${this.instanceId}/providers/FoundationaLLM.Agent/agents/${agentId}?api-version=${this.apiVersion}`, {
			method: 'POST',
			body: request,
		});
	},

	async createAgent(request: CreateAgentRequest): Promise<any> {
		return await this.fetch(`/instances/${this.instanceId}/providers/FoundationaLLM.Agent/agents/${request.name}?api-version=${this.apiVersion}`, {
			method: 'POST',
			body: request,
		});
	},
}
