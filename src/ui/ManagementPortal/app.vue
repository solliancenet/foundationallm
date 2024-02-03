<template>
	<main :style="style">
		<Head>
			<Title>{{ pageTitle }}</Title>
			<Meta name="description" :content="pageTitle" />
		</Head>

		<NuxtLayout>
			<NuxtPage  />
		</NuxtLayout>

		<Toast position="top-center" />
	</main>
</template>

<script lang="ts">
export default {
	data() {
		return {
			pageTitle: 'FoundationaLLM Management',
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

.wrapper {
	display: flex;
	flex-direction: row;
	height: 100%;
}

.page {
	padding: 32px;
	padding-top: 24px;
	padding-bottom: 24px;
	overflow: scroll;
	flex-grow: 1;
}

.page-header {
	margin-top: 0px;
	margin-bottom: 8px;
}

.page-subheader {
	margin-bottom: 24px;
}

.column-2 {
	grid-column: 2;
}

.span-2 {
	grid-column: span 2;
}

.ml-1 {
	margin-left: 4px !important;
}

.mr-1 {
	margin-right: 4px !important;
}

.mt-2 {
	margin-top: 8px !important;
}

.mb-1 {
	margin-bottom: 4px !important;
}

.mb-2 {
	margin-bottom: 8px !important;
}

.w-100 {
	width: 100%;
}

.h-100 {
	height: 100%;
}

.d-flex {
	display: flex;
}

.align-center {
	align-items: center;
}

.justify-content-end {
	justify-content: end;
}

.justify-self-end {
	justify-self: end;
}

.cursor-pointer {
	cursor: pointer;
}
</style>

