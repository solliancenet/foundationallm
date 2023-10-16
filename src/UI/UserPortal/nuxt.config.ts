// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
	devtools: { enabled: true },
	components: true,
	app: {
		head: {
			title: process.env.BRANDING_COMPANY_NAME ?? 'FoundationaLLM',
			link: [
				{
					rel: 'icon',
					type: 'image/x-icon',
					href: process.env.BRANDING_FAV_ICON_URL ?? '/favicon.png',
				},
			],
		},
	},
	css: [
		'primevue/resources/themes/viva-light/theme.css',
		'~/styles/fonts.scss',
		'primeicons/primeicons.css',
	],
	build: {
		transpile: ['primevue'],
	},
	vite: {
		define: {
			API_URL: JSON.stringify(process.env.API_URL),
		},
	},
	runtimeConfig: {
		public: {
			LOGO_URL: process.env.BRANDING_LOGO_URL,
			LOGO_TEXT: process.env.BRANDING_LOGO_TEXT,
			BRANDING_PRIMARY_COLOR: process.env.BRANDING_PRIMARY_COLOR,
			BRANDING_SECONDARY_COLOR: process.env.BRANDING_SECONDARY_COLOR,
			BRANDING_ACCENT_COLOR: process.env.BRANDING_ACCENT_COLOR,
			BRANDING_BACKGROUND_COLOR: process.env.BRANDING_BACKGROUND_COLOR,
		},
	},
});
