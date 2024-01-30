<template>
	<div class="chat-sidebar">
		<!-- Sidebar section header -->
		<div class="chat-sidebar__section-header--mobile">
			<img v-if="appConfigStore.logoUrl !== ''" :src="$filters.enforceLeadingSlash(appConfigStore.logoUrl)" />
			<span v-else>{{ appConfigStore.logoText }}</span>
			<Button
				:icon="appStore.isSidebarClosed ? 'pi pi-arrow-right' : 'pi pi-arrow-left'"
				size="small"
				severity="secondary"
				class="secondary-button"
				@click="appStore.toggleSidebar"
			/>
		</div>
		<div class="chat-sidebar__section-header">
			<span>Chats</span>
			<!-- <button @click="handleAddSession">
				<span class="text">+</span>
			</button> -->
			<Button icon="pi pi-plus" text severity="secondary" @click="handleAddSession" />
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
				<div class="chat" :class="{ 'chat--selected': currentSession?.id === session.id }">
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

		<!-- Logged in user -->
		<div v-if="accountName" class="chat-sidebar__account">
			<Avatar icon="pi pi-user" class="chat-sidebar__avatar" size="large" />
			<div>
				<span class="chat-sidebar__username">{{ accountName }}</span>
				<Button
					class="chat-sidebar__sign-out secondary-button"
					icon="pi pi-sign-out"
					label="Sign Out"
					severity="secondary"
					size="small"
					@click="signOut()"
				/>
			</div>
		</div>

		<!-- Rename session dialog -->
		<Dialog
			class="sidebar-dialog"
			:visible="sessionToRename !== null"
			modal
			:header="`Rename Chat ${sessionToRename?.name}`"
			:closable="false"
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
			class="sidebar-dialog"
			:visible="sessionToDelete !== null"
			modal
			header="Delete a Chat"
			:closable="false"
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
import { mapStores } from 'pinia';
import type { Session } from '@/js/types';
import { useAppConfigStore } from '@/stores/appConfigStore';
import { useAppStore } from '@/stores/appStore';
import { getMsalInstance } from '@/js/auth';
declare const process: any;

export default {
	name: 'ChatSidebar',

	data() {
		return {
			sessionToRename: null as Session | null,
			newSessionName: '' as string,
			sessionToDelete: null as Session | null,
			accountName: '' as string,
			userName: '' as string,
		};
	},

	computed: {
		...mapStores(useAppConfigStore),
		...mapStores(useAppStore),

		sessions() {
			return this.appStore.sessions;
		},

		currentSession() {
			return this.appStore.currentSession;
		},
	},

	async created() {
		if (window.screen.width < 950) {
			this.appStore.isSidebarClosed = true;
		}

		if (process.client) {
			await this.appStore.init(this.$nuxt._route.query.chat);
			const msalInstance = await getMsalInstance();
			const accounts = await msalInstance.getAllAccounts();
			if (accounts.length > 0) {
				this.accountName = accounts[0].name;
				this.userName = accounts[0].username;
			}
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

		handleSessionSelected(session: Session) {
			this.appStore.changeSession(session);
		},

		async handleAddSession() {
			const newSession = await this.appStore.addSession();
			this.handleSessionSelected(newSession);
		},

		handleRenameSession() {
			this.appStore.renameSession(this.sessionToRename!, this.newSessionName);
			this.sessionToRename = null;
		},

		async handleDeleteSession() {
			await this.appStore.deleteSession(this.sessionToDelete!);
			this.sessionToDelete = null;
		},

		async signOut() {
			const msalInstance = await getMsalInstance();
			const accountFilter = {
				username: this.userName,
			};
			const logoutRequest = {
				account: msalInstance.getAccount(accountFilter),
			};
			await msalInstance.logoutRedirect(logoutRequest);
			this.$router.push({ path: '/login' });
		},
	},
};
</script>

<style lang="scss" scoped>
.chat-sidebar {
	width: 300px;
	max-width: 100%;
	height: 100%;
	display: flex;
	flex-direction: column;
	background-color: var(--primary-color);
	z-index: 3;
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

.chat-sidebar__section-header--mobile {
	display: none;
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

.chat-sidebar__account {
	display: grid;
	grid-template-columns: auto auto;
	padding: 12px 24px;
	justify-content: flex-start;
	text-transform: inherit;
}

.chat-sidebar__avatar {
	margin-right: 12px;
	color: var(--primary-color);
	height: 61px;
	width: 61px;
}
.chat-sidebar__sign-out {
	width: 100%;
}

.secondary-button {
	background-color: var(--secondary-button-bg)!important;
	border-color: var(--secondary-button-bg)!important;
	color: var(--secondary-button-text)!important;
}

.chat-sidebar__username {
	color: var(--primary-text);
	font-weight: 600;
	font-size: 0.875rem;
	text-transform: capitalize;
	line-height: 0;
	vertical-align: super;
}

.p-overlaypanel-content {
	background-color: var(--primary-color);
}

.overlay-panel__option {
	display: flex;
	align-items: center;
	cursor: pointer;
}

.overlay-panel__option:hover {
	color: var(--primary-color);
}

@media only screen and (max-width: 950px) {
	.chat-sidebar__section-header--mobile {
		height: 70px;
		padding: 12px 24px;
		display: flex;
		justify-content: space-between;
		align-items: center;
		img {
			max-height: 100%;
			width: auto;
			max-width: 148px;
			margin-right: 12px;
		}
	}
}
</style>

<style lang="scss">
@media only screen and (max-width: 950px) {
	.sidebar-dialog {
		width: 95vw;
	}
}
</style>
