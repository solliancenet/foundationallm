<template>
	<div class="chat-app">
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

	methods: {
		handleChangeSession(session: Session) {
			this.currentSession = session;
		},
	},
};
</script>

<style>
html,
body,
#__nuxt,
#__layout {
	height: 100%;
	margin: 0;
	font-family: 'Nunito', sans-serif;
}
</style>

<style lang="scss" scoped>
.chat-app {
	display: flex;
	height: 100vh;
	background-color: rgba(242, 242, 242, 1);
}
</style>
