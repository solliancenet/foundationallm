/* eslint-disable prettier/prettier */
import type { Message, Session, CompletionPrompt, Agent } from '@/js/types';
import { getMsalInstance } from '@/js/auth';

export default {
	apiUrl: null as string | null,
	bearerToken: null as string | null,

	setApiUrl(url: string) {
		// Set the api url and remove a trailing slash if there is one.
		this.apiUrl = url.replace(/\/$/, '');
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

	async getConfigValue(key: string) {
		return await $fetch(`/api/config/`, {
			params: {
				key
			}
		});
	},

	async fetch(url: string, opts: any = {}) {
		const options = opts;
		options.headers = opts.headers || {};

		// if (options?.query) {
		// 	url += '?' + (new URLSearchParams(options.query)).toString();
		// }

		const bearerToken = await this.getBearerToken();
		options.headers['Authorization'] = `Bearer ${bearerToken}`;

		try {
			return await $fetch(`${this.apiUrl}${url}`, options);
		} catch(error) {
			if (error.response.status === 401) {
				const router = useRouter();
				router.push({ query: { auth_expired: true } });
			}
		}
	},

	async getSessions() {
		return await this.fetch(`/sessions`) as Array<Session>;
	},

	async addSession() {
		return await this.fetch(`/sessions`, { method: 'POST' }) as Session;
	},

	async renameSession(sessionId: string, newChatSessionName: string) {
		return await this.fetch(`/sessions/${sessionId}/rename`, {
			method: 'POST',
			params: {
				newChatSessionName
			}
		}) as Session;
	},

	async summarizeSessionName(sessionId: string, text: string) {
		return await this.fetch(`/sessions/${sessionId}/summarize-name`, {
			method: 'POST',
			body: JSON.stringify(text),
		}) as { text: string };
	},

	async deleteSession(sessionId: string) {
		return await this.fetch(`/sessions/${sessionId}`, { method: 'DELETE' }) as Session;
	},

	async getMessages(sessionId: string) {
		return await this.fetch(`/sessions/${sessionId}/messages`) as Array<Message>;
	},

	async getPrompt(sessionId: string, promptId: string) {
		return await this.fetch(`/sessions/${sessionId}/completionprompts/${promptId}`) as CompletionPrompt;
	},

	async rateMessage(message: Message, rating: Message['rating']) {
		const params: {
			rating?: Message['rating']
		} = {};
		if (rating !== null) params.rating = rating;

		return await this.fetch(`/sessions/${message.sessionId}/message/${message.id}/rate`, {
				method: 'POST',
				params
			},
		) as Message;
	},

	async sendMessage(sessionId: string, text: string, agent: Agent) {
		const headers = agent ? { 'X-AGENT-HINT': JSON.stringify(agent) } : {};
		return await this.fetch(`/sessions/${sessionId}/completion`, {
			method: 'POST',
			body: JSON.stringify(text),
			headers,
		}) as string;
	},

	async getAllowedAgents() {
		return await this.fetch('/UserProfiles/agents') as Agent[];
	},
};
