<template>
	<div class="chat-thread">
		<div class="chat-thread__messages">
			<h1>ChatThread</h1>
		</div>

		<ChatInput />
	</div>
</template>

<script>
export default {
	name: 'ChatThread',

	data() {
		return {};
	},
};
</script>

<style lang="scss" scoped>
.chat-thread {
	display: flex;
	flex-direction: column;
}

.chat-thread__messages {
	flex: 1;
	border: 1px solid red;
}
</style>
