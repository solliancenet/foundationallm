<template>
	<div :class="{ 'grid--loading': loading }" style="overflow: auto">
		<!-- Loading overlay -->
		<template v-if="loading">
			<div class="grid__loading-overlay" role="status" aria-live="polite">
				<LoadingGrid />
				<div>{{ loadingStatusText }}</div>
			</div>
		</template>

		<!-- Table -->
		<DataTable
			:value="agents"
			striped-rows
			scrollable
			sortField="resource.display_name"
			:sortOrder="1"
			table-style="max-width: 100%"
			size="small"
		>
			<template #empty>
				<div role="alert" aria-live="polite">
					No agents found. Please use the menu on the left to create a new agent.
				</div>
			</template>
			<template #loading>Loading agent data. Please wait.</template>

			<!-- Name -->
			<Column
				field="resource.name"
				header="Name"
				sortable
				:style="columnStyle"
				:pt="{
					headerCell: {
						style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
					},
					sortIcon: { style: { color: 'var(--primary-text)' } },
				}"
			>
				<template #body="{ data }">
					<span>{{ data.resource.name }} {{ data.resource.display_name ? `(${data.resource.display_name})` : '' }}</span>
					<template v-if="data.resource.properties?.default_resource === 'true'">
						<Chip label="Default" icon="pi pi-star" style="margin-left: 8px" />
					</template>
				</template>
			</Column>

			<!-- Description -->
			<Column
				field="resource.description"
				header="Description"
				sortable
				:style="columnStyle"
				:pt="{
					headerCell: {
						style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
					},
					sortIcon: { style: { color: 'var(--primary-text)' } },
				}"
			></Column>

			<!-- Expiration -->
			<Column
				field="resource.expiration_date"
				header="Expiration Date"
				sortable
				:style="columnStyle"
				:pt="{
					headerCell: {
						style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
					},
					sortIcon: { style: { color: 'var(--primary-text)' } },
				}"
			>
				<template #body="{ data }">
					<span>{{ $filters.formatDate(data.resource.expiration_date) }}</span>
				</template>
			</Column>

			<!-- Edit -->
			<Column
				header="Edit"
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
					<NuxtLink :to="'/agents/edit/' + data.resource.name" class="table__button" tabindex="-1">
						<VTooltip :auto-hide="false" :popper-triggers="['hover']">
							<Button
								link
								:disabled="!data.actions.includes('FoundationaLLM.Agent/agents/write')"
								:aria-label="`Edit ${data.resource.name}`"
							>
								<i class="pi pi-cog" style="font-size: 1.2rem" aria-hidden="true"></i>
							</Button>
							<template #popper
								><div role="tooltip">Edit {{ data.resource.display_name || data.resource.name }}</div></template
							>
						</VTooltip>
					</NuxtLink>
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
					<VTooltip :auto-hide="false" :popper-triggers="['hover']">
						<Button
							link
							:disabled="!data.actions.includes('FoundationaLLM.Agent/agents/delete')"
							:aria-label="`Delete ${data.resource.name}`"
							@click="agentToDelete = data.resource"
						>
							<i class="pi pi-trash" style="font-size: 1.2rem" aria-hidden="true"></i>
						</Button>
						<template #popper
							><div role="tooltip">Delete {{ data.resource.display_name || data.resource.name }}</div></template
						>
					</VTooltip>
				</template>
			</Column>

			<!-- Set Default -->
			<Column
				header="Set Default"
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
					<VTooltip :auto-hide="false" :popper-triggers="['hover']">
						<Button
							link
							:disabled="!data.roles.includes('User Access Administrator')"
							:aria-label="`Set ${data.resource.name} as default`"
							@click="agentToSetAsDefault = data.resource"
						>
							<i class="pi pi-star" style="font-size: 1.2rem" aria-hidden="true"></i>
						</Button>
						<template #popper
							><div role="tooltip">Set {{ data.resource.display_name || data.resource.name }} as default agent</div></template
						>
					</VTooltip>
				</template>
			</Column>
		</DataTable>

		<!-- Delete agent dialog -->
		<Dialog
			:visible="agentToDelete !== null"
			modal
			v-focustrap
			header="Delete Agent"
			:closable="false"
		>
			<p>Do you want to delete the agent "{{ agentToDelete.name }}" ?</p>
			<template #footer>
				<Button label="Cancel" text @click="agentToDelete = null" />
				<Button label="Delete" severity="danger" autofocus @click="handleDeleteAgent" />
			</template>
		</Dialog>

		<!-- Set default agent dialog -->
		<Dialog
			:visible="agentToSetAsDefault !== null"
			modal
			v-focustrap
			header="Set Default Agent"
			:closable="false"
		>
			<p>
				Do you want to set the "{{ agentToSetAsDefault.name }}" agent as default?<br />Default
				agents are automatically selected in the User Portal for new conversations.
			</p>
			<template #footer>
				<Button label="Cancel" text @click="agentToSetAsDefault = null" />
				<Button label="Set as Default" severity="info" autofocus @click="handleSetDefaultAgent" />
			</template>
		</Dialog>
	</div>
</template>

<script lang="ts">
import api from '@/js/api';
import type { Agent, ResourceProviderGetResult, ResourceProviderActionResult } from '@/js/types';

export default {
	name: 'AgentsList',

	props: {
		agents: {
			type: Array as () => ResourceProviderGetResult<Agent>[],
			required: true,
		},
		loading: {
			type: Boolean,
			required: true,
		},
		loadingStatusText: {
			type: String,
			required: true,
		},
	},

	computed: {
		columnStyle() {
			return window.innerWidth <= 768 ? {} : { minWidth: '200px' };
		},
	},

	data() {
		return {
			agentToDelete: null as Agent | null,
			agentToSetAsDefault: null as Agent | null,
		};
	},

	methods: {
		async handleDeleteAgent() {
			try {
				await api.deleteAgent(this.agentToDelete!.name);
				this.agentToDelete = null;
				this.$emit('refreshAgents');
			} catch (error) {
				this.$toast.add({
					severity: 'error',
					detail: error?.response?._data || error,
					life: 5000,
				});
			}
		},

		async handleSetDefaultAgent() {
			try {
				let result: ResourceProviderActionResult = await api.setDefaultAgent(
					this.agentToSetAsDefault!.name,
				);
				if (result.isSuccessResult) {
					this.$toast.add({
						severity: 'success',
						detail: `Agent "${this.agentToSetAsDefault!.name}" set as default.`,
						life: 5000,
					});
				} else {
					this.$toast.add({
						severity: 'error',
						detail: 'Could not set the default agent. Please try again.',
						life: 5000,
					});
				}
				this.agentToSetAsDefault = null;
				this.$emit('refreshAgents');
			} catch (error) {
				this.$toast.add({
					severity: 'error',
					detail: error?.response?._data || error,
					life: 5000,
				});
			}
		},
	},
};
</script>

<style lang="scss">
.table__button {
	color: var(--primary-button-bg);
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

@media (max-width: 768px) {
	.p-column {
		min-width: auto !important;
		white-space: normal;
	}

	.p-datatable {
		font-size: 0.9rem;
	}

	.p-column-header-content {
		text-align: left;
	}
}
</style>
