<template>
	<main>
		<Head>
			<Title>{{ pageTitle }}</Title>
			<Meta name="description" :content="pageTitle" />
		</Head>

		<NuxtPage :style="style" />
	</main>
</template>

<script lang="ts">
import getAppConfigSetting from '@/js/config';

export default {
	data() {
		return {
			pageTitle: 'FoundationaLLM',
			style: {},
		};
	},

	async created() {
		this.pageTitle = await getAppConfigSetting('FoundationaLLM:Branding:PageTitle');
		this.style = {
			'--primary-bg': await getAppConfigSetting('FoundationaLLM:Branding:BackgroundColor'),
			'--primary-color': await getAppConfigSetting('FoundationaLLM:Branding:PrimaryColor'),
			'--secondary-color': await getAppConfigSetting('FoundationaLLM:Branding:SecondaryColor'),
			'--accent-color': await getAppConfigSetting('FoundationaLLM:Branding:AccentColor'),
			'--primary-text': await getAppConfigSetting('FoundationaLLM:Branding:PrimaryTextColor'),
			'--secondary-text': await getAppConfigSetting('FoundationaLLM:Branding:SecondaryTextColor'),
		};
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
</style>
