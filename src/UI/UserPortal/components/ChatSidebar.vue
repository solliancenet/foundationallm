<template>
	<div class="chat-sidebar">
		<!-- Sidebar header -->
		<div class="chat-sidebar__header">
			<span>Sessions</span>
			<button @click="handleAddSession">
				<span class="text">+</span>
			</button>
		</div>

		<!-- Chats -->
		<div class="chat-sidebar__chats">
			<div v-if="!sessions">No sessions</div>
			<div
				v-for="session in sessions"
				:key="session.id"
				class="chat-sidebar__chat"
				@click="handleSessionSelected(session)"
			>
				<div
					class="chat"
					:class="{ 'chat--selected': currentSession?.id === session.id }"
				>
					<span class="chat__name">{{ session.name }}</span>
					<div class="chat-options">
						<span class="option edit">
							<i class="icon pi pi-pencil"></i>
						</span>
						<span class="option delete">
							<i class="icon pi pi-trash"></i>
						</span>
					</div>
				</div>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import { Session } from '@/js/types';

export default {
	name: 'ChatSidebar',

	props: {
		sessions: {
			type: Array<Session>,
			required: true,
		},
	},

	emits: ['add-session', 'session-selected'],

	data() {
		return {
			currentSession: null as Session | null,
		};
	},

	methods: {
		handleAddSession() {
			this.$emit('add-session');
		},

		handleSessionSelected(session: Session) {
			this.currentSession = session;
			this.$emit('session-selected', session);
		},
	},
};
</script>

<style lang="scss" scoped>

.chat-sidebar {
	width: 300px;
	height: 100%;
	border-right: 1px solid gray;
	display: flex;
	flex-direction: column;
}

.chat-sidebar__header {
	height: 70px;
	padding: 24px;
	border-bottom: 1px solid gray;
	display: flex;
	justify-content: space-between;
	background-color: rgba(32, 32, 32, 1);
	color: white;
}

.chat-sidebar__chats {
	flex: 1;
	overflow-y: auto;
}

.chat {
	border-radius: 8px;
	margin: 24px;
	padding: 12px;
	display: flex;
    justify-content: space-between;
    align-items: center;
}

.chat-options {
	display: flex;
}

.chat--selected {
	background-color: lightgray;
}

.chat--selected .option {
	background-color: rgba(245, 245, 245, 1);
}
.option {
	background-color: rgba(220, 220, 220, 1);
	padding: 4px;
    border-radius: 3px;
}

.option:hover {
	background-color: rgba(200, 200, 200, 1);
	cursor: pointer;
}

.delete {
	margin-left: 8px;
}

.chat--selected .option:hover {
	background-color: rgba(180, 180, 180, 1);
}
</style>
