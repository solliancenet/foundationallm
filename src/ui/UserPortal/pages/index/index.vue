<template>
	<div class="chat-app">
		<NavBar />
		<div class="chat-content">
			<ChatSidebar v-show="!appStore.isSidebarClosed" class="chat-sidebar" />
			<div v-show="!appStore.isSidebarClosed" class="sidebar-blur" @click="appStore.toggleSidebar" />
			<ChatThread />
		</div>

		<!-- Authentication error dialog -->
		<Dialog
			class="sidebar-dialog"
			:visible="$route.query.auth_expired === true"
			modal
			header="Authentication Error"
			:closable="false"
		>
			<p>Failed to authenticate. Please try signing in again.</p>
			<template #footer>
				<Button label="Go to Sign In" severity="primary" @click="$router.push({ name: 'auth/login' })" />
			</template>
		</Dialog>
	</div>
</template>

<script lang="ts">
import { mapStores } from 'pinia';
import { useAppStore } from '@/stores/appStore';

export default {
	name: 'Index',

	computed: {
		...mapStores(useAppStore),
	},
};
</script>

<style lang="scss" scoped>
.chat-app {
	display: flex;
	flex-direction: column;
	height: 100vh;
	background-color: var(--primary-bg);
}
.chat-content {
	display: flex;
	flex-direction: row;
	height: calc(100% - 70px);
	background-color: var(--primary-bg);
}

@media only screen and (max-width: 620px) {
	.sidebar-blur {
		position:absolute;
		width: 100%;
		height: 100%;
		z-index: 2;
		top: 0px;
		left: 0px;
		backdrop-filter: blur(3px);
	}
}

@media only screen and (max-width: 950px) {
    .chat-sidebar {
        position: absolute;
		top: 0px;
        box-shadow: 5px 0px 10px rgba(0, 0, 0, 0.4)
    }
}
</style>
