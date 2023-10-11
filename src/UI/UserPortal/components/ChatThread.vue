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
		<div class="chat-thread__messages">
			<ChatMessage
				v-for="(message, index) in messages.slice().reverse()"
				:key="message.id"
				:message="message"
				@rate="handleRateMessage(index, $event)"
			/>
		</div>

		<!-- Chat input -->
		<ChatInput />
	</div>
</template>

<script lang="ts">
import type { PropType } from 'vue';
import { Message, Session } from '@/js/types';

export default {
	name: 'ChatThread',

	props: {
		messages: {
			type: Array<Message>,
			required: true,
		},

		session: {
			type: Object as PropType<Session>,
			required: true,
		},
	},

	methods: {
		async rateMessage(message: Message, rating: Message['rating']) {
			message.rating === rating
				? (message.rating = null)
				: (message.rating = rating);

			const data = (await $fetch(
				`${this.$config.public.API_URL}/sessions/${message.sessionId}/message/${
					message.id
				}/rate${message.rating !== null ? '?rating=' + message.rating : ''}`,
				{
					method: 'POST',
				},
			)) as Message;
		},

		async handleRateMessage(messageIndex, { message, like }: { message: Message; like: boolean }) {
			const data = await this.rateMessage(message, like);
			this.messages[messageIndex] = data;
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
	padding: 24px;
	border-bottom: 1px solid gray;
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
</style>
