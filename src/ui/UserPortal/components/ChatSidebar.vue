<template>
	<div class="chat-sidebar">
		<!-- Sidebar section header -->
		<div class="chat-sidebar__section-header--mobile">
			<img
				v-if="$appConfigStore.logoUrl !== ''"
				:src="$filters.enforceLeadingSlash($appConfigStore.logoUrl)"
				:alt="$appConfigStore.logoText"
			/>
			<span v-else>{{ $appConfigStore.logoText }}</span>
			<VTooltip :auto-hide="isMobile" :popper-triggers="isMobile ? [] : ['hover']">
				<Button
					:icon="$appStore.isSidebarClosed ? 'pi pi-arrow-right' : 'pi pi-arrow-left'"
					size="small"
					severity="secondary"
					aria-label="Toggle sidebar"
					:aria-expanded="!$appStore.isSidebarClosed"
					@click="$appStore.toggleSidebar"
					@keydown.esc="hideAllPoppers"
				/>
				<template #popper><div role="tooltip">Toggle sidebar</div></template>
			</VTooltip>
		</div>
		<div class="chat-sidebar__section-header">
			<h2 class="chat-sidebar__section-header__text">Chats</h2>
			<VTooltip :auto-hide="isMobile" :popper-triggers="isMobile ? [] : ['hover']">
				<Button
					icon="pi pi-plus"
					text
					severity="secondary"
					aria-label="Add new chat"
					:disabled="createProcessing"
					@keydown.esc="hideAllPoppers"
					@click="handleAddSession"
				/>
				<template #popper><div role="tooltip">Add new chat</div></template>
			</VTooltip>
		</div>

		<!-- Chats -->
		<div class="chat-sidebar__chats">
			<nav>
				<ul role="list" class="chat-list">
					<li v-if="!sessions" role="listitem" class="chat-list-item">No sessions</li>
					<li
						v-for="session in sessions"
						:key="session.id"
						class="chat-sidebar__chat chat-list-item"
						role="listitem"
						@click="handleSessionSelected(session)"
						@keydown.enter="handleSessionSelected(session)"
					>
						<div class="chat" :class="{ 'chat--selected': currentSession?.id === session.id }">
							<!-- Chat name -->

							<VTooltip :auto-hide="isMobile" :popper-triggers="isMobile ? [] : ['hover']">
								<span class="chat__name" tabindex="0" @keydown.esc="hideAllPoppers">{{
									session.display_name
								}}</span>
								<template #popper>
									<div role="tooltip">
										{{ session.display_name }}
									</div>
								</template>
							</VTooltip>

							<!-- Chat icons -->
							<span v-if="currentSession?.id === session.id" class="chat__icons">
								<!-- Rename session -->
								<VTooltip :auto-hide="isMobile" :popper-triggers="isMobile ? [] : ['hover']">
									<Button
										icon="pi pi-pencil"
										size="small"
										severity="secondary"
										text
										aria-label="Rename chat session"
										@click.stop="openRenameModal(session)"
										@keydown.esc="hideAllPoppers"
									/>
									<template #popper><div role="tooltip">Rename chat session</div></template>
								</VTooltip>

								<!-- Delete session -->
								<VTooltip :auto-hide="isMobile" :popper-triggers="isMobile ? [] : ['hover']">
									<Button
										icon="pi pi-trash"
										size="small"
										severity="danger"
										text
										aria-label="Delete chat session"
										@click.stop="sessionToDelete = session"
										@keydown.esc="hideAllPoppers"
									/>
									<template #popper><div role="tooltip">Delete chat session</div></template>
								</VTooltip>
							</span>
						</div>
					</li>
				</ul>
			</nav>
		</div>

		<!-- Logged in user -->
		<div v-if="$authStore.currentAccount?.name" class="chat-sidebar__account">
			<UserAvatar size="large" class="chat-sidebar__avatar" />

			<div>
				<span class="chat-sidebar__username">{{ $authStore.currentAccount?.name }}</span>
				<Button
					class="chat-sidebar__sign-out"
					icon="pi pi-sign-out"
					label="Sign Out"
					severity="secondary"
					size="small"
					@click="$authStore.logout()"
				/>
			</div>
		</div>

		<!-- Rename session dialog -->
		<Dialog
			v-if="sessionToRename !== null"
			v-focustrap
			:visible="sessionToRename !== null"
			:header="`Rename Chat ${sessionToRename?.display_name}`"
			:closable="false"
			class="sidebar-dialog"
			modal
		>
			<InputText
				v-model="newSessionName"
				:style="{ width: '100%' }"
				type="text"
				placeholder="New chat name"
				aria-label="New chat name"
				autofocus
				@keydown="renameSessionInputKeydown"
			></InputText>
			<template #footer>
				<Button label="Cancel" text @click="closeRenameModal" />
				<Button label="Rename" @click="handleRenameSession" />
			</template>
		</Dialog>

		<!-- Delete session dialog -->
		<Dialog
			v-if="sessionToDelete !== null"
			v-focustrap
			:visible="sessionToDelete !== null"
			:closable="false"
			class="sidebar-dialog"
			modal
			header="Delete a Chat"
			@keydown="deleteSessionKeydown"
		>
			<div v-if="deleteProcessing" class="delete-dialog-content">
				<div role="status">
					<i
						class="pi pi-spin pi-spinner"
						style="font-size: 2rem"
						role="img"
						aria-label="Loading"
					></i>
				</div>
			</div>
			<div v-else>
				<p>Do you want to delete the chat "{{ sessionToDelete.display_name }}" ?</p>
			</div>
			<template #footer>
				<Button label="Cancel" text :disabled="deleteProcessing" @click="sessionToDelete = null" />
				<Button
					label="Delete"
					severity="danger"
					autofocus
					:disabled="deleteProcessing"
					@click="handleDeleteSession"
				/>
			</template>
		</Dialog>
	</div>
</template>

<script lang="ts">
import { hideAllPoppers } from 'floating-vue';
import type { Session } from '@/js/types';
declare const process: any;

export default {
	name: 'ChatSidebar',

	data() {
		return {
			sessionToRename: null as Session | null,
			newSessionName: '' as string,
			sessionToDelete: null as Session | null,
			deleteProcessing: false,
			isMobile: window.screen.width < 950,
			createProcessing: false,
			debounceTimeout: null as NodeJS.Timeout | null,
		};
	},

	computed: {
		sessions() {
			return this.$appStore.sessions;
		},

		currentSession() {
			return this.$appStore.currentSession;
		},
	},

	async created() {
		if (window.screen.width < 950) {
			this.$appStore.isSidebarClosed = true;
		}

		if (process.client) {
			await this.$appStore.init(this.$nuxt._route.query.chat);
		}
	},

	methods: {
		openRenameModal(session: Session) {
			this.sessionToRename = session;
			this.newSessionName = session.display_name;
		},

		closeRenameModal() {
			this.sessionToRename = null;
			this.newSessionName = '';
		},

		handleSessionSelected(session: Session) {
			this.$appStore.changeSession(session);
		},

		async handleAddSession() {
			if (this.createProcessing) return;

			if (this.debounceTimeout) {
				this.$toast.add({
					severity: 'warn',
					summary: 'Warning',
					detail: 'Please wait before creating another session.',
					life: 3000,
				});
				return;
			}

			this.createProcessing = true;

			try {
				const newSession = await this.$appStore.addSession();
				this.handleSessionSelected(newSession);

				this.debounceTimeout = setTimeout(() => {
					this.debounceTimeout = null;
				}, 2000);
			} catch (error) {
				this.$toast.add({
					severity: 'error',
					summary: 'Error',
					detail: 'Could not create a new session. Please try again.',
					life: 5000,
				});
			} finally {
				this.createProcessing = false; // Re-enable the button
			}
		},

		handleRenameSession() {
			this.$appStore.renameSession(this.sessionToRename!, this.newSessionName);
			this.sessionToRename = null;
		},

		async handleDeleteSession() {
			this.deleteProcessing = true;
			try {
				await this.$appStore.deleteSession(this.sessionToDelete!);
				this.sessionToDelete = null;
			} catch (error) {
				this.$toast.add({
					severity: 'error',
					summary: 'Error',
					detail: 'Could not delete the session. Please try again.',
					life: 5000,
				});
			} finally {
				this.deleteProcessing = false;
			}
		},

		renameSessionInputKeydown(event: KeyboardEvent) {
			if (event.key === 'Enter') {
				this.handleRenameSession();
			}
			if (event.key === 'Escape') {
				this.closeRenameModal();
			}
		},

		deleteSessionKeydown(event: KeyboardEvent) {
			if (event.key === 'Escape') {
				this.sessionToDelete = null;
			}
		},

		hideAllPoppers() {
			hideAllPoppers();
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
	flex: 1;
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

.chat-sidebar__section-header__text {
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
	font-size: 0.8125rem;
	font-weight: 400;
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

.delete-dialog-content {
	display: flex;
	justify-content: center;
	padding: 20px 150px;
}

ul.chat-list {
	list-style-type: none;
	padding-left: 0;
	margin: 0;
}

li.chat-list-item {
	padding: 0;
	margin: 0;
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
