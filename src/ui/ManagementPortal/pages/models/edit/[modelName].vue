<template>
	<CreateModel :edit-id="$route.params.modelName" />
</template>

<script lang="ts">
import CreateModel from '../create.vue';

export default {
	components: {
		CreateModel,
	},
};
</script>
