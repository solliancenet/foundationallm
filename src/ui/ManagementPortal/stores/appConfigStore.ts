import { defineStore } from 'pinia';
import type { AuthConfigOptions } from '@js/auth';
import api from '@/js/api';

export const useAppConfigStore = defineStore('appConfig', {
	state: () => ({
		// API: Defines API-specific settings such as the base URL for application requests.
		// apiUrl: null,

		// Auth: These settings configure the MSAL authentication.
		auth: {
			clientId: null,
			instance: null,
			tenantId: null,
			scopes: [],
			callbackPath: null,
		} as AuthConfigOptions,
	}),
	getters: {},
	actions: {
		async getConfigVariables() {
			const [
				// apiUrl,
				authClientId,
				authInstance,
				authTenantId,
				authScopes,
				authCallbackPath,
			] = await Promise.all([
				// api.getConfigValue('FoundationaLLM:APIs:CoreAPI:APIUrl'),
				api.getConfigValue('FoundationaLLM:Management:Entra:ClientId'),
				api.getConfigValue('FoundationaLLM:Management:Entra:Instance'),
				api.getConfigValue('FoundationaLLM:Management:Entra:TenantId'),
				api.getConfigValue('FoundationaLLM:Management:Entra:Scopes'),
				api.getConfigValue('FoundationaLLM:Management:Entra:CallbackPath'),
			]);

			// this.apiUrl = apiUrl;

			this.auth.clientId = authClientId;
			this.auth.instance = authInstance;
			this.auth.tenantId = authTenantId;
			this.auth.scopes = authScopes;
			this.auth.callbackPath = authCallbackPath;
		},
	},
});
