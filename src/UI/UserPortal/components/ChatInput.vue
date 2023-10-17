<template>
	<div class="chat-input p-inputgroup">
		<InputText
			v-model="text"
			class="input"
			type="text"
			placeholder="What would you like to ask?"
			@keydown.enter="handleSend"
		/>
		<Button class="submit" icon="pi pi-send" label="Send" @click="handleSend" />
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
	margin: 16px;
	width: 100%;
}

.pre-input {
	flex: 0 0 10%;
}

.input {
	width: 100%;
	height: 100%;
	height: 64px;
}

.input:focus {
	// height: 192px;
}

.submit {
	flex: 0 0 10%;
	text-align: left;
	flex-basis: auto;
}
</style>
