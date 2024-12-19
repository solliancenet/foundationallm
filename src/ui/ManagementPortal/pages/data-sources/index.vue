<template>
	<main id="main-content">
		<div style="display: flex">
			<div style="flex: 1">
				<h2 class="page-header">Data Sources</h2>
				<div class="page-subheader">The following data sources are available.</div>
			</div>

			<div style="display: flex; align-items: center">
				<NuxtLink to="/data-sources/create" tabindex="-1">
					<Button aria-label="Create data source">
						<i class="pi pi-plus" style="color: var(--text-primary); margin-right: 8px"></i>
						Create Data Source
					</Button>
				</NuxtLink>
			</div>
		</div>

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
				:value="dataSources"
				striped-rows
				scrollable
				table-style="max-width: 100%"
				size="small"
			>
				<template #empty>
					<div role="alert" aria-live="polite">No data sources found.</div>
				</template>

				<template #loading>Loading data sources. Please wait.</template>

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

				<!-- Type -->
				<Column
					field="resource.type"
					header="Source Type"
					sortable
					style="min-width: 200px"
					:pt="{
						headerCell: {
							style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
						},
						sortIcon: { style: { color: 'var(--primary-text)' } },
					}"
				></Column>

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
							:to="'/data-sources/edit/' + data.resource.name"
							class="table__button"
							tabindex="-1"
						>
							<VTooltip :auto-hide="false" :popper-triggers="['hover']">
								<Button link :aria-label="`Edit ${data.resource.name}`">
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
								:aria-label="`Delete ${data.resource.name}`"
								@click="dataSourceToDelete = data.resource"
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
			:visible="dataSourceToDelete !== null"
			modal
			v-focustrap
			header="Delete Data Source"
			:closable="false"
		>
			<p>Do you want to delete the data source "{{ dataSourceToDelete.name }}" ?</p>
			<template #footer>
				<Button label="Cancel" text @click="dataSourceToDelete = null" />
				<Button label="Delete" severity="danger" autofocus @click="handleDeleteDataSource" />
			</template>
		</Dialog>
	</main>
</template>

<script lang="ts">
import api from '@/js/api';
import type { DataSource, ResourceProviderGetResult } from '@/js/types';

export default {
	name: 'PublicDataSources',

	data() {
		return {
			dataSources: [] as ResourceProviderGetResult<DataSource>[],
			loading: false as boolean,
			loadingStatusText: 'Retrieving data...' as string,
			dataSourceToDelete: null as DataSource | null,
		};
	},

	async created() {
		await this.getAgentDataSources();
	},

	methods: {
		async getAgentDataSources() {
			this.loading = true;
			try {
				this.dataSources = await api.getAgentDataSources();
			} catch (error) {
				this.$toast.add({
					severity: 'error',
					detail: error?.response?._data || error,
					life: 5000,
				});
			}
			this.loading = false;
		},

		async handleDeleteDataSource() {
			try {
				await api.deleteDataSource(this.dataSourceToDelete!.name);
				this.dataSourceToDelete = null;
			} catch (error) {
				return this.$toast.add({
					severity: 'error',
					detail: error?.response?._data || error,
					life: 5000,
				});
			}

			await this.getAgentDataSources();
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
</style>
