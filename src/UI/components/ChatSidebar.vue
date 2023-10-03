<template>
	<div class="chat-sidebar">
		<h1>ChatSidebar</h1>
		<h2 v-for="(chat, index) in chats" :key="index">
			{{ chat.title }}
		</h2>
	</div>
</template>

<script lang="ts">
export default {
	name: 'ChatSidebar',

	data() {
		return {
			chats: [{ title: 'Chat 1' }, { title: 'Chat 2' }],
		};
	},
};
</script>

<style lang="scss" scoped>
.chat-sidebar {
	width: 320px;
	height: 100%;
	border-right: 1px solid gray;
	display: flex;
	flex-direction: column;
}
</style>
