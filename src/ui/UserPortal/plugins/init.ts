import { defineNuxtPlugin } from '#app';
import { useAppConfigStore } from '@/stores/appConfigStore';
import api from '@/js/api';
import { setAuthConfig } from '@/js/auth';

export default defineNuxtPlugin(async (nuxtApp: any) => {
	// Load config variables server-side to ensure they are passed to the client via the store.
	// if (process.server) {
		const appConfigStore = useAppConfigStore(nuxtApp.$pinia);
		await appConfigStore.getConfigVariables();
	// }

	// Set the api url to use from the dynamic azure config.
	// const appConfigStore = useAppConfigStore(nuxtApp.$pinia);
	const config = useRuntimeConfig();

	// Use LOCAL_API_URL from the .env file if it's set, otherwise use the Azure App Configuration value.
	const localApiUrl = config.public.LOCAL_API_URL;
	const apiUrl = localApiUrl || appConfigStore.apiUrl;

	api.setApiUrl(apiUrl);

	// Set the auth configuration for MSAL from the dynamic azure config.
	setAuthConfig(appConfigStore.auth);
});
