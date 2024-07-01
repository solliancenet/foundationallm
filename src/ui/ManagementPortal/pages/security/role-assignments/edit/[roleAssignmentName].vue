<template>
	<CreateRoleAssignment :edit-id="$route.params.roleAssignmentName" />
</template>

<script lang="ts">
import CreateRoleAssignment from '../create.vue';

export default {
	components: {
		CreateRoleAssignment,
	},
};
</script>
