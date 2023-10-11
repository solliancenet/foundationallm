<template>
	<div class="chat-app">
		<ChatSidebar
			:sessions="sessions"
			@session-selected="handleSessionSelected"
			@add-session="handleAddSession"
		/>
		<ChatThread :messages="messages" :session="selectedSession" />
	</div>
</template>

<script lang="ts">
import { Message, Session } from '@/js/types';
import api from '~/server/api';

export default {
	name: 'Index',

	data() {
		return {
			sessions: [] as Array<Session>,
			selectedSession: {} as Session,
			messages: [] as Array<Message>,
		};
	},

	async created() {
		const data = await api.getSessions();
		this.sessions = data;
		this.selectedSession = data[0];
		await this.handleSessionSelected(this.selectedSession);
	},

	methods: {
		async handleSessionSelected(session: Session) {
			const data = await api.getMessages(session.id);
			this.messages = data;
			this.selectedSession = session;
		},

		async handleAddSession() {
			const data = await api.addSession();
			this.sessions.push(data);
			this.handleSessionSelected(data);
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
