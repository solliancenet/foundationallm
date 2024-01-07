import { defineStore } from 'pinia';
import type { AccountInfo } from '@azure/msal-browser';
import { getMsalInstance, getLoginRequest, createTokenRefreshTimer } from '@/js/auth';

export const useAuthStore = defineStore('auth', {
	state: () => ({
		accounts: [] as AccountInfo[],
		currentAccount: null as AccountInfo | null,
	}),

	getters: {
		isAuthed(state) {
			return !!state.currentAccount;
		},
	},

	actions: {
		setAccounts(accounts: AccountInfo[]) {
			this.accounts = accounts;
			this.currentAccount = accounts[0];
		},

		async login() {
			const msalInstance = await getMsalInstance();
			const loginRequest = await getLoginRequest();

			await msalInstance.handleRedirectPromise();
			const response = await msalInstance.loginRedirect(loginRequest);
			if (response.account) {
				this.currentAccount = response.account;
				createTokenRefreshTimer();
			}

			return response;
		},

		async logout() {
			const msalInstance = await getMsalInstance();
			const accountFilter = {
				username: this.currentAccount?.username,
			};
			const logoutRequest = {
				account: msalInstance.getAccount(accountFilter),
			};

			await msalInstance.logoutRedirect(logoutRequest);

			const nuxtApp = useNuxtApp();
			nuxtApp.$router.push({ name: 'auth/login' });
		},
	},
});
