<template>
	<main id="main-content">
		<!-- Header -->
		<template v-if="!headless">
			<h2 class="page-header">{{ editId ? 'Edit Role Assignment' : 'Create Role Assignment' }}</h2>
			<div class="page-subheader">
				{{
					editId
						? 'Edit your role assignment settings below.'
						: 'Complete the settings below to configure the role assignment.'
				}}
			</div>
		</template>

		<!-- Steps -->
		<div class="steps" :class="{ 'steps--loading': loading }">
			<!-- Loading overlay -->
			<template v-if="loading">
				<div class="steps__loading-overlay" role="status" aria-live="polite">
					<LoadingGrid />
					<div>{{ loadingStatusText }}</div>
				</div>
			</template>

			<!-- Scope -->
			<div class="step-header span-2">What is the assignment scope?</div>
			<div class="span-2">
				<div id="aria-scope" class="mb-2">Scope</div>
				<div class="input-wrapper">
					<InputText
						:value="scope"
						readonly
						placeholder="Instance"
						type="text"
						class="w-100"
						aria-labelledby="aria-scope"
					/>
				</div>
			</div>

			<!-- Description -->
			<div class="step-header span-2">What is the description of the role assignment?</div>
			<div class="span-2">
				<div id="aria-description" class="mb-2">Data description:</div>
				<div class="input-wrapper">
					<InputText
						v-model="roleAssignment.description"
						placeholder="Enter a description for this role assignment"
						type="text"
						class="w-100"
						aria-labelledby="aria-description"
					/>
				</div>
			</div>

			<!-- Principal -->
			<div class="step-header span-2">What principal to assign?</div>
			<div class="span-2">
				<!-- Type -->
				<div id="aria-principal-type" class="mb-2">Principal Type:</div>
				<div class="d-flex gap-4">
					<!-- <InputText
						v-model="principal.object_type"
						readonly
						placeholder="Browse for selection"
						type="text"
						class="w-50"
						aria-labelledby="aria-principal-type"
					/> -->
					<Dropdown
						v-model="principal.object_type"
						:options="principalTypeOptions"
						placeholder="--Select--"
						class="mb-2 w-100"
						aria-labelledby="aria-principal-type"
					/>
				</div>

				<!-- Name -->
				<div id="aria-principal-name" class="mb-2 mt-2">Principal Name:</div>
				<div class="d-flex gap-4">
					<InputText
						v-model="principal.display_name"
						readonly
						placeholder="Browse for selection"
						type="text"
						class="w-50"
						aria-labelledby="aria-principal-name"
					/>
				</div>

				<!-- Email -->
				<div id="aria-principal-email" class="mb-2 mt-2">Principal Email:</div>
				<div class="d-flex gap-4">
					<InputText
						v-model="principal.email"
						readonly
						:placeholder="!principal.email ? 'None specified' : 'Browse for selection'"
						type="text"
						class="w-50"
						aria-labelledby="aria-principal-email"
					/>
				</div>

				<!-- ID -->
				<div id="aria-principal-id" class="mb-2 mt-2">Principal ID:</div>
				<div class="d-flex gap-4">
					<InputText
						v-model="roleAssignment.principal_id"
						placeholder="Browse for selection"
						type="text"
						class="w-50"
						aria-labelledby="aria-principal-id"
					/>
					<Button
						label="Browse"
						aria-label="Browse for principals"
						severity="primary"
						@click="selectPrincipalDialogOpen = true"
					/>
				</div>

				<!-- Browse principals dialog -->
				<Dialog
					:visible="selectPrincipalDialogOpen"
					modal
					header="Browse Principals"
					:closable="false"
					:style="{ minWidth: '30rem' }"
				>
					<div id="aria-principal-search-type" class="mb-2">Search type</div>
					<Dropdown
						v-model="principalSearchType"
						id="principal-search-type"
						:options="principalTypeOptions"
						placeholder="--Select--"
						class="mb-2 w-100"
						aria-labelledby="aria-principal-search-type"
					/>

					<div id="aria-principal-search-query" class="mb-2">Search query</div>
					<AutoComplete
						v-model="dialogPrincipal"
						:suggestions="principalOptions.length === 0 ? null : principalOptions"
						:loading="loadingPrincipals"
						force-selection
						dropdown
						data-key="id"
						option-label="display_name"
						class="w-100"
						aria-labelledby="aria-principal-search-query"
						@show="handlePrincipalDropdownShow"
						@hide="handlePrincipalDropdownHide"
						@complete="handlePrincipalSearch"
					>
						<template #option="{ option }">
							<div class="flex items-center">
								<div>{{ option.display_name }}</div>
								<div style="font-size: 0.8rem">{{ option.email }}</div>
							</div>
						</template>
					</AutoComplete>

					<template #footer>
						<Button label="Cancel" text @click="selectPrincipalDialogOpen = false" />
						<Button label="Select" severity="primary" @click="handlePrincipalSelected" />
					</template>
				</Dialog>
			</div>

			<!-- Role -->
			<div id="aria-role" class="step-header span-2">What role to assign?</div>
			<div class="span-2">
				<Dropdown
					v-model="roleAssignment.role_definition_id"
					:options="roleOptions"
					option-label="display_name"
					option-value="object_id"
					placeholder="--Select--"
					aria-labelledby="aria-role"
				/>
			</div>

			<!-- Buttons -->
			<div v-if="!headless" class="button-container column-2 justify-self-end">
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
	</main>
</template>

<script lang="ts">
import { v4 as uuidv4 } from 'uuid';
import type { PropType } from 'vue';
import api from '@/js/api';

import type { Role, RoleAssignment } from '@/js/types';

export default {
	name: 'CreateRoleAssignment',

	props: {
		editId: {
			type: [Boolean, String] as PropType<false | string>,
			required: false,
			default: false,
		},

		headless: {
			type: Boolean,
			required: false,
			default: false,
		},

		scope: {
			type: String,
			required: false,
			default: null,
		},
	},

	expose: ['createRoleAssignment'],

	data() {
		return {
			loading: false as boolean,
			loadingStatusText: 'Retrieving data...' as string,

			selectPrincipalDialogOpen: false,
			roleOptions: [] as Role[],
			principalSearchType: 'User' as null | string,
			principalTypeOptions: ['User', 'Group'],

			// scope: this.$route.query.scope ?? null,
			roleAssignment: {
				name: '',
				description: '',
				principal_id: null,
				role_definition_id: null,
				type: 'FoundationaLLM.Authorization/roleAssignments',
				// cost_center: null,
			} as null | RoleAssignment,

			principal: {
				object_type: null,
				display_name: '',
			},
			dialogPrincipal: null,

			loadingPrincipals: false,
			principalOptions: [],
			principalSearchQuery: '',
			principalsCurrentPage: 1,
			hasNextPrincipalPage: false,
		};
	},

	watch: {
		principalSearchType() {
			this.dialogPrincipal = null;
		},
		selectPrincipalDialogOpen(value) {
			if (value) {
				this.$nextTick(() => {
					const input = document.querySelector('#principal-search-type span');
					input?.focus();
				});
			}
		},
	},

	async created() {
		this.loading = true;

		this.loadingStatusText = `Retrieving roles...`;
		this.roleOptions = await api.getRoleDefinitions();

		if (this.editId) {
			this.loadingStatusText = `Retrieving role assignment "${this.editId}"...`;
			this.roleAssignment = await api.getRoleAssignment(this.editId);
		}

		this.loading = false;
	},

	methods: {
		handlePrincipalDropdownShow() {
			const dropdownPanel = document.querySelector('.p-autocomplete-panel');
			if (dropdownPanel) {
				dropdownPanel.addEventListener('scroll', this.handlePrincipalScroll);
			}
		},

		handlePrincipalDropdownHide() {
			this.principalOptions = [];
		},

		async handlePrincipalSearch(event) {
			this.principalSearchQuery = event.query;
			this.principalsCurrentPage = 1;
			this.principalOptions = [];
			await this.loadMorePrincipals();
		},

		async handlePrincipalScroll(event) {
			const dropdown = event.target;
			const buffer = 10;
			if (dropdown.scrollHeight - dropdown.scrollTop - buffer <= dropdown.clientHeight) {
				if (this.loadingPrincipals || !this.hasNextPrincipalPage) return;

				this.principalsCurrentPage += 1;
				await this.loadMorePrincipals();
			}
		},

		async loadMorePrincipals() {
			const apiMethod = this.principalSearchType === 'Group' ? api.getGroups : api.getUsers;

			this.loadingPrincipals = true;

			const principalsCurrentPage = await apiMethod.call(api, {
				page_number: this.principalsCurrentPage,
				name: this.principalSearchQuery,
			});

			this.principalOptions.push(...principalsCurrentPage.items);
			this.hasNextPrincipalPage = principalsCurrentPage.has_next_page;

			this.loadingPrincipals = false;
		},

		handleCancel() {
			if (!confirm('Are you sure you want to cancel?')) {
				return;
			}

			this.$router.push('/security/role-assignments');
		},

		handlePrincipalSelected() {
			if (!this.dialogPrincipal) {
				return this.$toast.add({
					severity: 'error',
					detail: 'Please select a principal to assign.',
					life: 5000,
				});
			}

			this.principal = this.dialogPrincipal;
			this.roleAssignment.principal_id = this.dialogPrincipal.id;
			this.roleAssignment.principal_type = this.dialogPrincipal.object_type;
			this.dialogPrincipal = null;
			this.selectPrincipalDialogOpen = false;
		},

		async handleCreateRoleAssignment() {
			try {
				await this.createRoleAssignment();
			} catch (error) {
				this.$toast.add({
					severity: 'error',
					detail: error,
					life: 5000,
				});
			}
		},

		async createRoleAssignment() {
			const errors: string[] = [];

			if (!this.roleAssignment.principal_id) {
				errors.push('Please specify a principal.');
			}

			if (!this.roleAssignment.role_definition_id) {
				errors.push('Please specify a role.');
			}

			if (!this.roleAssignment.principal_type) {
				if (this.principal.object_type) {
					this.roleAssignment.principal_type = this.principal.object_type;
				} else {
					errors.push('Please specify a principal type.');
				}
			}

			if (errors.length > 0) {
				throw errors.join('\n');
			}

			this.loading = true;
			let successMessage = null as null | string;
			try {
				this.loadingStatusText = 'Saving role assignment...';

				await api.createRoleAssignment({
					...this.roleAssignment,
					name: uuidv4(),
					...(this.scope ? { scope: `/instances/${api.instanceId}/${this.scope}` } : {}),
				});
				successMessage = `Role assignment was successfully saved.`;
			} catch (error) {
				this.loading = false;
				throw error?.response?._data || error;
			}

			this.$toast.add({
				severity: 'success',
				detail: successMessage,
				life: 5000,
			});

			this.loading = false;

			if (!this.editId && !this.headless) {
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
				width: 100% !important;
			}
		}
	}
}
</style>
