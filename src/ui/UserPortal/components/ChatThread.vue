<template>
	<div class="chat-thread">
		<!-- Message list -->
		<div class="chat-thread__messages" :class="messages.length === 0 && 'empty'">
			<template v-if="isLoading">
				<div class="chat-thread__loading">
					<i class="pi pi-spin pi-spinner" style="font-size: 2rem"></i>
				</div>
			</template>

			<template v-else>
				<!-- Messages -->
				<template v-if="messages.length !== 0">
					<ChatMessage
						v-for="(message, index) in messages.slice().reverse()"
						:key="message.id"
						:message="message"
						:show-word-animation="index === 0 && userSentMessage && message.sender === 'Assistant'"
						@rate="handleRateMessage($event.message, $event.isLiked)"
					/>
				</template>

				<!-- New chat alert -->
				<div v-else class="new-chat-alert">
					<div class="alert-header">
						<i class="pi pi-exclamation-circle"></i>
						<span class="alert-header-text">Get Started</span>
					</div>
					<div class="alert-body">
						<span class="alert-body-text">How can I help?</span>
					</div>
				</div>
			</template>
		</div>

		<!-- Chat input -->
		<div class="chat-thread__input">
			<ChatInput :disabled="isLoading || isMessagePending" @send="handleSend" />
		</div>

		<Footer v-if="$appConfigStore.footerText">
			<FooterItem v-html="$appConfigStore.footerText"></FooterItem>
		</Footer>
	</div>
</template>

<script lang="ts">
import type { Message, Session } from '@/js/types';

export default {
	name: 'ChatThread',

	emits: ['session-updated'],

	data() {
		return {
			isLoading: true,
			userSentMessage: false,
			isMessagePending: false,
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

	methods: {
		async handleRateMessage(message: Message, isLiked: Message['rating']) {
			await this.$appStore.rateMessage(message, isLiked);
		},

		async handleSend(text: string) {
			if (!text) return;

			this.isMessagePending = true;
			this.userSentMessage = true;
			await this.$appStore.sendMessage(text);
			this.isMessagePending = false;
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
	background-color: #d9f0d1;
	margin: 10px;
	padding: 10px;
	border-radius: 6px;
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
	font-size: 1.2rem;
	font-weight: 300;
	font-style: italic;
}

footer {
	text-align: right;
	font-size: 0.85rem;
    padding-right: 24px;
}
</style>
