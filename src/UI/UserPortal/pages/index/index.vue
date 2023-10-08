<template>
	<div class="chat-app">
		<ChatSidebar :sessions="sessions" @session-chosen="sessionChosen" @add-chat="onAddChat" />
		<ChatThread class="expand" :messages="messages" :session="selectedSession" />
	</div>
</template>

<script>
export default {
	name: 'Index',

	data() {
		return {
			status: null,
			sessions: null,
			selectedSession: null,
			messages: null,
		};
	},
	methods: {
		async getSessions() {
			return await useFetch('https://solliance-data-copilot-api.azurewebsites.net/sessions');
		},
		async sessionChosen(session) {
			const {data} = await useFetch(`https://solliance-data-copilot-api.azurewebsites.net/sessions/${session.id}/messages`);
			this.messages = data;
			this.selectedSession = session;
			console.log(this.messages);
		},
		async onAddChat() {
			const data = await $fetch('https://solliance-data-copilot-api.azurewebsites.net/sessions', {
				method: 'POST',
			});
			this.sessions.push(data);
			this.sessionChosen(data);
		},
	},
	async created() {
		const {data} = await this.getSessions();
		this.sessions = data;
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
}
</style>

<style lang="scss" scoped>
.chat-app {
	display: flex;
	height: 100%;
}

.expand {
	flex: 1;
}
</style>
