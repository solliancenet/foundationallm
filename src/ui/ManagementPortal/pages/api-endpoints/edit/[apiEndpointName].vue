<template>
	<CreateAPIEndpoint :edit-id="$route.params.apiEndpointName" />
</template>

<script lang="ts">
import CreateAPIEndpoint from '../create.vue';

export default {
	components: {
		CreateAPIEndpoint,
	},
};
</script>
