import { defineNuxtPlugin } from '#app';
import { useAppConfigStore } from '@/stores/appConfigStore';
import api from '@/js/api';

export default defineNuxtPlugin(async (nuxtApp: any) => {
	// Load config variables into the app config store
	const appConfigStore = useAppConfigStore(nuxtApp.$pinia);
	await appConfigStore.getConfigVariables();

	api.setApiUrl(appConfigStore.apiUrl);
	api.setInstanceId(appConfigStore.instanceId);

	// Provide global properties to the app
	nuxtApp.provide('appConfigStore', appConfigStore);
});
