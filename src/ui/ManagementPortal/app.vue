<template>
	<Head>
		<Title>{{ pageTitle }}</Title>
		<Meta name="description" :content="pageTitle" />
		<Link rel="icon" type="image/x-icon" :href="iconLink" />
	</Head>

	<div class="main-content">
		<NuxtLayout>
			<NuxtPage />
		</NuxtLayout>
		<Toast position="top-center" />
	</div>
</template>

<script lang="ts">
export default {
	data() {
		return {
			pageTitle: 'FoundationaLLM Management',
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
};
</script>

<style lang="scss">
html,
body,
#__nuxt,
#__layout,
.main-content {
	height: 100%;
	margin: 0;
	font-family: 'Poppins', sans-serif;
}

.wrapper {
	display: flex;
	flex-direction: row;
	height: 100vh;
	flex-wrap: wrap;
}

.page {
	padding: 32px;
	padding-top: 24px;
	padding-bottom: 24px;
	overflow-y: auto;
	max-height: 100%;
	flex-grow: 1;
}

.page-header {
	margin-top: 0px;
	margin-bottom: 8px;
}

.page-subheader {
	margin-bottom: 24px;
}

.p-component {
	border-radius: 0px;
}

.p-button-text {
	color: var(--primary-button-bg) !important;
}

.p-button:not(.p-button-text):not(.p-togglebutton) {
	background-color: var(--primary-button-bg);
	border-color: var(--primary-button-bg);
	color: var(--primary-button-text);

	& > * {
		color: var(--primary-button-text);
	}
}

.p-button-secondary:not(.p-button-text):not(.p-togglebutton) {
	background-color: var(--secondary-button-bg);
	border-color: var(--secondary-button-bg);
	color: var(--secondary-button-text);

	& > * {
		color: var(--secondary-button-text);
	}
}

.p-togglebutton:not(.p-button-text) {
	background-color: var(--primary-button-bg);
	border-color: var(--primary-button-bg);
	color: var(--primary-button-text);

	& > * {
		color: var(--primary-button-text);
	}
}

.p-datatable-header {
	padding-left: 0;
	padding-right: 0;
}

:root {
	--jse-theme-color: var(--primary-button-bg, #6366f1); /* Default theme color */
}
</style>
