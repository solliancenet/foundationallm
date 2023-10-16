<template>
	<div :style="style" class="chat-app">
		<ChatSidebar @change-session="handleChangeSession" />
		<ChatThread :session="currentSession" />
	</div>
</template>

<script lang="ts">
import { Session } from '@/js/types';

export default {
	name: 'Index',

	data() {
		return {
			currentSession: {} as Session,
		};
	},

	computed: {
		style() {
			return {
				'--primary-bg': this.$config.public.BRANDING_BACKGROUND_COLOR,
				'--primary-color': this.$config.public.BRANDING_PRIMARY_COLOR,
				'--secondary-color': this.$config.public.BRANDING_SECONDARY_COLOR,
				'--accent-color': this.$config.public.BRANDING_ACCENT_COLOR,
			};
		}
	},

	methods: {
		handleChangeSession(session: Session) {
			this.currentSession = session;
		},
	},
};
</script>

<style lang="scss">
:root {
	--primary-text: white;
}

html,
body,
#__nuxt,
#__layout {
	height: 100%;
	margin: 0;
	font-family: 'Poppins', sans-serif;
}
</style>

<style lang="scss" scoped>
.chat-app {
	display: flex;
	height: 100vh;
	background-color: var(--primary-bg);
}
</style>
