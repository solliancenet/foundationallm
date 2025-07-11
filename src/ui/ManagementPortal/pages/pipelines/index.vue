<template>
	<main id="main-content">
		<div style="display: flex">
			<div style="flex: 1">
				<h2 class="page-header">Data Pipelines</h2>
				<div class="page-subheader">The following pipelines are available.</div>
			</div>

			<div style="display: flex; align-items: center">
				<NuxtLink to="/pipelines/create" tabindex="-1">
					<Button aria-label="Create Pipeline">
						<i class="pi pi-plus" style="color: var(--text-primary); margin-right: 8px"></i>
						Create Pipeline
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
				:globalFilterFields="['resource.name', 'resource.description']"
				v-model:filters="filters"
				filterDisplay="menu"
				paginator
				:rows="10"
				:rowsPerPageOptions="[10, 25, 50, 100]"
				:paginatorTemplate="'FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown'"
				:value="pipelines"
				striped-rows
				scrollable
				:sort-field="'resource.name'"
				:sort-order="1"
				table-style="max-width: 100%"
				size="small"
			>
				<template #header>
					<div class="w-full flex justify-between">
						<TableSearch v-model="filters" placeholder="Search pipelines" />
						<Button
							type="button"
							icon="pi pi-refresh"
							@click="$emit('refresh-pipelines')"
						/>
					</div>
				</template>

				<template #empty>
					<div role="alert" aria-live="polite">No vector stores found.</div>
				</template>

				<template #loading>Loading pipelines. Please wait.</template>

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

				<!-- Trigger -->
				<!-- <Column
					field="resource.trigger_type"
					header="Trigger"
					sortable
					:pt="{
						headerCell: {
							style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
						},
						sortIcon: { style: { color: 'var(--primary-text)' } },
					}"
				></Column> -->

				<!-- Active -->
				<Column
					field="resource.active"
					header="Active"
					sortable
					:pt="{
						headerCell: {
							style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
						},
						sortIcon: { style: { color: 'var(--primary-text)' } },
					}"
				></Column>

				<!-- View -->
				<!-- <Column
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
						<NuxtLink :to="`/pipelines/edit/${data.resource.name}`" tabindex="-1">
							<Button link label="View" :aria-label="`View ${data.resource.name}`" />
						</NuxtLink>
					</template>
				</Column> -->

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
							:to="'/pipelines/edit/' + data.resource.name"
							:aria-disabled="
								!data.actions.includes('FoundationaLLM.DataPipeline/dataPipelines/write')
							"
							:style="{
								pointerEvents: !data.actions.includes(
									'FoundationaLLM.DataPipeline/dataPipelines/write',
								)
									? 'none'
									: 'auto',
							}"
							class="table__button"
						>
							<Button
								link
								:disabled="
									!data.actions.includes('FoundationaLLM.DataPipeline/dataPipelines/write')
								"
								:aria-label="`Edit ${data.resource.name}`"
							>
								<i class="pi pi-cog" style="font-size: 1.2rem" aria-hidden="true"></i>
							</Button>
						</NuxtLink>
					</template>
				</Column>

				<!-- Run -->
				<Column
					header="Run"
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
							:disabled="
								!data.actions.includes('FoundationaLLM.DataPipeline/dataPipelines/write')
							"
							:aria-label="`Run ${data.resource.name}`"
							@click="openTriggerPipeline(data.resource)"
						>
							<i class="pi pi-play-circle" style="font-size: 1.2rem" aria-hidden="true"></i>
						</Button>
					</template>
				</Column>

				<!-- Run -->
				<!-- <Column
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
							label="Run"
							:aria-label="`Run ${data.resource.name}`"
							@click="dataPipelineToRun = data"
						/>
					</template>
				</Column> -->
			</DataTable>
		</div>

		<!-- View Pipeline -->
		<Dialog :visible="pipelineToView !== null" modal :header="pipelineToView?.name">
			<div class="dialog-content">
				<div class="details-grid">
					<div class="detail-item">
						<h4>Description</h4>
						<p>{{ pipelineToView?.description || '-' }}</p>
					</div>
					<div class="detail-item">
						<h4>Data Source</h4>
						<p>{{ pipelineToView?.data_source || '-' }}</p>
					</div>
					<div class="detail-item">
						<h4>Trigger</h4>
						<p>{{ pipelineToView?.trigger_type || '-' }}</p>
					</div>
					<div class="detail-item">
						<h4>Embedding</h4>
						<p>{{ pipelineToView?.embedding || '-' }}</p>
					</div>
					<div class="detail-item">
						<h4>Active</h4>
						<p>{{ pipelineToView?.active ? 'Yes' : 'No' }}</p>
					</div>
					<div class="detail-item">
						<h4>Vector Store</h4>
						<p>{{ pipelineToView?.vector_store || '-' }}</p>
					</div>
				</div>

				<h3>Pipeline Runs</h3>
				<table class="pipeline-runs-table">
					<thead>
						<tr>
							<th>Start</th>
							<th>End</th>
							<th>Status</th>
							<th></th>
						</tr>
					</thead>
					<tbody>
						<tr v-for="run in pipelineToView?.runs || []" :key="run.id">
							<td>{{ run.start }}</td>
							<td>{{ run.end }}</td>
							<td>{{ run.status }}</td>
							<td>
								<button class="view-button" @click="viewRun(run)">View</button>
							</td>
						</tr>
					</tbody>
				</table>
			</div>
		</Dialog>

		<!-- Trigger Pipeline Modal -->
		<Dialog 
		:visible="selectedPipelineResource !== null" 
		modal
		closable
		:header="'Trigger pipeline — ' + selectedPipelineResource?.name"
		@update:visible="closeTriggerPipeline"
		>
			<form @submit.prevent="triggerPipeline">

				<div>
					<label>Data Pipeline processor</label>
				</div>
				<div style="margin-bottom: 1rem">
					<Dropdown
						v-model="selectedProcessor"
						:options="processorOptions"
						option-label="value"
						option-value="value"
						placeholder="--Select Data Pipeline Processor--"
					/>
				</div>

				<div v-for="(value, key) in selectedPipelineParameters" :key="key" class="form-group">
					<div style="margin-bottom: 1rem">
						<label :for="key">{{ value.path }}</label>
						<InputText
							type="text"
							:id="key"
							v-model="selectedPipelineParameters[key].default_value"
							class="w-full"
						/>
					</div>
				</div>

				<div>
					<Button
						class="sidebar-dialog__button"
						label="Trigger"
						text
						@click="triggerPipeline"
					/>
					<Button
						class="sidebar-dialog__button"
						label="Close"
						text
						@click="closeTriggerPipeline"
					/>
				</div>
			</form>
		</Dialog>

	</main>
</template>

<script lang="ts">
import api from '@/js/api';
import type { ResourceProviderGetResult } from '@/js/types';

export default {
	name: 'Pipelines',

	data() {
		return {
			pipelines: [] as ResourceProviderGetResult<Pipeline>[],
			pipelineToView: null,
			selectedProcessor: null,
			selectedPipelineResource: null,
			selectedPipelineParameters: [],
			loading: false as boolean,
			loadingStatusText: 'Retrieving data...' as string,
			filters: {
				global: { value: null, matchMode: 'contains' }
			},
			processorOptions: [
				{ value: 'DataPipelineFrontendWorker' },
				{ value: 'DataPipelineBackendWorker' },
			] as { value: string }[],
		};
	},

	async created() {
		await this.getPipelines();
	},

	beforeUnmount() {
		// Clear filters when leaving the component
		this.filters.global.value = null;
	},

	methods: {
		async getPipelines() {
			this.loading = true;
			try {
				this.pipelines = await api.getPipelines();
			} catch (error) {
				this.$toast.add({
					severity: 'error',
					detail: error?.response?._data || error,
					life: 5000,
				});
			}
			this.loading = false;
		},

		openTriggerPipeline(resource) {
			this.selectedPipelineResource = resource;

			this.selectedPipelineParameters = this.extractPluginParameters(resource);
			//console.log('extractPluginParameters result', JSON.parse(JSON.stringify(this.selectedPipelineParameters)));
		},

		closeTriggerPipeline() {
			this.selectedPipelineResource = null;
			this.selectedPipelineParameters = [];
		},

		triggerPipeline() {
			let errorCount = 0;
			if (!this.selectedPipelineResource) { 
				this.$toast.add({
						severity: 'error',
						detail: 'Invalid pipeline selected.',
						life: 3000,
					});
				errorCount++;
			}

			if (this.selectedPipelineParameters.length === 0) { 
				this.$toast.add({
						severity: 'error',
						detail: 'Invalid pipeline parameter values.',
						life: 3000,
					});
				errorCount++;
			}

			this.selectedPipelineParameters.forEach(param => {
				if (param.default_value === undefined || param.default_value === null) {
					this.$toast.add({
						severity: 'error',
						detail: `Parameter ${param.path} has no value.`,
						life: 3000,
					});
					errorCount++;;
				}
			});

			if (!this.selectedProcessor) { 
				this.$toast.add({
						severity: 'error',
						detail: 'Invalid data pipeline processor.',
						life: 3000,
					});
				errorCount++;;
			}

			if (errorCount > 0) {
				return;
			}

			const payload = {
				data_pipeline_object_id: this.selectedPipelineResource.object_id,
				trigger_name: this.selectedPipelineResource.triggers[0].name,
				trigger_parameter_values: Object.fromEntries(this.selectedPipelineParameters.map(x => [x.path, x.default_value])),
				processor: this.selectedProcessor,
			};

			//console.log('Triggering pipeline with payload:', JSON.stringify(payload));

			api.triggerPipeline(this.selectedPipelineResource.name, payload)
				.then(() => {
					this.$toast.add({
						severity: 'success',
						detail: 'Pipeline ${this.selectedPipelineResource.name} triggered successfully.',
						life: 3000,
					});
					this.closeTriggerPipeline();
				})
				.catch(error => {
					this.$toast.add({
						severity: 'error',
						detail: error?.response?._data || error,
						life: 5000,
					});
				});
		},

		// extractPluginParameters(obj, results = []) {
		// 	if (obj && typeof obj === 'object') {
		// 		for (const key in obj) {
		// 			if (key === 'plugin_parameters') {
		// 				const params = obj[key]
		// 				if (Array.isArray(params) && params.length > 0) {
		// 					results.push(...params)
		// 				}
		// 			} else {
		// 				this.extractPluginParameters(obj[key], results)
		// 			}
		// 		}
		// 	} else if (Array.isArray(obj)) {
		// 		for (const item of obj) {
		// 			this.extractPluginParameters(item, results)
		// 		}
		// 	}
		// 	return results
		// },

		extractPluginParameters(obj, context = []) {
  			const results = []

			const that = this;
			function recurse(current, ctx) {
				if (Array.isArray(current)) {
				current.forEach(item => recurse(item, ctx))
				} else if (current && typeof current === 'object') {
					if (Array.isArray(current.plugin_parameters) && current.plugin_parameters.length > 0) {
						current.plugin_parameters.forEach(param => {
							const paramName = param.parameter_metadata?.name
							results.push({
								...param,
								path: [...ctx, paramName].join('.')
							})
						})
					}

					for (const key in current) {
						//console.log('ctx', ctx);
						const value = current[key]

						if (key === 'data_source') {
							recurse(value, [...ctx, 'DataSource', value.name])
						} else if (key === 'plugin_dependencies') {
							const deps = Array.isArray(value) ? value : []
							deps.forEach(dep => {
								const name = that.extractPluginName(dep)
								recurse(dep, [...ctx, 'Dependency', name])
							})
						} else if (key === 'starting_stages' || key === 'next_stages') {
							const stages = Array.isArray(value) ? value : []
							// var exists = stages.filter(s => s.name.startsWith('Stage'));
							// if (exists.length !== 0){
							// 	ctx = []
							// }
							stages.forEach(stage => {
								const name = stage.name
								//const newCtx = ctx.filter((v, i, arr) => !(arr[i - 1] === 'Stage'))
								let newCtx = [...ctx]
								const stageIndex = newCtx.lastIndexOf('Stage')
								if (stageIndex !== -1 && newCtx.length > stageIndex + 1) {
									newCtx.splice(stageIndex, 2)
								}
								recurse(stage, [...newCtx, 'Stage', name])
							})
						} else if (typeof value === 'object' && value !== null) {
							recurse(value, ctx)
						}
					}
				}
			}

			recurse(obj, context)
			return results //.sort((a, b) => a.path.localeCompare(b.path))
		},

		extractPluginName(obj) {
			if (obj.name) return obj.name
			if (obj.plugin_object_id) return obj.plugin_object_id.split('/').pop()?.split('-').pop()
			return 'Undefined'
		},

		// async getPipeline(pipelineName: string) {
		// 	try {
		// 		const pipeline = await api.getPipeline(pipelineName);
		// 	} catch (error) {
		// 		this.$toast.add({
		// 			severity: 'error',
		// 			detail: error?.response?._data || error,
		// 			life: 5000,
		// 		});
		// 	}
		// },

		// async getPipelineRuns(pipelineName: string) {
		//     try {
		//         const runs = await api.getPipelineRuns(pipelineName);
		//         console.log(runs);
		//     } catch (error) {
		//         this.$toast.add({
		//             severity: 'error',
		//             detail: error?.response?._data || error,
		//             life: 5000,
		//         });
		//     }
		// },

		// async getVectorStores() {
		// 	this.loading = true;
		// 	try {
		// 		this.vectorStores = await api.getIndexingProfiles();
		// 	} catch (error) {
		// 		this.$toast.add({
		// 			severity: 'error',
		// 			detail: error?.response?._data || error,
		// 			life: 5000,
		// 		});
		// 	}
		// 	this.loading = false;
		// },

		// async handleDeleteDataSource() {
		// 	try {
		// 		await api.deleteDataSource(this.dataSourceToDelete!.name);
		// 		this.dataSourceToDelete = null;
		// 	} catch (error) {
		// 		return this.$toast.add({
		// 			severity: 'error',
		// 			detail: error?.response?._data || error,
		// 			life: 5000,
		// 		});
		// 	}

		// 	await this.getAgentDataSources();
		// },

		// async handleDeleteVectorStore() {
		// 	try {
		// 		await api.deleteIndexingProfile(this.vectorStoreToDelete!.name);
		// 		this.vectorStoreToDelete = null;
		// 	} catch (error) {
		// 		return this.$toast.add({
		// 			severity: 'error',
		// 			detail: error?.response?._data || error,
		// 			life: 5000,
		// 		});
		// 	}

		// 	await this.getVectorStores();
		// },
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

.dialog-content {
	display: flex;
	flex-direction: column;
	gap: 1.5rem;
}

.details-grid {
	display: grid;
	grid-template-columns: 1fr 1fr;
	gap: 1rem;
}

.detail-item h4 {
	margin: 0 0 0.5rem;
}

.pipeline-runs-table {
	width: 100%;
	border-collapse: collapse;
	margin-top: 1rem;
}

.pipeline-runs-table th,
.pipeline-runs-table td {
	padding: 0.5rem;
	text-align: left;
}
</style>
