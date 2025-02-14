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
				'--app-text-size': `${this.$appStore.textSize}rem`,
				'--app-contrast': this.$appStore.highContrastMode ? '2' : '1',
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
body {
	font-size: var(--app-text-size, 1rem);
}

html {
	filter: contrast(var(--app-contrast, 1));
}

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

@media print {
	// @font-face {
    //     font-family: 'KaTeX_Main';
    //     src: url('https://cdnjs.cloudflare.com/ajax/libs/KaTeX/0.13.11/fonts/KaTeX_Main-Regular.woff2') format('woff2'),
    //          url('https://cdnjs.cloudflare.com/ajax/libs/KaTeX/0.13.11/fonts/KaTeX_Main-Regular.woff') format('woff');
    // }

	body, html {
		margin: 0;
		padding: 0;
	}

	.chat-thread__messages {
		display: flex !important;
		height: auto !important;
		align-items: stretch !important;
	}

	* {
		overflow: visible !important;
		height: auto !important;
		print-color-adjust: exact;
	}

	.message__copy, .message__footer {
		display: none !important;
	}

	main, .chat-app {
		height: auto !important;
	}

	header, aside, footer, .chat-thread__input, .drop-files-here-container, .print-button {
		display: none !important;
	}

	.message-row {
		// page-break-inside: avoid;
		// page-break-after: auto;
		margin-bottom: 1rem;
	}

	.message__body {
		overflow-wrap: break-word !important;
        word-break: break-word !important;
		white-space: normal !important;
	}

	.v-popper__popper {
		display: none !important;
	}

	.katex {
        font-size: inherit !important; /* Adjust font size to fit */
        line-height: normal !important;
    }

	.katex .vlist > span > span {
        display: inline !important; /* Fixes spacing issue */
    }

	.katex-display {
        display: block !important;
        margin: 0 !important;
        padding: 0 !important;
        text-align: left !important; /* Ensure alignment */
    }

	.katex-block {
        margin-top: 1rem !important;
		margin-bottom: 1rem !important;
        padding: 0 !important;
        line-height: normal !important;
    }
}

</style>
