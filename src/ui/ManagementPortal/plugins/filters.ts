const filters = {
	enforceLeadingSlash(path: string) {
		if (!path.startsWith('/')) {
			return '/' + path;
		} else {
			return path;
		}
	}
};

export default defineNuxtPlugin((nuxtApp) => {
	nuxtApp.vueApp.config.globalProperties.$filters = filters;
});
