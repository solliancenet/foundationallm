<template>
	<div class="chat-sidebar">
		<!-- Sidebar section header -->
		<div class="chat-sidebar__section-header">
			<span>Chats</span>
			<!-- <button @click="handleAddSession">
				<span class="text">+</span>
			</button> -->
			<Button
				icon="pi pi-plus"
				text
				severity="secondary"
				@click="handleAddSession"
			/>
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
					<span v-if="currentSession?.id === session.id" class="chat__icons">
						<!-- Rename session -->
						<Button
							icon="pi pi-pencil"
							size="small"
							severity="secondary"
							text
							@click.stop="openRenameModal(session)"
						/>

						<!-- Delete session -->
						<Button
							icon="pi pi-trash"
							size="small"
							severity="danger"
							text
							@click.stop="sessionToDelete = session"
						/>
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
import type { PropType } from 'vue';
import { Session } from '@/js/types';
import api from '@/js/api';

export default {
	name: 'ChatSidebar',

	props: {
		currentSession: {
			type: [Object, null] as PropType<Session | null>,
			required: true,
		}
	},

	emits: ['change-session', 'session-updated'],

	expose: ['getSessions'],

	data() {
		return {
			sessions: [] as Array<Session>,
			sessionToRename: null as Session | null,
			newSessionName: '' as string,
			sessionToDelete: null as Session | null,
		};
	},

	async created() {
		if (process.client) {
			await this.getSessions();
			const sessionId = this.$nuxt._route.query.chat;
			const existingSession = this.sessions.find((session: Session) => session.id === sessionId);
			this.handleSessionSelected(existingSession || this.sessions[0]);
		}
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
			await api.renameSession(this.sessionToRename!.id, this.newSessionName);
			this.sessionToRename!.name = this.newSessionName;
			this.$emit('session-updated', this.sessionToRename);
			this.sessionToRename = null;
		},

		async handleAddSession() {
			const session = await api.addSession();
			this.handleSessionSelected(session);
			await this.getSessions();
		},

		handleSessionSelected(session: Session) {
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
	display: flex;
	flex-direction: column;
	background-color: var(--primary-color);
}

.chat-sidebar__header {
	height: 70px;
	width: 100%;
	padding-right: 24px;
	padding-left: 24px;
	padding-top: 12px;
	display: flex;
	align-items: center;
	color: var(--primary-text);

	img {
		max-height: 100%;
		width: auto;
		max-width: 148px;
		margin-right: 12px;
	}
}

.chat-sidebar__section-header {
	height: 64px;
	padding: 24px;
	padding-bottom: 12px;
	display: flex;
	justify-content: space-between;
	align-items: center;
	color: var(--primary-text);
	text-transform: uppercase;
	// font-size: 14px;
	font-size: 0.875rem;
	font-weight: 600;
}

.chat-sidebar__chats {
	flex: 1;
	overflow-y: auto;
}

.chat {
	padding: 24px;
	display: flex;
	justify-content: space-between;
	align-items: center;
	color: var(--primary-text);
	transition: all 0.1s ease-in-out;
	font-size: 13px;
	font-size: 0.8125rem;
	height: 72px;
}

.chat__name {
	white-space: nowrap;
	overflow: hidden;
	text-overflow: ellipsis;
}

.chat__icons {
	display: flex;
	justify-content: space-between;
}

.chat:hover {
	background-color: rgba(217, 217, 217, 0.05);
}
.chat--selected {
	color: var(--secondary-text);
	background-color: var(--secondary-color);
	border-left: 4px solid rgba(217, 217, 217, 0.5);
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

.chat__name {
	cursor: pointer;
}

.chat__icons {
	flex-shrink: 0;
	margin-left: 12px;
}
</style>
