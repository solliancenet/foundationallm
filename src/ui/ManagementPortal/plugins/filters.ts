const filters = {
	publicDirectory(path: string) {
		const config = useRuntimeConfig();
		return config.app.baseURL + path;
	}
};

export default defineNuxtPlugin((nuxtApp) => {
	nuxtApp.vueApp.config.globalProperties.$filters = filters;
});
