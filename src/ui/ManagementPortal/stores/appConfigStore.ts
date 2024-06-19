import { defineStore } from 'pinia';
// import type { AuthConfigOptions } from '@js/auth';
import api from '@/js/api';

export const useAppConfigStore = defineStore('appConfig', {
	state: () => ({
		// API: Defines API-specific settings such as the base URL for application requests.
		apiUrl: null,
		authorizationApiUrl: null,
		coreApiUrl: null,
		gatekeeperApiUrl: null,
		gatekeeperIntegrationApiUrl: null,
		gatewayApiUrl: null,
		langChainApiUrl: null,
		orchestrationApiUrl: null,
		semanticKernelApiUrl: null,
		vectorizationApiUrl: null,
		vectorizationWorkerApiUrl: null,

		instanceId: null,

		// Style: These settings impact the visual style of the chat interface.
		logoUrl: null,
		logoText: null,
		primaryBg: null,
		primaryColor: null,
		secondaryColor: null,
		accentColor: null,
		primaryText: null,
		secondaryText: null,
		accentText: null,
		primaryButtonBg: null,
		primaryButtonText: null,
		secondaryButtonBg: null,
		secondaryButtonText: null,
		footerText: null,

		// Auth: These settings configure the MSAL authentication.
		auth: {
			clientId: null,
			instance: null,
			tenantId: null,
			scopes: [],
			callbackPath: null,
		}, // as AuthConfigOptions,
	}),
	getters: {},
	actions: {
		async getConfigVariables() {
			const getConfigValueSafe = async (key: string, defaultValue: any = null) => {
                try {
                    return await api.getConfigValue(key);
                } catch (error) {
                    console.error(`Failed to get config value for key ${key}:`, error);
                    return defaultValue;
                }
            };
			
			const [
				apiUrl,
				authorizationApiUrl,
				coreApiUrl,
				gatekeeperApiUrl,
				gatekeeperIntegrationApiUrl,
				gatewayApiUrl,
				langChainApiUrl,
				orchestrationApiUrl,
				semanticKernelApiUrl,
				vectorizationApiUrl,
				vectorizationWorkerApiUrl,
				instanceId,
				logoUrl,
				logoText,
				primaryBg,
				primaryColor,
				secondaryColor,
				accentColor,
				primaryText,
				secondaryText,
				accentText,
				primaryButtonBg,
				primaryButtonText,
				secondaryButtonBg,
				secondaryButtonText,
				footerText,
				authClientId,
				authInstance,
				authTenantId,
				authScopes,
				authCallbackPath,
			] = await Promise.all([
				api.getConfigValue('FoundationaLLM:APIs:ManagementAPI:APIUrl'),
				api.getConfigValue('FoundationaLLM:APIs:AuthorizationAPI:APIUrl'),
				api.getConfigValue('FoundationaLLM:APIs:CoreAPI:APIUrl'),
				api.getConfigValue('FoundationaLLM:APIs:GatekeeperAPI:APIUrl'),
				api.getConfigValue('FoundationaLLM:APIs:GatekeeperIntegrationAPI:APIUrl'),
				api.getConfigValue('FoundationaLLM:APIs:GatewayAPI:APIUrl'),
				api.getConfigValue('FoundationaLLM:APIs:LangChainAPI:APIUrl'),
				api.getConfigValue('FoundationaLLM:APIs:OrchestrationAPI:APIUrl'),
				api.getConfigValue('FoundationaLLM:APIs:SemanticKernelAPI:APIUrl'),
				api.getConfigValue('FoundationaLLM:APIs:VectorizationAPI:APIUrl'),
				api.getConfigValue('FoundationaLLM:APIs:VectorizationWorker:APIUrl'),

				api.getConfigValue('FoundationaLLM:Instance:Id'),

				getConfigValueSafe('FoundationaLLM:Branding:LogoUrl', 'foundationallm-logo-white.svg'),
				getConfigValueSafe('FoundationaLLM:Branding:LogoText'),
				getConfigValueSafe('FoundationaLLM:Branding:BackgroundColor', '#fff'),
				getConfigValueSafe('FoundationaLLM:Branding:PrimaryColor', '#131833'),
				getConfigValueSafe('FoundationaLLM:Branding:SecondaryColor', '#334581'),
				getConfigValueSafe('FoundationaLLM:Branding:AccentColor', '#fff'),
				getConfigValueSafe('FoundationaLLM:Branding:PrimaryTextColor', '#fff'),
				getConfigValueSafe('FoundationaLLM:Branding:SecondaryTextColor', '#fff'),
				getConfigValueSafe('FoundationaLLM:Branding:AccentTextColor', '#131833'),
				getConfigValueSafe('FoundationaLLM:Branding:PrimaryButtonBackgroundColor', '#5472d4'),
				getConfigValueSafe('FoundationaLLM:Branding:PrimaryButtonTextColor', '#fff'),
				getConfigValueSafe('FoundationaLLM:Branding:SecondaryButtonBackgroundColor', '#70829a'),
				getConfigValueSafe('FoundationaLLM:Branding:SecondaryButtonTextColor', '#fff'),
				getConfigValueSafe('FoundationaLLM:Branding:FooterText'),

				api.getConfigValue('FoundationaLLM:Management:Entra:ClientId'),
				api.getConfigValue('FoundationaLLM:Management:Entra:Instance'),
				api.getConfigValue('FoundationaLLM:Management:Entra:TenantId'),
				api.getConfigValue('FoundationaLLM:Management:Entra:Scopes'),
				api.getConfigValue('FoundationaLLM:Management:Entra:CallbackPath'),
			]);

			this.apiUrl = apiUrl;
			this.authorizationApiUrl = authorizationApiUrl;
			this.coreApiUrl = coreApiUrl;
			this.gatekeeperApiUrl = gatekeeperApiUrl;
			this.gatekeeperIntegrationApiUrl = gatekeeperIntegrationApiUrl;
			this.gatewayApiUrl = gatewayApiUrl;
			this.langChainApiUrl = langChainApiUrl;
			this.orchestrationApiUrl = orchestrationApiUrl;
			this.semanticKernelApiUrl = semanticKernelApiUrl;
			this.vectorizationApiUrl = vectorizationApiUrl;
			this.vectorizationWorkerApiUrl = vectorizationWorkerApiUrl;

			this.instanceId = instanceId;

			this.logoUrl = logoUrl;
			this.logoText = logoText;
			this.primaryBg = primaryBg;
			this.primaryColor = primaryColor;
			this.secondaryColor = secondaryColor;
			this.accentColor = accentColor;
			this.primaryText = primaryText;
			this.secondaryText = secondaryText;
			this.accentText = accentText;
			this.primaryButtonBg = primaryButtonBg;
			this.primaryButtonText = primaryButtonText;
			this.secondaryButtonBg = secondaryButtonBg;
			this.secondaryButtonText = secondaryButtonText;
			this.footerText = footerText;

			this.auth.clientId = authClientId;
			this.auth.instance = authInstance;
			this.auth.tenantId = authTenantId;
			this.auth.scopes = authScopes;
			this.auth.callbackPath = authCallbackPath;
		},
	},
});
