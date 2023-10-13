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
				v-for="(session, index) in sessions"
				:key="session.id"
				class="chat-sidebar__chat"
				@click="handleSessionSelected(session)"
			>
				<div
					class="chat"
					:class="{ 'chat--selected': currentSession?.id === session.id }"
				>
					<span class="chat__name">{{ session.name }}</span>

					<span class="chat__icons">
						<button small @click="openRenameModal(session)">edit</button>

						<!-- Rename session dialog -->
						<Dialog
							:visible="viewRenameSession === session.id"
							modal
							:header="`Rename Chat ${session.name}`"
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
								<Button label="Rename" @click="handleRenameSession(session, index)" />
							</template>
						</Dialog>

						<button small @click="deleteSession = session">x</button>
					</span>
				</div>
			</div>
		</div>

		<!-- Delete session dialog -->
		<Dialog
			:visible="deleteSession !== null"
			modal
			header="Delete a Chat"
			:closable="false"
			:style="{ width: '50vw' }"
		>
			<p>Do you want to delete the chat "{{ deleteSession.name }}" ?</p>
			<template #footer>
				<Button label="No" text @click="deleteSession = null" />
				<Button label="Yes" severity="danger" @click="handleDeleteSession" />
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
			currentSession: null as Session | null,
			deleteSession: null as Session | null,
			sessions: [] as Array<Session>,
			viewRenameSession: null as Session['id'] | null,
			newSessionName: '' as string,
		};
	},

	async created() {
		await this.getSessions();
		this.handleSessionSelected(this.sessions[0]);
	},

	methods: {
		openRenameModal(session: Session) {
			this.viewRenameSession = session.id;
			this.newSessionName = session.name;
		},

		closeRenameModal() {
			this.viewRenameSession = null;
			this.newSessionName = '';
		},

		async getSessions() {
			const session = await api.getSessions();
			this.sessions = session;
		},

		async handleRenameSession(session: Session, sessionIndex: number) {
			const updatedSession = await api.renameSession(session.id, this.newSessionName);
			this.sessions[sessionIndex] = updatedSession;
			this.viewRenameSession = false;
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
			await api.deleteSession(this.deleteSession!.id);
			this.deleteSession = null;
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
