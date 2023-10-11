// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
	devtools: { enabled: true },
	components: true,
	css: [
		'primevue/resources/themes/viva-light/theme.css',
		'~/styles/fonts.scss',
	],
	build: {
		transpile: ['primevue'],
	},
	vite: {
		define: {
			API_URL: JSON.stringify(process.env.API_URL),
		},
	},
});
