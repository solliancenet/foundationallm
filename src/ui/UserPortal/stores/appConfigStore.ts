import { defineStore } from 'pinia';
import type { AuthConfigOptions } from '@js/auth';
import api from '@/js/api';

export const useAppConfigStore = defineStore('appConfig', {
	state: () => ({
		// API: Defines API-specific settings such as the base URL for application requests.
		apiUrl: null,

		// Layout: These settings impact the structural layout of the chat interface.
		isKioskMode: false,

		// Style: These settings impact the visual style of the chat interface.
		pageTitle: null,
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
		} as AuthConfigOptions,
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
				isKioskMode,
				pageTitle,
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
				api.getConfigValue('FoundationaLLM:APIs:CoreAPI:APIUrl'),
				
				getConfigValueSafe('FoundationaLLM:Branding:KioskMode'),
				getConfigValueSafe('FoundationaLLM:Branding:PageTitle'),
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
				
				api.getConfigValue('FoundationaLLM:Chat:Entra:ClientId'),
				api.getConfigValue('FoundationaLLM:Chat:Entra:Instance'),
				api.getConfigValue('FoundationaLLM:Chat:Entra:TenantId'),
				api.getConfigValue('FoundationaLLM:Chat:Entra:Scopes'),
				api.getConfigValue('FoundationaLLM:Chat:Entra:CallbackPath'),
			]);

			this.apiUrl = apiUrl;

			this.isKioskMode = JSON.parse(isKioskMode.toLowerCase());

			this.pageTitle = pageTitle;
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
