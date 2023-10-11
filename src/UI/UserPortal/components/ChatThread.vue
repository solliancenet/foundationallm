<template>
	<div class="chat-thread">
		<div class="chat-thread__header">
			<template v-if="session">
				<h1>{{ session.name }}</h1>
				<h2>{{ session.tokensUsed }} tokens used</h2>
			</template>
			<template v-else>
				<h1>No session selected</h1>
				<h2>Please select a session</h2>
			</template>
		</div>
		<div class="chat-thread__messages">
			<div v-for="(message, index) in messages" :key="index" :class="message.sender === 'User' ? 'user-message message' : 'message'">
				<div class="message-header">
					<div class="header-sender">
						<div class="icon"></div>
						<div class="sender">{{ message.sender }}</div>
					</div>
					<div class="header-tokens">
						Tokens: {{ message.tokens }}
					</div>
					<div class="header-timestamp">
						{{ message.timeStamp }}
					</div>
				</div>
				<div class="message-body">
					<div class="body-text">
						{{ message.text }}
					</div>
				</div>
				<div class="message-footer">
					<div class="footer-votes" v-if="message.sender !== 'User'">
						<div :class="message.rating ? 'selected like' : 'like'" @click="rateMessage(message, true)">
							<div class="icon"></div>
							<div>Like</div>
						</div>
						<div :class="message.rating === false ? 'selected dislike' : 'dislike'" @click="rateMessage(message, false)">
							<div class="icon"></div>
							<div>Dislike</div>
						</div>
					</div>
					<div class="view-prompt">
						<div class="view-prompt-button">
							<div class="icon"></div>
							<div class="view-prompt-text">View Prompt</div>
						</div>
					</div>
				</div>
			</div>
		</div>
		<ChatInput />
	</div>
</template>

<script>
export default {
	name: 'ChatThread',

	props: {
		messages: {
			type: Array,
			required: true,
		},
		session: {
			type: Object,
			required: true,
		},
	},
	data() {
		return {};
	},
	methods: {
		async rateMessage(message, rating) {
			message.rating === rating ? message.rating = null : message.rating = rating;
			const data = await $fetch(`${this.$config.public.BASE_URL}/sessions/${message.sessionId}/message/${message.id}/rate${message.rating !== null ? '?rating=' + message.rating : ''}`, {
				method: 'POST'
			});
			console.log(data);
		},
	},
};
</script>

<style lang="scss" scoped>
.chat-thread {
	display: flex;
	flex-direction: column;
}

.chat-thread__messages {
	flex: 1;
	border: 1px solid red;
	overflow: scroll;
}

.message {
	border: 1px solid rgb(255, 99, 99);
	border-radius: 15px;
	background-color: rgb(29, 207, 91);
	margin: 20px 250px 20px 20px;
}

.user-message {
	background-color: rgb(83, 89, 255);
	margin: 20px 20px 20px 250px;
}

.message-header {
	display: flex;
	align-items: center;
	justify-content: flex-end;
	padding: 10px 15px;
	border-bottom: 1px solid rgb(232, 99, 255);
}

.header-sender {
	flex: 1 0 65%;
}

.sender {
	font-weight: bold;
}

.header-tokens {
	background-color: #fff;
	border-radius: 5px;
	margin-right: 15px;
	padding: 5px 5px;
}

.header-timestamp {
	text-align: right;
}

.message-body {
	padding: 15px 15px;
}

.message-footer {
	display: flex;
    justify-content: space-between;
	padding: 10px 15px;
	border-top: 1px solid rgb(232, 99, 255);
}

.footer-votes {
	display: flex;
	cursor: pointer;
}

.dislike {
	margin-left: 10px;
}

.selected {
	background-color: rgb(232, 99, 255);
}

.view-prompt {
    flex: 1;
	display: flex;
    justify-content: flex-end;
}
.view-prompt-button {
	cursor: pointer;
	background-color: rgb(232, 99, 255);
}

.chat-thread__header {
	border: 1px solid rgb(232, 99, 255);
}
</style>
