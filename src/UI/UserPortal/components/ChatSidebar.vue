<template>
	<div class="chat-sidebar">
		<h1>ChatSidebar</h1>
		<div class="add-chat" @click="$emit('addChat', true)">
			<div class="icon"></div>
			<div class="text">Add Chat</div>
		</div>
		<div v-if="!sessions">No sessions</div>
		<div v-else v-for="(session, index) in sessions" :key="index" @click="$emit('sessionChosen', session)" class="sidebar-sessions">
			<h2>{{ session.name }}</h2>
			<h3>Tokens: {{ session.tokensUsed }}</h3>
		</div>
	</div>
</template>

<script lang="ts">
export default {
	name: 'ChatSidebar',

	props: {
		sessions: {
			type: Array,
			required: true,
		},
	},
};
</script>

<style lang="scss" scoped>
.chat-sidebar {
	width: 320px;
	height: 100%;
	overflow: scroll;
	border-right: 1px solid gray;
	display: flex;
	flex-direction: column;
}

.add-chat {
	display: flex;
	flex-direction: row;
	align-items: center;
	justify-content: center;
	height: 50px;
	border: 1px solid rgb(99, 255, 180);
	cursor: pointer;
}

.sidebar-sessions {
	border: 1px solid rgb(99, 255, 180);
	cursor: pointer;
}
</style>
