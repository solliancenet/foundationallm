<template>
	<CreateDataSource :edit-id="$route.params.dataSourceName" />
</template>

<script lang="ts">
import CreateDataSource from '../create.vue';

export default {
	name: 'EditDataSource',
	components: {
		CreateDataSource,
	},
};
</script>
