<template>
	<!-- <div v-html="value" /> -->
	<component :is="compiledMarkdownComponent" />
</template>

<script>
import CodeBlockHeader from '@/components/CodeBlockHeader.vue';

function addCodeHeaderComponents(htmlString) {
	const parser = new DOMParser();
	const doc = parser.parseFromString(htmlString, 'text/html');

	doc.querySelectorAll('pre code').forEach((element) => {
		const languageClass = element.getAttribute('class');
		const encodedCode = element.getAttribute('data-code');
		// const autoDetectLanguage = element.getAttribute('data-language');
		const languageMatch = languageClass.match(/language-(\w+)/);
		const language = languageMatch ? languageMatch[1] : 'plaintext';

		const header = document.createElement('div');
		header.innerHTML = `<code-block-header language="${language}" codecontent="${encodedCode}"></code-block-header>`;

		element.parentNode.insertBefore(header.firstChild, element);
	});

	const html = doc.body.innerHTML;
	const withVueCurlyBracesSanitized = html
		.replace(/{{/g, '&#123;&#123;')
		.replace(/}}/g, '&#125;&#125;');

	return withVueCurlyBracesSanitized;
}

export default {
	props: {
		value: {
			type: String,
			required: true,
		},
	},

	computed: {
		compiledMarkdownComponent() {
			return {
				template: `<div>${addCodeHeaderComponents(this.value)}</div>`,
				components: {
					CodeBlockHeader,
				},
			};
		},
	},
};
</script>