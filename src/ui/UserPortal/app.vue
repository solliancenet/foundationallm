<template>
	<main>
		<Head>
			<Title>{{ pageTitle }}</Title>
			<Meta name="description" :content="pageTitle" />
		</Head>

		<!-- Page to render -->
		<NuxtPage :style="style" />

		<!-- Session expired dialog -->
		<Dialog
			modal
			:visible="$authStore.isExpired && $route.name !== 'auth/login'"
			:closable="false"
			header="Your session has expired."
		>
			Please log in again to continue using the app.
			<template #footer>
				<Button label="Log in" primary @click="handleRefreshLogin" />
			</template>
		</Dialog>
	</main>
</template>

<script lang="ts">
export default {
	data() {
		return {
			pageTitle: this.$appConfigStore.pageTitle || 'FoundationaLLM',
		};
	},

	computed: {
		style() {
			return {
				'--primary-bg': this.$appConfigStore.primaryBg,
				'--primary-color': this.$appConfigStore.primaryColor,
				'--secondary-color': this.$appConfigStore.secondaryColor,
				'--accent-color': this.$appConfigStore.accentColor,
				'--primary-text': this.$appConfigStore.primaryText,
				'--secondary-text': this.$appConfigStore.secondaryText,
				'--accent-text': this.$appConfigStore.accentText,
				'--primary-button-bg': this.$appConfigStore.primaryButtonBg,
				'--primary-button-text': this.$appConfigStore.primaryButtonText,
				'--secondary-button-bg': this.$appConfigStore.secondaryButtonBg,
				'--secondary-button-text': this.$appConfigStore.secondaryButtonText,
			};
		},
	},

	methods: {
		async handleRefreshLogin() {
			await this.$authStore.logoutSilent();
			this.$router.push({ name: 'auth/login' });
		},
	},
};
</script>

<style lang="scss">
html,
body,
#__nuxt,
#__layout,
main {
	height: 100%;
	margin: 0;
	font-family: 'Poppins', sans-serif;
}

.p-component {
	border-radius: 0px;
}
</style>
