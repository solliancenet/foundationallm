import { defineNuxtPlugin } from '#app';
import { useAppConfigStore } from '@/stores/appConfigStore';
import { useAuthStore } from '@/stores/authStore';
import { useAppStore } from '@/stores/appStore';
import { setAuthConfig } from '@/js/auth';
import api from '@/js/api';

export default defineNuxtPlugin(async (nuxtApp: any) => {
	// Load config variables into the app config store
	const appConfigStore = useAppConfigStore(nuxtApp.$pinia);
	await appConfigStore.getConfigVariables();

	api.setApiUrl(appConfigStore.apiUrl);
	api.setInstanceId(appConfigStore.instanceId);

	// Set the auth configuration for MSAL from the dynamic azure config.
	setAuthConfig(appConfigStore.auth);

	// Provide global properties to the app
	nuxtApp.provide('appConfigStore', appConfigStore);

	const authStore = useAuthStore(nuxtApp.$pinia);
	nuxtApp.provide('authStore', authStore);

	const appStore = useAppStore(nuxtApp.$pinia);
	nuxtApp.provide('appStore', authStore);
});
