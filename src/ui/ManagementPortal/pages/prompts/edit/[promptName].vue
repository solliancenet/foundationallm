<template>
	<CreatePrompt edit-prompt="true" :prompt-name="$route.params.promptName" />
</template>

<script lang="ts">
import CreatePrompt from '../create.vue';

export default {
	components: {
		CreatePrompt,
	},
};
</script>
