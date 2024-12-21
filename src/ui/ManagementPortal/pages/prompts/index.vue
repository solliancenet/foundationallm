<template>
	<main id="main-content">
		<h2 class="page-header">All Agent Prompts</h2>

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
				:value="prompts"
				striped-rows
				scrollable
				table-style="max-width: 100%"
				size="small"
			>
				<template #empty>
					<div role="alert" aria-live="polite">No prompts found.</div>
				</template>
				<template #loading>Loading agent prompts. Please wait.</template>

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
							:to="'/prompts/edit/' + data.resource.name"
							class="table__button"
							tabindex="-1"
						>
							<VTooltip :auto-hide="false" :popper-triggers="['hover']">
								<Button
									link
									:disabled="!data.actions.includes('FoundationaLLM.Prompt/prompts/write')"
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
			</DataTable>
		</div>
	</main>
</template>

<script lang="ts">
import api from '@/js/api';
import type { Prompt, ResourceProviderGetResult } from '@/js/types';

export default {
	name: 'Prompts',

	data() {
		return {
			prompts: [] as ResourceProviderGetResult<Prompt>[],
			loading: false as boolean,
			loadingStatusText: 'Retrieving data...' as string,
			accessControlModalOpen: false,
		};
	},

	async created() {
		await this.getPrompts();
	},

	methods: {
		async getPrompts() {
			this.loading = true;
			try {
				this.prompts = (await api.getPrompts()) || [];
			} catch (error) {
				this.$toast.add({
					severity: 'error',
					detail: error?.response?._data || error,
					life: 5000,
				});
			}
			this.loading = false;
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
