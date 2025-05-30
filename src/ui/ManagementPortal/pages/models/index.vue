<template>
	<div>
		<div style="display: flex">
			<div style="flex: 1">
				<h2 class="page-header">Models</h2>
				<div class="page-subheader">The following language models are available.</div>
			</div>

			<div style="display: flex; align-items: center">
				<NuxtLink to="/models/create">
					<Button>
						<i class="pi pi-plus" style="color: var(--text-primary); margin-right: 8px"></i>
						Create Model
					</Button>
				</NuxtLink>
			</div>
		</div>

		<div :class="{ 'grid--loading': loading }">
			<!-- Loading overlay -->
			<template v-if="loading">
				<div class="grid__loading-overlay">
					<LoadingGrid />
					<div>{{ loadingStatusText }}</div>
				</div>
			</template>

			<!-- Table -->
			<DataTable
				:globalFilterFields="['resource.name']"
				:filters="filters"
				filterDisplay="menu"
				paginator
				:rows="10"
				:rowsPerPageOptions="[10, 25, 50, 100]"
				:paginatorTemplate="'FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown'"
				:value="aiModels"
				striped-rows
				scrollable
				:sort-field="'resource.name'"
				:sort-order="1"
				table-style="max-width: 100%"
				size="small"
			>
				<template #header>
					<div class="w-full flex justify-between">
						<TableSearch v-model="filters" placeholder="Search models" />
						<Button
							type="button"
							icon="pi pi-refresh"
							@click="getModels"
						/>
					</div>
				</template>

				<template #empty>
					No models/endpoints found. Please use the menu on the left to create a new model/endpoint.
				</template>

				<template #loading>Loading model & endpoints. Please wait.</template>

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
							:to="'/models/edit/' + data.resource.name"
							:aria-disabled="!data.actions.includes('FoundationaLLM.AIModel/aiModels/write')"
							:style="{
								pointerEvents: !data.actions.includes('FoundationaLLM.AIModel/aiModels/write')
									? 'none'
									: 'auto',
							}"
							class="table__button"
						>
							<Button
								link
								:disabled="!data.actions.includes('FoundationaLLM.AIModel/aiModels/write')"
								:aria-label="`Edit ${data.resource.name}`"
							>
								<i class="pi pi-cog" style="font-size: 1.2rem" aria-hidden="true"></i>
							</Button>
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
						<Button
							link
							:aria-label="`Delete ${data.resource.name}`"
							:disabled="!data.actions.includes('FoundationaLLM.AIModel/aiModels/delete')"
							@click="itemToDelete = data.resource"
						>
							<i class="pi pi-trash" style="font-size: 1.2rem" aria-hidden="true"></i>
						</Button>
					</template>
				</Column>
			</DataTable>
		</div>

		<!-- Delete model/endpoint dialog -->
		<Dialog :visible="itemToDelete !== null" modal header="Delete Model" :closable="false">
			<p>Do you want to delete the model "{{ itemToDelete.name }}" ?</p>
			<template #footer>
				<Button label="Cancel" text @click="itemToDelete = null" />
				<Button label="Delete" severity="danger" @click="handleDelete" />
			</template>
		</Dialog>
	</div>
</template>

<script lang="ts">
import api from '@/js/api';
import type { AIModel } from '@/js/types';

export default {
	name: 'Models',

	data() {
		return {
			aiModels: [] as AIModel,
			loading: false as boolean,
			loadingStatusText: 'Retrieving data...' as string,
			filters: {
				global: { value: null, matchMode: 'contains' }
			},
			itemToDelete: null as AIModel | null,
		};
	},

	async created() {
		await this.getModels();
	},

	beforeUnmount() {
		// Clear filters when leaving the component
		this.filters.global.value = null;
	},

	methods: {
		async getModels() {
			this.loading = true;
			try {
				this.aiModels = await api.getAIModels();
			} catch (error) {
				this.$toast.add({
					severity: 'error',
					detail: error?.response?._data || error,
					life: 5000,
				});
			}
			this.loading = false;
		},

		async handleDelete() {
			try {
				await api.deleteAIModel(this.itemToDelete!.name);
				this.$toast.add({
					severity: 'success',
					detail: `Successfully deleted AI model "${this.itemToDelete!.name}"!`,
					life: 5000,
				});
				this.itemToDelete = null;
			} catch (error) {
				return this.$toast.add({
					severity: 'error',
					detail: error?.response?._data || error,
					life: 5000,
				});
			}

			await this.getModels();
		},
	},
};
</script>

<style lang="scss" scoped>
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
