<template>
	<div class="chat-thread">
		<!-- Header -->
		<div class="chat-thread__header">
			<template v-if="session">
				<span>{{ session.name }}</span>
			</template>
			<template v-else>
				<span>Please select a session</span>
			</template>
		</div>

		<!-- Messages -->
		<div :class="messages.length !== 0 ? 'chat-thread__messages' : 'chat-thread__messages empty'">
			<ChatMessage
				v-if="messages.length !== 0"
				v-for="(message, index) in messages.slice().reverse()"
				:key="message.id"
				:message="message"
				@rate="handleRateMessage(messages.length - 1 - index, $event)"
			/>
			<div v-else class="new-chat-alert">
				<div class="alert-header">
					<i class="pi pi-exclamation-circle"></i>
					<span class="alert-header-text">Get Started</span>
				</div>
				<div class="alert-body">
					<span class="alert-body-text">How can I help?</span>
				</div>
			</div>
		</div>

		<!-- Chat input -->
		<ChatInput @send="handleSend" />
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
	},

	data() {
		return {
			messages: [] as Array<Message>,
		};
	},

	watch: {
		session() {
			this.getMessages();
		}
	},

	async created() {
		await this.getMessages();
	},

	methods: {
		async getMessages() {
			const data = await api.getMessages(this.session.id);
			this.messages = data;
		},

		async handleRateMessage(messageIndex: number, { message, like }: { message: Message; like: boolean }) {
			const updatedMessage = await api.rateMessage(message, like);
			this.messages[messageIndex] = updatedMessage;
		},

		async handleSend(text: string) {
			await api.sendMessage(this.session.id, text);
			await this.getMessages();
		},
	},
};
</script>

<style lang="scss" scoped>
.chat-thread {
	height: 100%;
	display: flex;
	flex-direction: column;
	position: relative;
	flex: 1
}

.chat-thread__header {
	height: 70px;
	padding: 24px;
	border-bottom: 1px solid gray;
	background-color: rgba(32, 32, 32, 1);
	color: white;
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
