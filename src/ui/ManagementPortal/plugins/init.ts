import { defineNuxtPlugin } from '#app';
import { useAppConfigStore, useAuthStore, useAppStore } from '@/stores';
import { setAuthConfig } from '@/js/auth';
import api from '@/js/api';

export default defineNuxtPlugin(async (nuxtApp: any) => {
	// Load config variables into the app config store
	const appConfigStore = useAppConfigStore(nuxtApp.$pinia);
	await appConfigStore.getConfigVariables();

	api.setApiUrl(appConfigStore.apiUrl);
	api.setInstanceId(appConfigStore.instanceId);

	// Make stores globally accessible on the nuxt app instance
	nuxtApp.provide('appConfigStore', appConfigStore);

	const authStore = await useAuthStore(nuxtApp.$pinia).init();
	nuxtApp.provide('authStore', authStore);

	const appStore = useAppStore(nuxtApp.$pinia);
	nuxtApp.provide('appStore', appStore);
});
