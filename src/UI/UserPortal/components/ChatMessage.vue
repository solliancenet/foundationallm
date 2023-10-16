<template>
	<div
		class="message-row"
		:class="message.sender === 'User' ? 'message--out' : 'message--in'"
	>
		<div class="message">
			<div class="message__header">
				<!-- Sender -->
				<span class="header__sender">
					<img v-if="message.sender !== 'User'" class="avatar" src="~/assets/brain-royalty-free.png">
					<span>{{ message.sender }}</span>
				</span>

				<!-- Timestamp -->
				<span>{{ $filters.timeAgo(new Date(message.timeStamp)) }}</span>
			</div>

			<!-- Message text -->
			<div class="message__body">
				{{ message.text }}
			</div>

			<div class="message__footer" v-if="message.sender !== 'User'">
				<span class="ratings">
					<!-- Like -->
					<span>
						<Button
							size="small"
							text
							:icon="message.rating ? 'pi pi-thumbs-up-fill' : 'pi pi-thumbs-up'"
							:label="message.rating ? 'Message Liked!' : 'Like'"
							@click.stop="handleRate(message, true)"
						/>
					</span>

					<!-- Dislike -->
					<span>
						<Button
							size="small"
							text
							:icon="message.rating === false ? 'pi pi-thumbs-down-fill' : 'pi pi-thumbs-down'"
							:label="message.rating === false ? 'Message Disliked.' : 'Dislike'"
							@click.stop="handleRate(message, false)"
						/>
					</span>
				</span>

				<!-- View prompt -->
				<span class="view-prompt">
					<Button
						size="small"
						text
						icon="pi pi-book"
						label="View Prompt"
						@click.stop="handleViewPrompt"
					/>

					<!-- Prompt dialog -->
					<Dialog
						:visible="viewPrompt"
						modal
						header="Completion Prompt"
						:closable="false"
						:style="{ width: '50vw' }"
					>
						<p class="prompt-text">{{ prompt.prompt }}</p>
						<template #footer>
							<Button label="Close" @click="viewPrompt = false" />
						</template>
					</Dialog>
				</span>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import type { PropType } from 'vue';
import { Message, CompletionPrompt } from '@/js/types';
import api from '~/server/api';

export default {
	name: 'ChatMessage',

	props: {
		message: {
			type: Object as PropType<Message>,
			required: true,
		},
	},

	emits: ['rate'],

	data() {
		return {
			prompt: {} as CompletionPrompt,
			viewPrompt: false,
		};
	},

	methods: {
		handleRate(message: Message, like: boolean) {
			this.$emit('rate', { message, like: message.rating === like ? null : like })
		},

		async handleViewPrompt() {
			const prompt = await api.getPrompt(this.message.sessionId, this.message.completionPromptId);
			this.prompt = prompt;
			this.viewPrompt = true;
		}
	},
};
</script>

<style lang="scss" scoped>
.message-row {
	display: flex;
	align-items: flex-end;
	margin: 8px;
}

.message {
	padding: 16px;
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
		background-color: var(--primary-color);
		color: var(--primary-text)
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

.ratings {
	display: flex;
	gap: 16px;
}

.icon {
	margin-right: 4px;
	cursor: pointer;
}

.view-prompt {
	cursor: pointer;
}

.dislike {
	margin-left: 12px;
	cursor: pointer;
}

.like {
	cursor: pointer;
}

.prompt-text {
	white-space: pre-wrap;
	overflow-wrap: break-word;
}
</style>
