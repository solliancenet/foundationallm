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
	compatibilityDate: '2024-08-27',
	ssr: false,
	devtools: { enabled: true },
	modules: ['@pinia/nuxt', 'floating-vue/nuxt', '@nuxtjs/tailwindcss'],
	tailwindcss: {
		config: {
			corePlugins: {
				preflight: false,
			},
		},
	},
	components: true,
	app: {
		head: {
			htmlAttrs: {
				lang: 'en',
			},
			title: process.env.BRANDING_PAGE_TITLE ?? 'FoundationaLLM Management Portal',
			link: [
				{
					rel: 'icon',
					type: 'image/x-icon',
					href:
						process.env.NUXT_APP_BASE_URL !== undefined
							? process.env.NUXT_APP_BASE_URL
							: '' + (process.env.BRANDING_FAV_ICON_URL ?? 'favicon.ico'),
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
		...(buildLoadingTemplate ? { loadingTemplate: () => buildLoadingTemplate } : {}),
		port: 3001,
	},
	runtimeConfig: {
		APP_CONFIG_ENDPOINT: process.env.APP_CONFIG_ENDPOINT,
		public: {
			LOCAL_API_URL: process.env.LOCAL_API_URL,
		},
	},
});
