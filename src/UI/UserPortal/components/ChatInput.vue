<template>
	<div class="chat-input">
		<div class="pre-input">
			<div class="icon"></div>
			<div class="text"></div>
		</div>
		<InputText
			v-model="text"
			class="input"
			type="text"
			placeholder="What would you like to ask?"
			@keydown.enter="handleSend"
		></InputText>
		<div class="submit">
			<div class="icon"></div>
			<Button @click="handleSend">Send</Button>
		</div>
	</div>
</template>

<script lang="ts">
export default {
	name: 'ChatInput',

	emits: ['send'],

	data() {
		return {
			text: '' as string,
		};
	},

	methods: {
		handleSend() {
			this.$emit('send', this.text);
			this.text = '';
		},
	},
};
</script>

<style lang="scss" scoped>
.chat-input {
	display: flex;
	background-color: white;
	border-radius: 8px;
}

.pre-input {
	flex: 0 0 10%;
}

.input {
	width: 100%;
	height: 100%;
	height: 64px;
	transition: height 0.25s ease-in-out;
}

.input:focus {
	// height: 192px;
}

.submit {
	flex: 0 0 10%;
	text-align: center;
}
</style>
