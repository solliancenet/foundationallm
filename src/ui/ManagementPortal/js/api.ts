/* eslint-disable prettier/prettier */
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

		return await $fetch(`${this.apiUrl}${url}`, options);
	},
};
