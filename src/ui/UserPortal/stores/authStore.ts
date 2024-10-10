import { defineStore } from 'pinia';
import type { AccountInfo } from '@azure/msal-browser';
import { PublicClientApplication } from '@azure/msal-browser';
import { useAppStore } from './appStore';

export const useAuthStore = defineStore('auth', {
	state: () => ({
		msalInstance: null,
		tokenExpirationTimerId: null as number | null,
		isExpired: false,
		apiToken: null,
	}),

	getters: {
		accounts(): AccountInfo[] {
			return this.msalInstance.getAllAccounts();
		},

		currentAccount(): AccountInfo | null {
			return this.accounts[0] || null;
		},

		isAuthenticated(): boolean {
			return !!this.currentAccount && !this.isExpired;
		},

		authConfig() {
			return useNuxtApp().$appConfigStore.auth;
		},

		oneDriveWorkSchoolScopes() {
			const appStore = useAppStore();
			return appStore.fileStoreConfiguration?.fileStoreConnectors?.find(
				(connector) => connector.subcategory === 'OneDriveWorkSchool',
			)?.authentication_parameters['scope'];
		},

		apiScopes() {
			return [this.authConfig.scopes];
		},
	},

	actions: {
		async init() {
			const msalInstance = new PublicClientApplication({
				auth: {
					clientId: this.authConfig.clientId,
					authority: `${this.authConfig.instance}${this.authConfig.tenantId}`,
					redirectUri: this.authConfig.callbackPath,
					scopes: this.apiScopes,
					// Must be registered as a SPA redirectURI on your app registration.
					postLogoutRedirectUri: '/',
				},
				cache: {
					cacheLocation: 'sessionStorage',
				},
			});

			await msalInstance.initialize();
			this.msalInstance = msalInstance;

			return this;
		},

		createTokenRefreshTimer() {
			const tokenExpirationTimeMS = this.apiToken.expiresOn;
			const currentTime = Date.now();
			const timeUntilExpirationMS = tokenExpirationTimeMS - currentTime;

			// If the token expires within the next minute, try to refresh it
			if (timeUntilExpirationMS <= 60 * 1000) {
				return this.tryTokenRefresh();
			}

			console.log(`Auth: Cleared previous access token timer.`);
			clearTimeout(this.tokenExpirationTimerId);

			this.tokenExpirationTimerId = setTimeout(() => {
				this.tryTokenRefresh();
			}, timeUntilExpirationMS);

			const refreshDate = new Date(tokenExpirationTimeMS);
			console.log(
				`Auth: Set access token timer refresh for ${refreshDate} (in ${timeUntilExpirationMS / 1000} seconds).`,
			);
		},

		async tryTokenRefresh() {
			try {
				await this.getApiToken();
				console.log('Auth: Successfully refreshed access token.');
			} catch (error) {
				console.error('Auth: Failed to refresh access token:', error);
				this.isExpired = true;
			}
		},

		async getApiToken() {
			try {
				this.apiToken = await this.msalInstance.acquireTokenSilent({
					account: this.currentAccount,
					scopes: this.apiScopes,
				});

				this.createTokenRefreshTimer();

				return this.apiToken;
			} catch (error) {
				this.isExpired = true;
				throw error;
			}
		},

		async requestOneDriveWorkSchoolConsent() {
			let accessToken = '';
			const oneDriveWorkSchoolAPIScopes: any = {
				account: this.currentAccount,
				scopes: [this.oneDriveWorkSchoolScopes],
			};

			try {
				const resp = await this.msalInstance.acquireTokenSilent(oneDriveWorkSchoolAPIScopes);
				accessToken = resp.accessToken;
			} catch (error) {
				// Redirect to get token or login
				localStorage.setItem('oneDriveWorkSchoolConsentRedirect', JSON.stringify(true));
				
				oneDriveWorkSchoolAPIScopes.state = 'Core API redirect';
				await this.msalInstance.loginRedirect(oneDriveWorkSchoolAPIScopes);
			}
			return accessToken;
		},

		async getOneDriveWorkSchoolToken(): string | null {
			const appStore = useAppStore();
			const oneDriveBaseURL = appStore.fileStoreConfiguration?.fileStoreConnectors?.find(
				(connector) => connector.subcategory === 'OneDriveWorkSchool',
			)?.url;
			const oneDriveToken = await this.msalInstance.acquireTokenSilent({
				account: this.currentAccount,
				scopes: [`${oneDriveBaseURL}${this.oneDriveWorkSchoolScopes}`],
			});

			return oneDriveToken;
		},

		async getProfilePhoto(): string | null {
			try {
				const graphScopes = ['https://graph.microsoft.com/User.Read'];
				const graphToken = await this.msalInstance.acquireTokenSilent({
					account: this.currentAccount,
					scopes: graphScopes,
				});

				const profilePhotoBlob = await $fetch('https://graph.microsoft.com/v1.0/me/photo/$value', {
					method: 'GET',
					headers: {
						Authorization: `Bearer ${graphToken.accessToken}`,
					},
				});

				return URL.createObjectURL(profilePhotoBlob);
			} catch (error) {
				return null;
			}
		},

		async login() {
			return await this.msalInstance.loginRedirect({
				scopes: this.apiScopes,
			});
		},

		async clearLocalSession() {
			await this.msalInstance.controller.browserStorage.clear();
		},

		async logoutSilent() {
			const logoutHint = this.currentAccount.idTokenClaims.login_hint;
			await this.msalInstance.logoutRedirect({
				logoutHint,
			});
		},

		async logout() {
			await this.msalInstance.logoutRedirect({
				account: this.currentAccount,
			});

			useNuxtApp().$router.push({ name: 'auth/login' });
		},
	},
});
