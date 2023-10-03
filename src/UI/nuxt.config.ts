// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
	devtools: { enabled: true },
	components: true,
	css: ['primevue/resources/themes/viva-light/theme.css'],
	build: {
		transpile: ['primevue'],
	},
});
