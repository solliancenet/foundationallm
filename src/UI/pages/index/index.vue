<template>
	<div class="chat-app">
		<ChatSidebar />
		<ChatThread class="expand" />
	</div>
</template>

<script>
export default {
	name: 'Index',

	data() {
		return {};
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
