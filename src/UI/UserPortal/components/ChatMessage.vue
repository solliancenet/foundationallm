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
				<span class="header__sender">
					<img v-if="message.sender !== 'User'" class="avatar" src="~/assets/brain-royalty-free.png">
					<span>{{ message.sender }}</span>
				</span>

				<span>{{ message.timeStamp }}</span>
			</div>

			<div class="message__body">
				{{ message.text }}
			</div>

			<div class="message__footer" v-if="message.sender !== 'User'">
				<div class="message__votes">
					<span
						:class="message.rating ? 'selected like' : 'like'"
						@click="handleRate(message, true)"
					>
						<i :class="message.rating ? 'icon pi pi-thumbs-up-fill' : 'icon pi pi-thumbs-up'"></i>
						<span>Like</span>
					</span>

					<span
						:class="message.rating === false ? 'selected dislike' : 'dislike'"
						@click="handleRate(message, false)"
					>
						<i :class="message.rating === false ? 'icon pi pi-thumbs-down-fill' : 'icon pi pi-thumbs-down'"></i>
						<span>Dislike</span>
					</span>
				</div>

				<div class="view-prompt">
					<i class="icon pi pi-book"></i>
					<span>View Prompt</span>
				</div>
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
	box-shadow: 0 5px 10px 0 rgba(27, 29, 33, 0.1);
}

.message--in {
	.message {
		background-color: rgba(250, 250, 250, 1);
	}
}

.message--out {
	flex-direction: row-reverse;
	.message {
		background-color: rgba(233, 235, 249, 1);
	}
}

.message__header {
	margin-bottom: 12px;
	display: flex;
	justify-content: space-between;
}

.message__body {
	white-space: pre-wrap;
	overflow-wrap: break-word;
}

.message__footer {
	margin-top: 12px;
	display: flex;
	justify-content: space-between;
}

.header__sender {
	display: flex;
	align-items: center;
}

.avatar {
	width: 32px;
	height: 32px;
	border-radius: 50%;
	margin-right: 12px;
}

.view-prompt {
	cursor: pointer;
}

.icon {
	margin-right: 4px;
}

.dislike {
	margin-left: 12px;
	cursor: pointer;
}

.like {
	cursor: pointer;
}
</style>
