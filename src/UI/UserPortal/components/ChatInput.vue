<template>
	<div class="chat-input">
		<div class="pre-input">
			<div class="icon"></div>
			<div class="text"></div>
		</div>
		<div class="input-container">
			<input class="input" type="text" placeholder="What would you like to ask?"/>
		</div>
		<div class="submit">
			<div class="icon"></div>
			<div class="text">Submit</div>
		</div>
	</div>
</template>

<script>
export default {
	name: 'ChatInput',

	data() {
		return {};
	},
};
</script>

<style lang="scss" scoped>
.chat-input {
	display: flex;
	height: 200px;
	border: 1px solid blue;
	flex: 0 0 50px;
}

.pre-input {
	flex: 0 0 10%;
	border: 1px solid green;
}

.input-container {
	flex: 0 0 80%;
	border: 1px solid green;
}

.input {
	width: 100%;
	height: 100%;
}

.submit {
	flex: 0 0 10%;
	border: 1px solid green;
	text-align: center;
}

</style>
