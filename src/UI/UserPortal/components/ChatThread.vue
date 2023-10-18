<template>
	<div class="chat-thread">
		<!-- Header -->
		<div class="chat-thread__header" v-if="sidebarClosed">
			<template v-if="session">
				<span>{{ session.name }}</span>
			</template>
			<template v-else>
				<span>Please select a session</span>
			</template>
		</div>

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
						@rate="handleRateMessage(messages.length - 1 - index, $event)"
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
			<ChatInput @send="handleSend" />
		</div>
	</div>
</template>

<script lang="ts">
import type { PropType } from 'vue';
import { Message, Session } from '@/js/types';
import api from '~/server/api';

export default {
	name: 'ChatThread',

	props: {
		session: {
			type: Object as PropType<Session>,
			required: true,
		},
		sidebarClosed: {
			type: Boolean,
			required: false,
			default: false,
		},
	},

	emits: ['update-session'],

	data() {
		return {
			messages: [] as Array<Message>,
			isLoading: true,
		};
	},

	watch: {
		session() {
			this.getMessages();
		}
	},

	methods: {
		async getMessages() {
			this.isLoading = true;
			const data = await api.getMessages(this.session.id);
			this.messages = data;
			this.isLoading = false;
		},

		async handleRateMessage(messageIndex: number, { message, like }: { message: Message; like: Message['rating'] }) {
			const updatedMessage = await api.rateMessage(message, like);
			this.messages[messageIndex] = updatedMessage;
		},

		async handleSend(text: string) {
			const tempUserMessage: Message = {
				completionPromptId: null,
				id: '',
				rating: null,
				sender: 'User',
				sessionId: this.session.id,
				text,
				timeStamp: new Date().toISOString(),
				tokens: 0,
				type: 'Message',
				vector: [],
			};
			this.messages.push(tempUserMessage);

			await api.sendMessage(this.session.id, text);
			await this.getMessages();

			// Update the session name based on the message sent
			if (this.messages.length === 2) {
				const sessionFullText = this.messages.map((message) => message.text).join('\n');
				const { text: newSessionName } = await api.summarizeSessionName(this.session.id, sessionFullText);
				const updatedSession = await api.renameSession(this.session.id, newSessionName);
				this.$emit('update-session', updatedSession);
			}
		},
	}
};
</script>

<style lang="scss" scoped>
.chat-thread {
	height: 100%;
	display: flex;
	flex-direction: column;
	position: relative;
	flex: 1;
}

.chat-thread__header {
	height: 70px;
	padding: 24px;
	border-bottom: 1px solid #EAEAEA;
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
	padding: 24px;
}

.chat-thread__input {
	display: flex;
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
.alert-header, .alert-header > i {
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
</style>
