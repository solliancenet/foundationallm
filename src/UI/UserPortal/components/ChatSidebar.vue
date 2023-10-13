<template>
	<div class="chat-sidebar">
		<!-- Sidebar header -->
		<div class="chat-sidebar__header">
			<span>Chats</span>
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
					<!-- Chat name -->
					<span class="chat__name">{{ session.name }}</span>

					<!-- Chat icons -->
					<span class="chat__icons">
						<!-- Rename session -->
						<button small @click.stop="openRenameModal(session)">edit</button>

						<!-- Delete session -->
						<button small @click.stop="sessionToDelete = session">x</button>
					</span>
				</div>
			</div>
		</div>

		<!-- Rename session dialog -->
		<Dialog
			:visible="sessionToRename !== null"
			modal
			:header="`Rename Chat ${sessionToRename?.name}`"
			:closable="false"
			:style="{ width: '50vw' }"
		>
			<InputText
				v-model="newSessionName"
				type="text"
				placeholder="New chat name"
				:style="{ width: '100%' }"
			></InputText>
			<template #footer>
				<Button label="Cancel" text @click="closeRenameModal" />
				<Button label="Rename" @click="handleRenameSession" />
			</template>
		</Dialog>

		<!-- Delete session dialog -->
		<Dialog
			:visible="sessionToDelete !== null"
			modal
			header="Delete a Chat"
			:closable="false"
			:style="{ width: '50vw' }"
		>
			<p>Do you want to delete the chat "{{ sessionToDelete.name }}" ?</p>
			<template #footer>
				<Button label="Cancel" text @click="sessionToDelete = null" />
				<Button label="Delete" severity="danger" @click="handleDeleteSession" />
			</template>
		</Dialog>
	</div>
</template>

<script lang="ts">
import { Session } from '@/js/types';
import api from '~/server/api';

export default {
	name: 'ChatSidebar',

	emits: ['change-session'],

	data() {
		return {
			sessions: [] as Array<Session>,
			currentSession: null as Session | null,
			sessionToRename: null as Session | null,
			newSessionName: '' as string,
			sessionToDelete: null as Session | null,
		};
	},

	async created() {
		await this.getSessions();
		this.handleSessionSelected(this.sessions[0]);
	},

	methods: {
		openRenameModal(session: Session) {
			this.sessionToRename = session;
			this.newSessionName = session.name;
		},

		closeRenameModal() {
			this.sessionToRename = null;
			this.newSessionName = '';
		},

		async getSessions() {
			const session = await api.getSessions();
			this.sessions = session;
		},

		async handleRenameSession() {
			const updatedSession = await api.renameSession(this.sessionToRename!.id, this.newSessionName);
			const sessionIndex = this.sessions.findIndex(session => session.id === updatedSession.id);
			this.sessions[sessionIndex] = updatedSession;
			this.sessionToRename = null;
		},

		async handleAddSession() {
			const session = await api.addSession();
			this.handleSessionSelected(session);
			await this.getSessions();
		},

		handleSessionSelected(session: Session) {
			this.currentSession = session;
			this.$emit('change-session', session);
		},

		async handleDeleteSession() {
			await api.deleteSession(this.sessionToDelete!.id);
			this.sessionToDelete = null;
			await this.getSessions();
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
}

.chat--selected {
	background-color: lightgray;
}

.chat__name {
}

.chat__icons {
	flex-shrink: 0;
}
</style>
