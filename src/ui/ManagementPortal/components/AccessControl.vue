<template>
	<!-- Trigger button -->
	<div style="display: flex; align-items: center">
		<Button @click="dialogOpen = true">
			<i class="pi pi-lock" style="color: var(--text-primary); margin-right: 8px"></i>
			Access Control
		</Button>
	</div>

	<!-- RBAC dialog -->
	<Dialog
		v-model:visible="dialogOpen"
		modal
		:header="currentStep === STEPS.CREATE_STEP ? 'Create Role Assignment' : 'Access Control'"
		:style="{ minWidth: '70%' }"
		@hide="handleClose"
	>
		<!-- Table step -->
		<template v-if="currentStep === STEPS.TABLE_STEP">

			<!-- Multiple scopes -->
			<TabView v-if="scopes.length > 1" v-model:activeIndex="currentScopeIndex" lazy>
				<TabPanel v-for="scope in scopes" :key="scope.value" :header="scope.label">
					<RoleAssignmentsTable :scope="currentScope.value" />
				</TabPanel>
			</TabView>

			<!-- Single scope -->
			<RoleAssignmentsTable v-else :scope="currentScope.value" />
		</template>

		<!-- Create step -->
		<template v-if="currentStep === STEPS.CREATE_STEP">
			<CreateRoleAssignment ref="createForm" headless :scope="currentScope.value" />
		</template>

		<!-- Footer -->
		<template #footer>
			<div class="mt-4">
				<!-- Table step buttons -->
				<template v-if="currentStep === STEPS.TABLE_STEP">
					<Button label="Close" text @click="handleClose" class="pt-2" />

					<Button v-if="currentStep === STEPS.TABLE_STEP" @click="currentStep = STEPS.CREATE_STEP">
						<i class="pi pi-plus" style="color: var(--text-primary); margin-right: 8px"></i>
						Add role assignment for this {{ currentScope.label }}
					</Button>
				</template>

				<!-- Create step buttons -->
				<template v-else-if="currentStep === STEPS.CREATE_STEP">
					<Button label="Back" text @click="currentStep = STEPS.TABLE_STEP" />

					<Button @click="handleCreateRoleAssignment">
						<i class="pi pi-plus" style="color: var(--text-primary); margin-right: 8px"></i>
						Create role assignment
					</Button>
				</template>
			</div>
		</template>
	</Dialog>
</template>

<script lang="ts">
import CreateRoleAssignment from '@/pages/security/role-assignments/create.vue';

const STEPS = {
	TABLE_STEP: 1,
	CREATE_STEP: 2,
};

export default {
	components: {
		CreateRoleAssignment,
	},

	props: {
		scopes: {
			type: Array,
			required: true,
			// default: [],
		},
	},

	data() {
		return {
			STEPS,
			dialogOpen: false,
			currentScopeIndex: 0,
			currentStep: STEPS.TABLE_STEP,
		};
	},

	computed: {
		currentScope() {
			return this.scopes[this.currentScopeIndex];
		},
	},

	methods: {
		handleClose() {
			this.dialogOpen = false;
			this.currentStep = STEPS.TABLE_STEP;
		},

		async handleCreateRoleAssignment() {
			try {
				await this.$refs.createForm.createRoleAssignment();
				this.currentStep = STEPS.TABLE_STEP;
			} catch (error) {
				this.$toast.add({
					severity: 'error',
					detail: error,
					life: 5000,
				});
			}
		},
	},
};
</script>
