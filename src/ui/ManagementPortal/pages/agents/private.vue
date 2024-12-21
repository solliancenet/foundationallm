<template>
	<main id="main-content">
		<h2 class="page-header">My Agents</h2>
		<div class="page-subheader">View your agents.</div>

		<div :class="{ 'grid--loading': loading }">
			<!-- Loading overlay -->
			<template v-if="loading">
				<div class="grid__loading-overlay" role="status" aria-live="polite">
					<LoadingGrid />
					<div>{{ loadingStatusText }}</div>
				</div>
			</template>

			<!-- Table -->
			<DataTable :value="agents" striped-rows scrollable table-style="max-width: 100%" size="small">
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
					style="min-width: 200px"
					:pt="{
						headerCell: {
							style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
						},
						sortIcon: { style: { color: 'var(--primary-text)' } },
					}"
				></Column>

				<!-- Description -->
				<Column
					field="resource.description"
					header="Description"
					sortable
					style="min-width: 200px"
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
					style="min-width: 200px"
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
						<NuxtLink
							:to="'/agents/edit/' + data.resource.name"
							class="table__button"
							tabindex="-1"
						>
							<VTooltip :auto-hide="false" :popper-triggers="['hover']">
								<Button
									link
									:disabled="!data.actions.includes('FoundationaLLM.Agent/agents/write')"
									:aria-label="`Edit ${data.resource.name}`"
								>
									<i class="pi pi-cog" style="font-size: 1.2rem" aria-hidden="true"></i>
								</Button>
								<template #popper
									><div role="tooltip">Edit {{ data.resource.name }}</div></template
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
								><div role="tooltip">Delete {{ data.resource.name }}</div></template
							>
						</VTooltip>
					</template>
				</Column>
			</DataTable>
		</div>

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
	</main>
</template>

<script lang="ts">
import api from '@/js/api';
import type { Agent, ResourceProviderGetResult } from '@/js/types';

export default {
	name: 'PrivateAgents',

	data() {
		return {
			agents: [] as ResourceProviderGetResult<Agent>[],
			loading: false as boolean,
			loadingStatusText: 'Retrieving data...' as string,
			agentToDelete: null as Agent | null,
		};
	},

	async created() {
		await this.getAgents();
	},

	methods: {
		async getAgents() {
			this.loading = true;
			try {
				const agents = await api.getAgents();
				// Only retrieve agents where the user is an owner.
				this.agents = agents.filter((agent) => agent.roles.includes('Owner'));
			} catch (error) {
				this.$toast.add({
					severity: 'error',
					detail: error?.response?._data || error,
					life: 5000,
				});
			}
			this.loading = false;
		},

		async handleDeleteAgent() {
			try {
				await api.deleteAgent(this.agentToDelete!.name);
				this.agentToDelete = null;
			} catch (error) {
				return this.$toast.add({
					severity: 'error',
					detail: error?.response?._data || error,
					life: 5000,
				});
			}

			await this.getAgents();
		},
	},
};
</script>

<style lang="scss"></style>
