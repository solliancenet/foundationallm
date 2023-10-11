<template>
	<div
		class="message-row"
		:class="{
			'message--in': !(message.sender === 'User'),
			'message--out': message.sender === 'User',
		}"
	>
		<div class="message">
			<div class="message__header">
				<span>
					<span class="icon"></span>
					<span>{{ message.sender }}</span>
				</span>

				<span>{{ message.timeStamp }}</span>
			</div>

			<div class="message__body">
				{{ message.text }}
			</div>

			<div class="message__footer" v-if="message.sender !== 'User'">
				<span
					:class="message.rating ? 'selected like' : 'like'"
					@click="handleRate(message, true)"
				>
					<span class="icon"></span>
					<button>Like</button>
				</span>

				<span
					:class="message.rating === false ? 'selected dislike' : 'dislike'"
					@click="handleRate(message, false)"
				>
					<span class="icon"></span>
					<button>Dislike</button>
				</span>

				<span class="view-prompt">
					<button>View Prompt</button>
				</span>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import type { PropType } from 'vue';
import { Message } from '@/js/types';

export default {
	name: 'ChatMessage',

	props: {
		message: {
			type: Object as PropType<Message>,
			required: true,
		},
	},

	emits: ['rate'],

	methods: {
		handleRate(message: Message, like: Boolean) {
			this.$emit('rate', { message, like });
		}
	},
};
</script>

<style lang="scss" scoped>
.message-row {
	display: flex;
	align-items: end;
	margin: 8px;
}

.message {
	padding: 16px;
	border-radius: 8px;
	width: 80%;
}

.message--in {
	.message {
		background-color: lightblue;
	}
}

.message--out {
	flex-direction: row-reverse;
	.message {
		background-color: lightgray;
	}
}

.message__header {
	margin-bottom: 8px;
	display: flex;
	justify-content: space-between;
}

.message__body {
	white-space: pre-wrap;
	overflow-wrap: break-word;
}

.message__footer {
	margin-top: 8px;
	display: flex;
	gap: 12px;
}
</style>
