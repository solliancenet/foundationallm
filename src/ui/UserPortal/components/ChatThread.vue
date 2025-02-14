<template>
	<div class="chat-thread">
		<!-- Message list -->
		<div
			ref="messageContainer"
			class="chat-thread__messages printable-section"
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
						v-for="(message, index) in messages.slice()"
						:id="`message-${getMessageOrderFromReversedIndex(index)}`"
						:key="message.renderId || message.id"
						:message="message"
						:show-word-animation="index === messages.length - 1 && message.sender !== 'User'"
						role="log"
						@rate="handleRateMessage($event.message)"
					/>
				</template>

				<!-- New chat alert -->
				<div v-else class="new-chat-alert">
					<div class="alert-body">
						<!-- eslint-disable-next-line vue/no-v-html -->
						<div class="alert-body-text" v-html="welcomeMessage"></div>
					</div>
				</div>
			</template>
		</div>

		<!-- Chat input -->
		<div class="chat-thread__input">
			<ChatInput ref="chatInput" :disabled="isLoading || isMessagePending" @send="handleSend" />
		</div>

		<!-- Footer -->
		<!-- eslint-disable-next-line vue/no-v-html -->
		<footer
			v-if="$appConfigStore.footerText"
			class="chat-thread__footer"
			v-html="$appConfigStore.footerText"
		/>

		<!-- File drag and drop -->
		<div v-if="isDragging" ref="dropZone" class="drop-files-here-container">
			<div class="drop-files-here">
				<i class="pi pi-upload" style="font-size: 2rem"></i>
				<div>Drop files here to upload</div>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import type { Message, Session } from '@/js/types';

export default {
	name: 'ChatThread',

	props: {
		isDragging: Boolean,
	},

	emits: ['session-updated'],

	data() {
		return {
			isLoading: true,
			userSentMessage: false,
			isMessagePending: false,
			welcomeMessage: '',
		};
	},

	computed: {
		currentSession() {
			return this.$appStore.currentSession;
		},

		pollingSession() {
			return this.$appStore.pollingSession;
		},

		lastSelectedAgent() {
			return this.$appStore.lastSelectedAgent;
		},

		messages() {
			return this.$appStore.currentMessages;
		},
	},

	watch: {
		async currentSession(newSession: Session, oldSession: Session) {
			const isReplacementForTempSession = oldSession?.is_temp && this.messages.length > 0;
			if (newSession.id === oldSession?.id || isReplacementForTempSession) return;
			this.isMessagePending = false;
			this.isLoading = true;
			this.userSentMessage = false;

			await this.$appStore.getMessages();
			await this.$appStore.ensureAgentsLoaded();

			this.$appStore.updateSessionAgentFromMessages(newSession);
			const sessionAgent = this.$appStore.getSessionAgent(newSession);
			this.welcomeMessage = this.getWelcomeMessage(sessionAgent);
			this.isLoading = false;
			this.scrollToBottom();
		},

		lastSelectedAgent(newAgent, oldAgent) {
			if (newAgent === oldAgent) return;
			this.welcomeMessage = this.getWelcomeMessage(newAgent);
		},

		pollingSession(newPollingSession, oldPollingSession) {
			if (newPollingSession === oldPollingSession) return;
			if (newPollingSession === this.currentSession.id) {
				this.isMessagePending = true;
			} else {
				this.isMessagePending = false;
			}
		},

		messages: {
			handler() {
				this.scrollToBottom();
			},
			deep: true,
		}
	},

	created() {
		if (!this.$appConfigStore.showLastConversionOnStartup && this.currentSession?.is_temp) {
			this.isLoading = false;
			const sessionAgent = this.$appStore.getSessionAgent(this.currentSession);
			this.welcomeMessage = this.getWelcomeMessage(sessionAgent);
		}
	},

	methods: {
		getWelcomeMessage(agent) {
			const welcomeMessage = agent?.resource?.properties?.welcome_message;
			return welcomeMessage && welcomeMessage.trim() !== ''
				? welcomeMessage
				: (this.$appConfigStore.defaultAgentWelcomeMessage ??
						'Start the conversation using the text box below.');
		},

		getMessageOrderFromReversedIndex(index) {
			return this.messages.length - 1 - index;
		},

		async handleRateMessage(message: Message) {
			await this.$appStore.rateMessage(message);
		},

		handleParentDrop(event) {
			event.preventDefault();
			const files = Array.from(event.dataTransfer?.files || []);
			this.$refs.chatInput.handleDrop(files);
		},

		async handleSend(text: string) {
			if (!text) return;

			this.isMessagePending = true;
			this.userSentMessage = true;

			const agent = this.$appStore.getSessionAgent(this.currentSession)?.resource;

			// Display an error toast message if agent is null or undefined.
			if (!agent) {
				this.$appStore.addToast({
					severity: 'info',
					summary: 'Could not send message',
					detail:
						'Please select an agent and try again. If no agents are available, refresh the page.',
					life: 8000,
				});
				this.isMessagePending = false;
				return;
			}

			// if (agent.long_running) {
			// 	// Handle long-running operations
			// 	const operationId = await this.$appStore.startLongRunningProcess('/async-completions', {
			// 		session_id: this.currentSession.id,
			// 		user_prompt: text,
			// 		agent_name: agent.name,
			// 		settings: null,
			// 		attachments: this.$appStore.attachments.map((attachment) => String(attachment.id)),
			// 	});

			// 	this.longRunningOperations.set(this.currentSession.id, true);
			// 	await this.pollForCompletion(this.currentSession.id, operationId);
			// } else {
			let waitForPolling = await this.$appStore.sendMessage(text);

			if (!waitForPolling) {
				this.isMessagePending = false;
			}
			// console.log(message);
			// await this.$appStore.getMessages();
			// }

			// this.isMessagePending = false;
		},

		scrollToBottom() {
			this.$nextTick(() => {
				this.$refs.messageContainer.scrollTop = this.$refs.messageContainer.scrollHeight;
			});
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
	flex-direction: column;
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

.chat-thread__footer {
	text-align: right;
	font-size: 0.85rem;
	padding-right: 24px;
	margin-bottom: 12px;

	:first-child {
		margin-top: 0px;
	}

	:last-child {
		margin-bottom: 0px;
	}
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
	padding: 10px 14px 10px 14px;
	// text-align: center;
	// font-style: italic;
}

.drop-files-here-container {
	position: absolute;
	top: 0;
	left: 0;
	width: 100%;
	height: 100%;
	background-color: rgba(255, 255, 255, 0.8);
	z-index: 9999;
	display: flex;
	justify-content: center;
	align-items: center;
}

.drop-files-here {
	display: flex;
	flex-direction: column;
	align-items: center;
	border-radius: 6px;
	gap: 2rem;
}
</style>
