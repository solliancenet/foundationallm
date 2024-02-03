import fs from 'fs';

const buildLoadingTemplate = (() => {
	const path = 'server/buildLoadingTemplate.html';

	try {
		const data = fs.readFileSync(path, 'utf8');
		return data;
	} catch (error) {
		console.error('Error reading build loading template!', error);
		return null;
	}
})();

// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
	ssr: false,
	devtools: { enabled: true },
	modules: ['@pinia/nuxt'],
	components: true,
	app: {
		head: {
			title: process.env.BRANDING_PAGE_TITLE ?? 'FoundationaLLM Management Portal',
			link: [
				{
					rel: 'icon',
					type: 'image/x-icon',
					href: process.env.NUXT_APP_BASE_URL + (process.env.BRANDING_FAV_ICON_URL ?? 'favicon.ico'),
				},
			],
		},
		baseURL: process.env.BASE_URL || '/',
	},
	routeRules: {
		'*': { ssr: false },
	},
	css: [
		'primevue/resources/themes/viva-light/theme.css',
		'~/styles/fonts.scss',
		'primeicons/primeicons.css',
	],
	build: {
		transpile: ['primevue'],
	},
	devServer: {
		...(buildLoadingTemplate
			? {
					loadingTemplate: () => buildLoadingTemplate,
				}
			: {}),
		port: 3001,
	},
	runtimeConfig: {
		APP_CONFIG_ENDPOINT: process.env.APP_CONFIG_ENDPOINT,
	},
});
