<template>
	<main>
		<Head>
			<Title>{{ pageTitle }}</Title>
			<Meta name="description" :content="pageTitle" />
			<Link rel="icon" type="image/x-icon" :href="iconLink" />
		</Head>

		<!-- Page to render -->
		<NuxtPage />

		<!-- Session expiration dialog -->
		<SessionExpirationDialog v-if="!$authStore.isExpired" />

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
			iconLink: this.$appConfigStore.favIconUrl || '/favicon.ico',
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

	watch: {
		style: {
			immediate: true,
			handler() {
				for (const cssVar in this.style) {
					document.documentElement.style.setProperty(cssVar, this.style[cssVar]);
				}
			},
		},
	},

	methods: {
		async handleRefreshLogin() {
			await this.$authStore.clearLocalSession();
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

.text--danger {
	color: red;
}

.d-flex {
	display: flex;
}

.justify-center {
	justify-content: center;
}

.p-component {
	border-radius: 0px;
}

.p-button-text {
	color: var(--primary-button-bg) !important;
}

.p-button:not(.p-button-text) {
	background-color: var(--primary-button-bg) !important;
	border-color: var(--primary-button-bg) !important;
	color: var(--primary-button-text) !important;
}

.p-button-secondary:not(.p-button-text) {
	background-color: var(--secondary-button-bg) !important;
	border-color: var(--secondary-button-bg) !important;
	color: var(--secondary-button-text) !important;
}
</style>
