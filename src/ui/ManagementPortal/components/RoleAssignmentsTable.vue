<template>
	<div :class="{ 'grid--loading': loading }">
		<!-- Loading overlay -->
		<template v-if="loading">
			<div class="grid__loading-overlay" role="status" aria-live="polite">
				<LoadingGrid />
				<div>{{ loadingStatusText }}</div>
			</div>
		</template>

		<!-- Table -->
		<DataTable
			v-model:expandedRowGroups="expandedRowGroups"
			:value="roleAssignments"
			expandable-row-groups
			row-group-mode="subheader"
			group-rows-by="role.display_name"
			sort-field="role.display_name"
			striped-rows
			scrollable
			table-style="max-width: 100%"
			size="small"
		>
			<template #empty> No role assignments found. </template>

			<template #loading>Loading data sources. Please wait.</template>

			<template #groupheader="{ data }">
				<span>{{ data.role.display_name }}</span>
				<span
					v-tooltip.bottom="{
						value: data.role.description,
						autoHide: false,
					}"
				>
					<i class="pi pi-info-circle" style="margin-left: 8px"></i>
				</span>
			</template>

			<!-- Name -->
			<Column
				field="principal.display_name"
				header="Name"
				sortable
				style="min-width: 120px"
				:pt="{
					headerCell: {
						style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
					},
					sortIcon: { style: { color: 'var(--primary-text)' } },
				}"
			>
				<template #body="{ data }">
					<div class="d-flex align-center" style="gap: 12px; margin-left: 32px">
						<i v-if="data.principal_type === 'Group'" class="pi pi-users"></i>
						<i v-else-if="data.principal_type === 'User'" class="pi pi-user"></i>
						<i v-else class="pi pi-verified"></i>

						<span>
							<div>{{ data.principal?.display_name }}</div>
							<div v-if="data.principal?.email">({{ data.principal?.email }})</div>
						</span>
					</div>
				</template>
			</Column>

			<!-- Principal Type -->
			<Column
				field="principal_type"
				header="Type"
				sortable
				style="min-width: 120px"
				:pt="{
					headerCell: {
						style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
					},
					sortIcon: { style: { color: 'var(--primary-text)' } },
				}"
			>
			</Column>

			<!-- Role -->
			<!-- 	<Column
				field="role.display_name"
				header="Role"
				sortable
				style="min-width: 240px;"
				:pt="{
					headerCell: {
						style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
					},
					sortIcon: { style: { color: 'var(--primary-text)' } },
				}"
			>
				<template #body="{ data }">
					<span>{{ data.role.display_name }}</span>
					<span
						v-tooltip.bottom="{
							value: data.role.description,
							autoHide: false,
						}"
					>
						<i class="pi pi-info-circle" style="margin-left: 8px;"></i>
					</span>
				</template>
			</Column>
-->
			<!-- Scope -->
			<Column
				field="scope_name"
				header="Scope"
				sortable
				style="min-width: 140px"
				:pt="{
					headerCell: {
						style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
					},
					sortIcon: { style: { color: 'var(--primary-text)' } },
				}"
			>
				<template #body="{ data }">
					<span>{{ data.scope_name }}</span>
				</template>
			</Column>

			<!-- Delete -->
			<Column
				header="Delete"
				header-style="width:6rem"
				style="text-align: center"
				:pt="{
					headerCell: {
						style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
					},
					headerContent: { style: { justifyContent: 'center' } },
				}"
			>
				<template #body="{ data }">
					<Button
						link
						:aria-label="`Delete role assignment ${data.principal?.display_name ? 'for ' + data.principal?.display_name : ''}`"
						@click="roleAssignmentToDelete = data"
					>
						<i class="pi pi-trash" style="font-size: 1.2rem" aria-hidden="true"></i>
					</Button>
				</template>
			</Column>
		</DataTable>

		<!-- Delete role assignment dialog -->
		<Dialog
			:visible="roleAssignmentToDelete !== null"
			modal
			header="Delete Role Assignment"
			:closable="false"
		>
			<p>
				Do you want to delete the role assignment for "{{
					roleAssignmentToDelete.principal.display_name
				}}"?
			</p>
			<template #footer>
				<Button label="Cancel" text @click="roleAssignmentToDelete = null" />
				<Button label="Delete" severity="danger" @click="handleDeleteRoleAssignment" />
			</template>
		</Dialog>
	</div>
</template>

<script lang="ts">
import api from '@/js/api';
import type { RoleAssignment } from '@/js/types';

export default {
	name: 'RoleAssignmentsTable',

	props: {
		scope: {
			required: false,
			type: String,
			default: null,
		},
	},

	data() {
		return {
			expandedRowGroups: [],
			roleAssignments: [] as RoleAssignment[],
			loading: false as boolean,
			loadingStatusText: 'Retrieving role assignments...' as string,
			roleAssignmentToDelete: null as RoleAssignment | null,
		};
	},

	async created() {
		await this.getRoleAssignments();
	},

	methods: {
		async getRoleAssignments() {
			this.loading = true;
			try {
				const roleAssignments = await api.getRoleAssignments(this.scope);

				const principalIds = [];
				for (const assignmentForPrincipalId of roleAssignments) {
					principalIds.push(assignmentForPrincipalId.resource.principal_id);
				}

				const principals = await api.getObjects({
					ids: principalIds,
				});

				const roleDefinitions = await api.getRoleDefinitions();

				// Expand all role groups in table
				for (const roleDefinition of roleDefinitions) {
					this.expandedRowGroups.push(roleDefinition.display_name);
				}

				this.roleAssignments = roleAssignments.map((assignment) => {
					return {
						name: assignment.resource.name,
						principal_type: assignment.resource.principal_type,
						principal: principals.find(
							(principal) => principal.id === assignment.resource.principal_id,
						),
						scope: assignment.resource.scope,
						scope_name: assignment.resource.scope_name,
						role: roleDefinitions.find(
							(role) => role.object_id === assignment.resource.role_definition_id,
						),
					};
				});
			} catch (error) {
				this.$toast.add({
					severity: 'error',
					detail: error?.response?._data || error,
					life: 5000,
				});
			}
			this.loading = false;
		},

		async handleDeleteRoleAssignment() {
			try {
				await api.deleteRoleAssignment(this.roleAssignmentToDelete!.name);
				this.roleAssignmentToDelete = null;
			} catch (error) {
				return this.$toast.add({
					severity: 'error',
					detail: error?.response?._data || error,
					life: 5000,
				});
			}

			await this.getRoleAssignments();
		},
	},
};
</script>

<style lang="scss">
.table__button {
	color: var(--primary-button-bg);
}

.steps {
	display: grid;
	grid-template-columns: minmax(auto, 50%) minmax(auto, 50%);
	gap: 24px;
	position: relative;
}

.grid--loading {
	pointer-events: none;
}

.grid__loading-overlay {
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

.description__column {
	max-width: 100%;
	display: inline-block;
	overflow: hidden;
	white-space: nowrap;
	text-overflow: ellipsis;
}
</style>
