<template>
	<div class="chat-thread">
		<!-- Message list -->
		<div
			ref="messageContainer"
			class="chat-thread__messages"
			:class="messages.length === 0 && 'empty'"
		>
			<template v-if="isLoading">
				<div class="chat-thread__loading" role="status">
					<i
						class="pi pi-spin pi-spinner"
						style="font-size: 2rem"
						role="img"
						aria-label="Loading"
					></i>
				</div>
			</template>

			<template v-else>
				<!-- Messages -->
				<template v-if="messages.length !== 0">
					<ChatMessage
						v-for="(message, index) in messages.slice().reverse()"
						:id="`message-${getMessageOrderFromReversedIndex(index)}`"
						:key="`${message.id}-${componentKey}`"
						:message="message"
						:show-word-animation="index === 0 && userSentMessage && message.sender === 'Assistant'"
						role="log"
						:aria-flowto="
							index === 0 ? null : `message-${getMessageOrderFromReversedIndex(index) + 1}`
						"
						@rate="handleRateMessage($event.message, $event.isLiked)"
					/>
				</template>

				<!-- New chat alert -->
				<div v-else class="new-chat-alert">
					<div class="alert-body">
						<div class="alert-body-text">Start the conversation using the text box below.</div>
					</div>
				</div>
			</template>
		</div>

		<!-- Chat input -->
		<div class="chat-thread__input">
			<ChatInput :disabled="isLoading || isMessagePending" @send="handleSend" />
		</div>

		<footer v-if="$appConfigStore.footerText">
			<!-- eslint-disable-next-line vue/no-v-html -->
			<div class="footer-item" v-html="$appConfigStore.footerText"></div>
		</footer>
	</div>
</template>

<script lang="ts">
import type { Message, Session } from '@/js/types';
import eventBus from '@/js/eventBus';

export default {
	name: 'ChatThread',

	emits: ['session-updated'],

	data() {
		return {
			isLoading: true,
			userSentMessage: false,
			isMessagePending: false,
			componentKey: 0,
			longRunningOperations: new Map<string, boolean>(), // sessionId -> isPending
		};
	},

	computed: {
		currentSession() {
			return this.$appStore.currentSession;
		},

		messages() {
			return this.$appStore.currentMessages;
		},
	},

	watch: {
		async currentSession(newSession: Session, oldSession: Session) {
			if (newSession.id === oldSession?.id) return;
			this.isLoading = true;
			this.userSentMessage = false;
			await this.$appStore.getMessages();
			this.isLoading = false;
		},
	},

	beforeUnmount() {
		eventBus.off('operation-completed', this.handleOperationCompleted);
	},

	mounted() {
		eventBus.on('operation-completed', this.handleOperationCompleted);
	},

	methods: {
		getMessageOrderFromReversedIndex(index) {
			return this.messages.length - 1 - index;
		},

		async handleRateMessage(message: Message, isLiked: Message['rating']) {
			await this.$appStore.rateMessage(message, isLiked);
		},

		async handleSend(text: string) {
			if (!text) return;

			this.isMessagePending = true;
			this.userSentMessage = true;

			const agent = this.$appStore.getSessionAgent(this.currentSession)?.resource;

			// Display an error toast message if agent is null or undefined.
			if (!agent) {
				this.$toast.add({
					severity: 'info',
					summary: 'Could not send message',
					detail:
						'Please select an agent and try again. If no agents are available, refresh the page.',
					life: 8000,
				});
				this.isMessagePending = false;
				return;
			}

			if (agent.long_running) {
				// Handle long-running operations
				const operationId = await this.$appStore.startLongRunningProcess('/completions', {
					session_id: this.currentSession.id,
					user_prompt: text,
					agent_name: agent.name,
					settings: null,
					attachments: this.$appStore.attachments.map((attachment) => String(attachment.id)),
				});

				this.longRunningOperations.set(this.currentSession.id, true);
				await this.pollForCompletion(this.currentSession.id, operationId);
			} else {
				await this.$appStore.sendMessage(text);
			}

			this.isMessagePending = false;
		},

		async pollForCompletion(sessionId: string, operationId: string) {
			while (true) {
				const status = await this.$appStore.checkProcessStatus(operationId);
				if (status.isCompleted) {
					this.longRunningOperations.set(sessionId, false);
					await this.$appStore.getMessages();
					break;
				}
				await new Promise((resolve) => setTimeout(resolve, 2000)); // Poll every 2 seconds
			}
		},

		async handleOperationCompleted({ sessionId }: { sessionId: string; operationId: string }) {
			if (this.currentSession.id === sessionId) {
				await this.$appStore.getMessages();
			}
		},
	},
};
</script>

<style lang="scss" scoped>
.chat-thread {
	height: 100%;
	max-width: 100%;
	display: flex;
	flex-direction: column;
	position: relative;
	flex: 1;
}

.chat-thread__header {
	height: 70px;
	padding: 24px;
	border-bottom: 1px solid #eaeaea;
	background-color: var(--accent-color);
}

.chat-thread__loading {
	width: 100%;
	height: 100%;
	display: flex;
	justify-content: center;
	align-items: center;
}

.chat-thread__messages {
	display: flex;
	flex-direction: column-reverse;
	flex: 1;
	overflow-y: auto;
	overscroll-behavior: auto;
	scrollbar-gutter: stable;
	padding: 24px 32px;
}

.chat-thread__input {
	display: flex;
	margin: 0px 24px 8px 24px;
	// box-shadow: 0 -5px 10px 0 rgba(27, 29, 33, 0.1);
}

.empty {
	flex-direction: column;
}

.new-chat-alert {
	background-color: #fafafa;
	margin: 10px;
	margin-left: auto;
	margin-right: auto;
	box-shadow: 0 5px 10px 0 rgba(27, 29, 33, 0.1);
	padding: 10px;
	border-radius: 6px;
	width: 55%;
}

.alert-header,
.alert-header > i {
	display: flex;
	align-items: center;
	font-size: 1.5rem;
}

.alert-header-text {
	font-weight: 500;
	margin-left: 8px;
}

.alert-body-text {
	color: #000;
	margin-left: auto;
	margin-right: auto;
	text-align: center;
	font-style: italic;
}

footer {
	text-align: right;
	font-size: 0.85rem;
	padding-right: 24px;
}
</style>
