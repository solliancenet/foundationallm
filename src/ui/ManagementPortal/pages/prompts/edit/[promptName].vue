<template>
	<CreatePrompt :edit-prompt="$route.params.promptName" />
</template>

<script lang="ts">
import CreatePrompt from '../create.vue';

export default {
	components: {
		CreatePrompt,
	},
};
</script>
