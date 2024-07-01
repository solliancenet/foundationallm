<template>
	<div>
		<!-- Header -->
		<h2 class="page-header">{{ editId ? 'Edit Role Assignment' : 'Create Role Assignment' }}</h2>
		<div class="page-subheader">
			{{
				editId
					? 'Edit your role assignment settings below.'
					: 'Complete the settings below to configure the role assignment.'
			}}
		</div>

		<!-- Steps -->
		<div class="steps" :class="{ 'steps--loading': loading }">
			<!-- Loading overlay -->
			<template v-if="loading">
				<div class="steps__loading-overlay">
					<LoadingGrid />
					<div>{{ loadingStatusText }}</div>
				</div>
			</template>

			<!-- Description -->
			<div class="step-header span-2">What is the description of the role assignment?</div>
			<div class="span-2">
				<div class="mb-2">Data description:</div>
				<div class="input-wrapper">
					<InputText
						v-model="roleAssignment.description"
						placeholder="Enter a description for this role assignment"
						type="text"
						class="w-100"
					/>
				</div>
			</div>

			<!-- Principal -->
			<div class="step-header span-2">What principal to assign?</div>
			<div class="span-2">
				
				<div class="mb-2 mt-2">Principal ID:</div>
				<div style="display: flex; gap: 16px;">
					<InputText
						v-model="roleAssignment.principal_id"
						placeholder="Enter principal id (GUID)"
						type="text"
						class="w-50"
					/>
					<Button
						label="Browse"
						severity="primary"
						@click="openBrowsePrincipalsModal = true"
					/>
				</div>

				<!-- Browse principals dialog -->
				<Dialog
					:visible="openBrowsePrincipalsModal"
					modal
					header="Browse Principals"
					:closable="false"
					:style="{ minWidth: '30rem' }"
				>
					<div class="mb-2">Search type</div>
					<Dropdown
						v-model="principalSearchType"
						:options="['User', 'Group']"
						placeholder="--Select--"
						class="mb-2 w-100"
					/>

					<div class="mb-2">Search query</div>
					<InputText
						v-model="principalSearch"
						placeholder="Search"
						type="text"
					/>
					<template #footer>
						<Button label="Cancel" text @click="openBrowsePrincipalsModal = false" />
						<Button label="Select" severity="primary" @click="handlePrincipalSelected" />
					</template>
				</Dialog>
			</div>

			<!-- Role -->
			<div class="step-header span-2">What role to assign?</div>
			<div class="span-2">
				<Dropdown
					v-model="roleAssignment.role_definition_id"
					:options="roleOptions"
					option-label="display_name"
					option-value="object_id"
					placeholder="--Select--"
				/>
			</div>

			<!-- Buttons -->
			<div class="button-container column-2 justify-self-end">
				<!-- Create role assignment -->
				<Button
					:label="editId ? 'Save Changes' : 'Create Role Assignment'"
					severity="primary"
					@click="handleCreateRoleAssignment"
				/>

				<!-- Cancel -->
				<Button
					v-if="editId"
					style="margin-left: 16px"
					label="Cancel"
					severity="secondary"
					@click="handleCancel"
				/>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import type { PropType } from 'vue';
import { debounce } from 'lodash';
import api from '@/js/api';

import type {
	Role,
	RoleAssignment,
} from '@/js/types';

export default {
	name: 'CreateRoleAssignment',

	props: {
		editId: {
			type: [Boolean, String] as PropType<false | string>,
			required: false,
			default: false,
		},
	},

	data() {
		return {
			loading: false as boolean,
			loadingStatusText: 'Retrieving data...' as string,

			openBrowsePrincipalsModal: false,
			roleOptions: [] as Role[],
			principalSearchType: null as null | string,

			roleAssignment: {
				description: '',
				principal_id: null,
				role_definition_id: null,
				cost_center: null,
			} as null | RoleAssignment,
		};
	},

	async created() {
		this.loading = true;

		this.loadingStatusText = `Retrieving roles...`;
		this.roleOptions = await api.getRoleDefinitions();

		if (this.editId) {
			this.loadingStatusText = `Retrieving role assignment "${this.editId}"...`;
			this.roleAssignment = await api.getRoleAssignment(this.editId);
		}

		const users = await api.getUsers();
		const user = users.items[0];

		const groups = await api.getGroups();
		const group = groups.items[0];

		const objects = await api.getObjects({
			ids: [user.id, group.id],
		});

		this.loading = false;
	},

	methods: {
		handleCancel() {
			if (!confirm('Are you sure you want to cancel?')) {
				return;
			}

			this.$router.push('/security/role-assignments');
		},

		handlePrincipalSelected() {

		},

		async handleCreateRoleAssignment() {
			const errors: string[] = [];

			if (!this.roleAssignment.principal_id) {
				errors.push('Please specify a principal.');
			}

			if (!this.roleAssignment.role_definition_id) {
				errors.push('Please specify a role.');
			}
		
			if (errors.length > 0) {
				this.$toast.add({
					severity: 'error',
					detail: errors.join('\n'),
				});

				return;
			}

			this.loading = true;
			let successMessage = null as null | string;
			try {
				this.loadingStatusText = 'Saving role assignment...';
				await api.updateRoleAssignment(this.roleAssignment);
				successMessage = `Role assignment was successfully saved.`;
			} catch (error) {
				this.loading = false;
				return this.$toast.add({
					severity: 'error',
					detail: error?.response?._data || error,
				});
			}

			this.$toast.add({
				severity: 'success',
				detail: successMessage,
			});

			this.loading = false;

			if (!this.editId) {
				this.$router.push('/security/role-assignments');
			}
		},
	},
};
</script>

<style lang="scss">
.steps {
	display: grid;
	grid-template-columns: minmax(auto, 50%) minmax(auto, 50%);
	gap: 24px;
	position: relative;
}

.steps--loading {
	pointer-events: none;
}

.steps__loading-overlay {
	position: fixed;
	top: 0;
	left: 0;
	width: 100%;
	height: 100%;
	display: flex;
	flex-direction: column;
	justify-content: center;
	align-items: center;
	gap: 16px;
	z-index: 10;
	background-color: rgba(255, 255, 255, 0.9);
	pointer-events: none;
}

.step-section-header {
	background-color: rgba(150, 150, 150, 1);
	color: white;
	font-size: 1rem;
	font-weight: 600;
	padding: 16px;
}

.step-header {
	font-weight: bold;
	margin-bottom: -10px;
}

.step {
	// display: flex;
	// flex-direction: column;
}

.step--disabled {
	pointer-events: none;
	opacity: 0.5;
}

.step-container {
	// padding: 16px;
	border: 2px solid #e1e1e1;
	flex-grow: 1;
	position: relative;

	&:hover {
		background-color: rgba(217, 217, 217, 0.4);
	}

	&__header {
		font-weight: bold;
		margin-bottom: 8px;
	}
}

.step-container__view {
	// padding: 16px;
	height: 100%;
	display: flex;
	flex-direction: row;
}

.step-container__view__inner {
	padding: 16px;
	flex-grow: 1;
	word-break: break-word;
}

.step-container__view__arrow {
	background-color: #e1e1e1;
	color: rgb(150, 150, 150);
	width: 40px;
	min-width: 40px;
	display: flex;
	justify-content: center;
	align-items: center;

	&:hover {
		background-color: #cacaca;
	}
}

$editStepPadding: 16px;
.step-container__edit {
	border: 2px solid #e1e1e1;
	position: absolute;
	width: calc(100% + 4px);
	background-color: white;
	top: -2px;
	left: -2px;
	z-index: 5;
	box-shadow: 0 5px 20px 0 rgba(27, 29, 33, 0.2);
	min-height: calc(100% + 4px);
	// padding: $editStepPadding;
	display: flex;
	flex-direction: row;
}

.step-container__edit__inner {
	padding: $editStepPadding;
	flex-grow: 1;
}

.step-container__edit__arrow {
	background-color: #e1e1e1;
	color: rgb(150, 150, 150);
	min-width: 40px;
	width: 40px;
	display: flex;
	justify-content: center;
	align-items: center;
	transform: rotate(180deg);

	&:hover {
		background-color: #cacaca;
	}
}

.step-container__edit-dropdown {
	border: 2px solid #e1e1e1;
	position: absolute;
	width: calc(100% + 4px);
	background-color: white;
	top: -2px;
	left: -2px;
	z-index: 5;
	box-shadow: 0 5px 20px 0 rgba(27, 29, 33, 0.2);
	display: flex;
	flex-direction: column;
	min-height: calc(100% + 4px);
}

.step-container__edit__header {
	padding: $editStepPadding;
}

.step-container__edit__group-header {
	font-weight: bold;
	padding: $editStepPadding;
	padding-bottom: 0px;
}

.step-container__edit__option {
	padding: $editStepPadding;
	word-break: break-word;
	&:hover {
		background-color: rgba(217, 217, 217, 0.4);
	}
}

// .step-container__edit__option + .step-container__edit__option{
// 	border-top: 2px solid #e1e1e1;
// }

.step-container__edit__option--selected {
	// outline: 2px solid #e1e1e1;
	// background-color: rgba(217, 217, 217, 0.4);
}

.step__radio {
	display: flex;
	gap: 10px;
}

.step-option__header {
	text-decoration: underline;
	margin-right: 8px;
}

.primary-button {
	background-color: var(--primary-button-bg) !important;
	border-color: var(--primary-button-bg) !important;
	color: var(--primary-button-text) !important;
}

.input-wrapper {
	position: relative;
	display: flex;
	align-items: center;
}

input {
	width: 100%;
	padding-right: 30px;
}

.icon {
	position: absolute;
	right: 10px;
	cursor: default;
}

.valid {
	color: green;
}

.invalid {
	color: red;
}

.flex-container {
	display: flex;
	align-items: center; /* Align items vertically in the center */
}

.flex-item {
	flex-grow: 1; /* Allow the textarea to grow and fill the space */
}

.flex-item-button {
	margin-left: 8px; /* Add some space between the textarea and the button */
}

.p-chips {
	ul {
		width: 100%;
		li {
			input {
				width: 100%!important;
			}
		}
	}
}
</style>
