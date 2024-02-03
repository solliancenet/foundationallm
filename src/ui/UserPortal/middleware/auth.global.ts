import { getMsalInstance, createTokenRefreshTimer } from '@/js/auth';
import { useAuthStore } from '@/stores/authStore';

export default defineNuxtRouteMiddleware(async (to, from) => {
	if (process.server) return;

	if (to.name !== 'status') {
		const msalInstance = await getMsalInstance();
		await msalInstance.handleRedirectPromise();
		const accounts = await msalInstance.getAllAccounts();

		if (accounts.length > 0) {
			const authStore = useAuthStore();
			authStore.setAccounts(accounts);
		}

		if (accounts.length > 0 && to.path !== '/') {
			return navigateTo({ path: '/', query: from.query });
		}

		if (accounts.length === 0 && to.name !== 'auth/login') {
			return navigateTo({ name: 'auth/login', query: from.query });
		} else {
			createTokenRefreshTimer();
		}
	}
});
