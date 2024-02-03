import { getMsalInstance, createTokenRefreshTimer } from '@/js/auth';
import { useAuthStore } from '@/stores/authStore';

export default defineNuxtRouteMiddleware(async (to, from) => {
	if (process.server) return;

	if (to.name !== 'status') {
		const msalInstance = await getMsalInstance();
		await msalInstance.handleRedirectPromise();
		const accounts = await msalInstance.getAllAccounts();

		const isAuthenticated = accounts.length > 0;

		if (isAuthenticated) {
			const authStore = useAuthStore();
			authStore.setAccounts(accounts);
			createTokenRefreshTimer();

			if (to.name === 'auth/login') {
	    	return navigateTo({ path: '/' });
			}
		}

		if (!isAuthenticated && to.name !== 'auth/login') {
	    return navigateTo({ name: 'auth/login' });
	  }	
	}
});
