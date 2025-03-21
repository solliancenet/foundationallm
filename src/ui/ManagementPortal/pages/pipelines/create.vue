<template>
	<main id="main-content">
		<div style="display: flex">
			<!-- Title -->
			<div style="flex: 1">
				<h2 class="page-header">{{ editId ? 'Edit Pipeline' : 'Create Pipeline' }}</h2>
				<div class="page-subheader">
					{{
						editId
							? 'Edit your pipeline settings below.'
							: 'Complete the settings below to configure the pipeline.'
					}}
				</div>
			</div>
		</div>

		<!-- Steps -->
		<div class="steps">
			<!-- Loading overlay -->
			<template v-if="loading">
				<div class="steps__loading-overlay" role="status" aria-live="polite">
					<LoadingGrid />
					<div>{{ loadingStatusText }}</div>
				</div>
			</template>

			<!-- Name -->
			<div class="step-header span-2">What is the name of the pipeline?</div>
			<div class="span-2">
				<div id="aria-pipeline-name" class="mb-2">Pipeline name:</div>
				<div id="aria-pipeline-name-desc" class="mb-2">
					No special characters or spaces, use letters and numbers with dashes and underscores only.
				</div>
				<div class="input-wrapper">
					<InputText
						v-model="pipeline.name"
						:disabled="isEditing"
						type="text"
						class="w-100"
						placeholder="Enter pipeline name"
						aria-labelledby="aria-pipeline-name aria-pipeline-name-desc"
						@input="handleNameInput"
					/>
					<span
						v-if="nameValidationStatus === 'valid'"
						class="icon valid"
						title="Name is available"
						aria-label="Name is available"
					>
						✔️
					</span>
					<span
						v-else-if="nameValidationStatus === 'invalid'"
						:title="validationMessage || ''"
						class="icon invalid"
						:aria-label="validationMessage || ''"
					>
						❌
					</span>
				</div>

				<div id="aria-pipeline-display-name" class="mb-2 mt-2">Pipeline display name:</div>
				<div class="input-wrapper">
					<InputText
						v-model="pipeline.display_name"
						type="text"
						class="w-100"
						placeholder="Enter a display name for this pipeline"
					/>
				</div>

				<div id="aria-pipeline-desc" class="mb-2 mt-2">Pipeline description:</div>
				<div class="input-wrapper">
					<InputText
						v-model="pipeline.description"
						type="text"
						class="w-100"
						placeholder="Enter a description for this pipeline"
						aria-labelledby="aria-pipeline-desc"
					/>
				</div>
			</div>

			<!-- Data Source -->
			<div class="step-header span-2">Select a data source</div>
			<div class="span-2">
				<Dropdown
					v-model="selectedDataSource"
					:options="dataSourceOptions"
					option-label="name"
					class="w-100"
					placeholder="Select a data source"
					@change="handleDataSourceChange"
				/>

				<div id="aria-data-source-name" class="mb-2 mt-2">Data source name:</div>
				<div class="input-wrapper">
					<InputText
						v-model="pipeline.data_source.name"
						type="text"
						class="w-100"
						placeholder="Enter a name for the data source"
						aria-labelledby="aria-data-source-name"
						@input="handleDataSourceNameInput"
					/>
				</div>

				<div id="aria-data-source-description" class="mb-2 mt-2">Data source description:</div>
				<div class="input-wrapper">
					<InputText
						v-model="pipeline.data_source.description"
						type="text"
						class="w-100"
						placeholder="Enter a description for the data source"
						aria-labelledby="aria-data-source-description"
					/>
				</div>

				<div id="aria-data-source-plugin" class="mb-2 mt-2">Data source plugin:</div>
				<div class="input-wrapper">
					<Dropdown
						v-model="selectedDataSourcePlugin"
						:options="dataSourcePluginOptions"
						option-label="display_name"
						class="w-100"
						placeholder="Select a data source plugin"
					/>
				</div>

				<div id="aria-data-source-parameters" class="mb-2 mt-2">Data source default values:</div>
				<div class="input-wrapper">
					<div
						v-for="(param, index) in selectedDataSourcePlugin?.parameters"
						:key="index"
						style="width: 100%"
					>
						<label>{{ param.name }}:</label>
						<template
							v-if="
								param.type === 'string' ||
								param.type === 'int' ||
								param.type === 'float' ||
								param.type === 'datetime'
							"
						>
							<InputText v-model="param.default_value" style="width: 100%" />
						</template>
						<template v-else-if="param.type === 'bool'">
							<InputSwitch v-model="param.default_value" />
						</template>
						<template v-else-if="param.type === 'array'">
							<Chips
								v-model="param.default_value"
								style="width: 100%"
								placeholder="Enter values separated by commas"
								separator=","
							></Chips>
						</template>
						<template v-else-if="param.type === 'resource-object-id'">
							<Dropdown
								v-model="param.default_value"
								:options="param.parameter_metadata.parameter_selection_hints_options"
								option-label="display_name"
								option-value="value"
								class="w-100"
								placeholder="Select a resource"
							/>
						</template>
					</div>
				</div>
			</div>

			<!-- Pipeline Stages -->
			<div class="step-header span-2">Configure pipeline stages</div>
			<div class="span-2">
				<div class="stages-container">
					<div
						v-for="(stage, index) in selectedStagePlugins"
						:key="index"
						class="stage-item"
						draggable="true"
						@dragstart="handleDragStart(index)"
						@dragover.prevent
						@drop="handleDrop(index)"
					>
						<div class="stage-header">
							<div class="mb-2">
								<label>Stage Name:</label>
								<InputText
									v-model="stage.name"
									class="w-100"
									@input="handleStageNameInput($event, index)"
								/>
							</div>
							<Button icon="pi pi-trash" severity="danger" @click="removeStage(index)" />
						</div>
						<div class="stage-content">
							<div class="mb-2">
								<label>Description:</label>
								<InputText v-model="stage.description" class="w-100" />
							</div>
							<div class="mb-2">
								<label>Plugin:</label>
								<Dropdown
									v-model="stage.plugin_object_id"
									:options="stagePluginsOptions"
									option-label="display_name"
									option-value="object_id"
									class="w-100"
									placeholder="Select a plugin"
									@change="handleStagePluginChange($event, index)"
								/>
							</div>
							<div v-if="stage.plugin_parameters" class="mb-2">
								<label class="step-header">Parameters:</label>
								<div
									v-for="(param, paramIndex) in stage.plugin_parameters"
									:key="paramIndex"
									class="parameter-item"
								>
									<label>{{ param.parameter_metadata.name }}:</label>
									<div style="font-size: 12px; color: #666">
										{{ param.parameter_metadata.description }}
									</div>
									<template
										v-if="
											param.parameter_metadata.type === 'string' ||
											param.parameter_metadata.type === 'int' ||
											param.parameter_metadata.type === 'float' ||
											param.parameter_metadata.type === 'datetime'
										"
									>
										<InputText v-model="param.default_value" style="width: 100%" />
									</template>
									<template v-else-if="param.parameter_metadata.type === 'bool'">
										<InputSwitch v-model="param.default_value" />
									</template>
									<template v-else-if="param.parameter_metadata.type === 'array'">
										<Chips
											v-model="param.default_value"
											style="width: 100%"
											placeholder="Enter values separated by commas"
											separator=","
										></Chips>
									</template>
									<template v-else-if="param.parameter_metadata.type === 'resource-object-id'">
										<Dropdown
											v-model="param.default_value"
											:options="
												stagePluginResourceOptions.find(
													(p) => p.parameter_metadata.name === param.parameter_metadata.name,
												)?.parameter_selection_hints_options || []
											"
											option-label="display_name"
											option-value="value"
											class="w-100"
											placeholder="Select a resource"
										/>
									</template>
								</div>
							</div>
							<div class="mb-2">
								<label>Dependencies:</label>
								<Dropdown
									v-if="
										stagePluginsDependenciesOptions.find(
											(dep) => dep.plugin_object_id === stage.plugin_object_id,
										)?.selection_type === 'Single'
									"
									v-model="stage.plugin_dependencies[0]"
									:options="
										stagePluginsDependenciesOptions.find(
											(dep) => dep.plugin_object_id === stage.plugin_object_id,
										)?.dependencyInfo || []
									"
									option-label="dependencyLabel"
									option-value="dependencies"
									class="w-100"
									placeholder="Select a dependency"
								/>
								<MultiSelect
									v-if="
										stagePluginsDependenciesOptions.find(
											(dep) => dep.plugin_object_id === stage.plugin_object_id,
										)?.selection_type === 'Multiple'
									"
									v-model="stage.plugin_dependencies"
									:options="
										stagePluginsDependenciesOptions.find(
											(dep) => dep.plugin_object_id === stage.plugin_object_id,
										)?.dependencyInfo || []
									"
									option-label="dependencyLabel"
									option-value="dependencies"
									class="w-100"
									placeholder="Select dependencies"
								/>
							</div>
							<div v-if="stage.plugin_dependencies[0]?.plugin_parameters" class="mb-2">
								<label class="step-header">Dependency Parameters:</label>
								<div
									v-for="(param, paramIndex) in stage?.plugin_dependencies[0]?.plugin_parameters"
									:key="paramIndex"
									class="parameter-item"
								>
									<label>{{ param.parameter_metadata.name }}:</label>
									<div style="font-size: 12px; color: #666">
										{{ param.parameter_metadata.description }}
									</div>
									<template
										v-if="
											param.parameter_metadata.type === 'string' ||
											param.parameter_metadata.type === 'int' ||
											param.parameter_metadata.type === 'float' ||
											param.parameter_metadata.type === 'datetime'
										"
									>
										<InputText v-model="param.default_value" class="w-100" />
									</template>
									<template v-else-if="param.parameter_metadata.type === 'bool'">
										<InputSwitch v-model="param.default_value" />
									</template>
									<template v-else-if="param.parameter_metadata.type === 'array'">
										<Chips
											v-model="param.default_value"
											style="width: 100%"
											placeholder="Enter values separated by commas"
											separator=","
										></Chips>
									</template>
									<template v-else-if="param.parameter_metadata.type === 'resource-object-id'">
										<Dropdown
											v-model="param.default_value"
											:options="
												stagePluginDependencyResourceOptions.find(
													(p) => p.parameter_metadata.name === param.parameter_metadata.name,
												)?.parameter_selection_hints_options || []
											"
											option-label="display_name"
											option-value="value"
											class="w-100"
											placeholder="Select a resource"
										/>
									</template>
								</div>
							</div>
						</div>
					</div>
					<Button label="Add Stage" icon="pi pi-plus" @click="addStage" />
				</div>
			</div>

			<!-- Pipeline Triggers -->
			<div class="step-header span-2">Configure pipeline triggers</div>
			<div class="span-2">
				<div class="trigger-container">
					<div
						v-for="(trigger, triggerIndex) in pipeline.triggers"
						:key="triggerIndex"
						class="mb-2 trigger-item"
					>
						<div class="trigger-header">
							<div class="mb-2">
								<label>Trigger Name:</label>
								<InputText v-model="trigger.name" class="w-100" />
							</div>
							<Button icon="pi pi-trash" severity="danger" @click="removeTrigger(triggerIndex)" />
						</div>

						<label>Trigger Type:</label>
						<Dropdown
							v-model="trigger.trigger_type"
							:options="triggerTypeOptions"
							option-label="label"
							option-value="value"
							class="w-100"
							placeholder="Select trigger type"
						/>
						<div v-if="trigger.trigger_type === 'Schedule'" class="mb-2">
							<label>Cron Schedule:</label>
							<InputText
								v-model="trigger.trigger_cron_schedule"
								class="w-100"
								placeholder="0 6 * * *"
							/>
						</div>
						<div class="step-header span-2 mb-2">Trigger Parameters:</div>
						<div class="span-2">
							<div v-for="(param, index) in triggerParameters" :key="index" class="mb-2">
								<label>{{ param.parameter_metadata.name }}:</label>
								<div style="font-size: 12px; color: #666">
									{{ param.parameter_metadata.description }}
								</div>
								<template
									v-if="
										param.parameter_metadata.type === 'string' ||
										param.parameter_metadata.type === 'int' ||
										param.parameter_metadata.type === 'float' ||
										param.parameter_metadata.type === 'datetime'
									"
								>
									<InputText
										v-model="pipeline.triggers[triggerIndex].parameter_values[param.key]"
										class="w-100"
									/>
								</template>
								<template v-else-if="param.parameter_metadata.type === 'bool'">
									<InputSwitch
										v-model="pipeline.triggers[triggerIndex].parameter_values[param.key]"
									/>
								</template>
								<template v-else-if="param.parameter_metadata.type === 'array'">
									<Chips
										v-model="pipeline.triggers[triggerIndex].parameter_values[param.key]"
										style="width: 100%"
										placeholder="Enter values separated by commas"
										separator=","
									></Chips>
								</template>
								<template v-else-if="param.parameter_metadata.type === 'resource-object-id'">
									<Dropdown
										v-model="pipeline.triggers[triggerIndex].parameter_values[param.key]"
										:options="param.resourceOptions"
										option-label="display_name"
										option-value="value"
										class="w-100"
										placeholder="Select a resource"
									/>
								</template>
							</div>
						</div>
					</div>
					<Button label="Add Trigger" icon="pi pi-plus" @click="addTrigger" />
				</div>
			</div>

			<!-- Buttons -->
			<div class="button-container column-2 justify-self-end">
				<!-- Create pipeline -->
				<Button
					:label="editId ? 'Save Changes' : 'Create Pipeline'"
					severity="primary"
					@click="handleCreatePipeline"
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
import type { PropType } from 'vue';
import { debounce } from 'lodash';
import api from '@/js/api';

export default {
	name: 'CreatePipeline',

	props: {
		editId: {
			type: [Boolean, String] as PropType<boolean | string>,
			required: false,
			default: false,
		},
	},

	data() {
		return {
			loading: true as boolean,
			loadingStatusText: 'Retrieving data...' as string,

			nameValidationStatus: null as string | null,
			validationMessage: null as string | null,

			dataSourceOptions: [] as any[],
			selectedDataSource: null as string | null,

			dataSourcePlugins: [] as any[],
			selectedDataSourcePlugin: null as any,
			dataSourcePluginOptions: [] as any[],

			stagePluginsOptions: [] as any[],
			selectedStagePlugins: [] as any[],
			stagePluginResourceOptions: [] as any[],

			stagePluginsDependenciesOptions: [] as any[],
			resolvedDependencies: [] as any[],
			stagePluginDependencyResourceOptions: [] as any[],

			triggerTypeOptions: [
				{ label: 'Schedule', value: 'Schedule' },
				{ label: 'Event', value: 'Event' },
				{ label: 'Manual', value: 'Manual' },
			],

			triggerParameters: [] as any[],

			pipeline: {
				type: 'data-pipeline',
				name: '',
				object_id: '',
				display_name: '',
				description: '',
				cost_center: null,
				active: false,
				data_source: {
					data_source_object_id: '',
					name: '',
					description: '',
					plugin_object_id: '',
					plugin_parameters: [],
					plugin_dependencies: [],
				},
				starting_stages: [],
				triggers: [],
				properties: null,
				created_on: '',
				updated_on: '',
				created_by: null,
				updated_by: null,
				deleted: false,
				expiration_date: null,
			},

			debouncedCheckName: null as (() => void) | null,
			draggedStageIndex: null as number | null,
			resourceOptions: [] as any[],
			resourceOptionsCache: {} as Record<string, any[]>, // Cache for resource options
		};
	},

	computed: {
		isEditing(): boolean {
			return Boolean(this.editId);
		},

		pipelineId(): string | undefined {
			return typeof this.editId === 'string' ? this.editId : undefined;
		},
	},

	watch: {
		pipeline: {
			handler(newVal) {
				console.log(newVal);
			},
			deep: true,
		},
		selectedDataSourcePlugin: {
			async handler(newVal) {
				if (newVal) {
					this.pipeline.data_source.plugin_object_id = newVal.object_id;
					this.pipeline.data_source.plugin_parameters = newVal.parameters.map((param: any) => ({
						parameter_metadata: {
							name: param.name,
							type: param.type,
							description: param.description,
						},
						default_value: param.default_value,
					}));

					// Avoid multiple API requests
					const paramsToFetch = newVal.parameter_selection_hints
						? Object.keys(newVal.parameter_selection_hints)
						: [];

					// Use Promise.all to fetch all options in parallel
					const resourceOptions = await Promise.all(
						paramsToFetch.map((key) =>
							this.getResourceOptions(key, newVal.object_id).then((options) => ({ key, options })),
						),
					);

					// Assign options efficiently
					resourceOptions.forEach(({ key, options }) => {
						const param = this.selectedDataSourcePlugin.parameters.find((p) => p.name === key);
						if (param) {
							param.parameter_selection_hints_options = options;
						}
					});
				}
				this.buildTriggerParameters();
			},
			deep: true,
		},
		selectedStagePlugins: {
			handler(newVal) {
				this.transformPipelineStages();
				this.buildTriggerParameters();
				newVal.forEach((stage) => {
					this.loadStagePluginDependencies(stage.plugin_object_id);
				});
			},
			deep: true,
		},
	},

	async created() {
		this.loading = true;

		try {
			// Load data sources
			const dataSourceResponse = await api.getAgentDataSources();
			this.dataSourceOptions = dataSourceResponse.map((result) => result.resource);

			// Load data source plugins
			const dataSourcePluginsResponse = await api.filterPlugins(['Data Source']);
			this.dataSourcePluginOptions = dataSourcePluginsResponse;

			// Load stage plugins
			const stagePluginsResponse = await api.filterPlugins(['Data Pipeline Stage']);
			this.stagePluginsOptions = stagePluginsResponse.map((result: any) => ({
				...result,
				object_id: result.object_id,
			}));
			this.stagePluginsOptions.forEach((plugin) => {
				this.buildStagePluginResourceOptions(plugin);
			});

			if (this.pipelineId) {
				this.loadingStatusText = `Retrieving pipeline "${this.pipelineId}"...`;
				const pipelineResult = await api.getPipeline(this.pipelineId);
				this.pipeline = pipelineResult[0].resource;

				this.selectedDataSource = this.dataSourceOptions.find(
					(option) => option.object_id === `/${this.pipeline.data_source.data_source_object_id}`,
				);

				this.selectedDataSourcePlugin = this.dataSourcePluginOptions.find(
					(plugin) => plugin.object_id === this.pipeline.data_source.plugin_object_id,
				);

				this.handleNextStages(this.pipeline.starting_stages);

				this.buildTriggerParameters();
			}
		} catch (error) {
			console.error('Error loading data:', error);
			(this as any).$toast.add({
				severity: 'error',
				detail: 'Error loading pipeline data. Please try again.',
				life: 5000,
			});
		}

		this.debouncedCheckName = debounce(this.checkName, 500);
		this.loading = false;
	},

	methods: {
		async checkName() {
			try {
				const response = await api.checkPipelineName(this.pipeline.name);

				if (response.status === 'Allowed') {
					this.nameValidationStatus = 'valid';
					this.validationMessage = null;
				} else if (response.status === 'Denied') {
					this.nameValidationStatus = 'invalid';
					this.validationMessage = response.message;
				}
			} catch (error) {
				console.error('Error checking pipeline name: ', error);
				this.nameValidationStatus = 'invalid';
				this.validationMessage = 'Error checking the pipeline name. Please try again.';
			}
		},

		handleDataSourceChange(event: any) {
			// This needs to be updated to handle the data source change
			const selectedDataSource = this.dataSourceOptions.find(
				(p) => p.object_id === event.value.object_id,
			);
			if (selectedDataSource) {
				this.selectedDataSourcePlugin = null;

				this.pipeline.data_source = {
					data_source_object_id: selectedDataSource.object_id,
					name: '',
					description: '',
					plugin_object_id: '',
					plugin_parameters: [],
					plugin_dependencies: [],
				};
			}
		},

		handleStagePluginChange(event: any, stageIndex: number) {
			const selectedPlugin = this.stagePluginsOptions.find((p) => p.object_id === event.value);
			this.selectedStagePlugins[stageIndex].plugin_object_id = selectedPlugin.object_id;
			this.selectedStagePlugins[stageIndex].plugin_parameters = selectedPlugin.parameters.map(
				(param) => ({
					parameter_metadata: param,
					default_value: null,
				}),
			);
			this.selectedStagePlugins[stageIndex].plugin_dependencies = [];
		},

		handleStagePluginDependencyChange(event: any, index: number) {
			const selectedDependencies = event.value || [];
			this.selectedStagePlugins[index].plugin_dependencies = selectedDependencies.map(
				(dependency) => {
					const selectedDependency = this.stagePluginsOptions.find(
						(p) => p.object_id === dependency.plugin_object_id,
					);
					return {
						plugin_object_id: selectedDependency ? selectedDependency.object_id : '',
						plugin_parameters:
							selectedDependency && selectedDependency.parameters
								? selectedDependency.parameters.map((param) => ({
										parameter_metadata: param,
										default_value: null,
									}))
								: [],
					};
				},
			);
		},

		addStage() {
			this.selectedStagePlugins.push({
				name: `Stage${this.selectedStagePlugins.length + 1}`,
				description: '',
				plugin_object_id: '',
				plugin_parameters: null,
				plugin_dependencies: [],
			});
			this.transformPipelineStages();
		},

		removeStage(index: number) {
			this.selectedStagePlugins.splice(index, 1);
			this.transformPipelineStages();
		},

		transformPipelineStages() {
			const nested = this.selectedStagePlugins.reduceRight((acc, stage) => {
				return [
					{
						...stage,
						next_stages: acc,
					},
				];
			}, []);

			this.pipeline.starting_stages = nested;
		},

		handleCancel() {
			if (!confirm('Are you sure you want to cancel?')) {
				return;
			}

			(this as any).$router.push('/pipelines');
		},

		handleNameInput(event: Event) {
			const sanitizedValue = (this as any).$filters.sanitizeNameInput(event);
			this.pipeline.name = sanitizedValue;
			// this.pipeline.display_name = sanitizedValue;

			if (!this.isEditing && this.debouncedCheckName) {
				this.debouncedCheckName();
			}
		},

		handleDataSourceNameInput(event: Event) {
			const sanitizedValue = (this as any).$filters.sanitizeNameInput(event);
			this.pipeline.data_source.name = sanitizedValue;
		},

		handleStageNameInput(event: Event, index: number) {
			const sanitizedValue = (this as any).$filters.sanitizeNameInput(event);
			this.selectedStagePlugins[index].name = sanitizedValue;
		},

		async handleCreatePipeline() {
			const pipeline = {
				name: this.pipeline.name,
				type: 'data-pipeline',
				display_name: this.pipeline.display_name,
				description: this.pipeline.description,
				active: false,
				data_source: this.pipeline.data_source,
				starting_stages: this.pipeline.starting_stages,
				triggers: this.pipeline.triggers,
			};
			try {
				if (this.isEditing) {
					await api.createPipeline(pipeline);
					this.$toast.add({
						severity: 'success',
						detail: 'Pipeline updated successfully!',
						life: 3000,
					});
				} else {
					await api.createPipeline(pipeline);
					this.$toast.add({
						severity: 'success',
						detail: 'Pipeline created successfully!',
						life: 3000,
					});
				}
				this.$router.push('/pipelines'); // Redirect to the pipelines list
			} catch (error) {
				console.error('Error saving pipeline:', error);
				this.$toast.add({
					severity: 'error',
					detail: 'Error saving pipeline. Please try again.',
					life: 5000,
				});
			}
		},

		handleNextStages(stages: any[]) {
			stages.forEach((stage: any) => {
				this.selectedStagePlugins.push({
					name: stage.name,
					description: stage.description,
					plugin_object_id: stage.plugin_object_id,
					plugin_parameters: stage.plugin_parameters,
					plugin_dependencies: stage.plugin_dependencies,
				});
				this.loadStagePluginDependencies(stage.plugin_object_id);
				if (stage.next_stages) {
					this.handleNextStages(stage.next_stages);
				}
			});
		},

		buildStagePluginResourceOptions(plugin: any) {
			if (this.stagePluginResourceOptions.find((p) => p.parameter_metadata.name === plugin.name)) {
				return;
			}
			this.stagePluginResourceOptions.push(
				plugin.parameters.map((param: any) => ({
					parameter_metadata: param,
					parameter_selection_hints_options: [],
				})),
			);
			this.stagePluginResourceOptions = this.stagePluginResourceOptions.flat();
			this.stagePluginResourceOptions.forEach(async (param: any) => {
				param.parameter_selection_hints_options = await this.getResourceOptions(
					param.parameter_metadata.name,
					plugin.object_id,
				);
			});
		},

		buildStagePluginDependencyResourceOptions(plugin: any) {
			if (
				this.stagePluginDependencyResourceOptions.find(
					(p) => p.parameter_metadata.name === plugin.name,
				)
			) {
				return;
			}
			this.stagePluginDependencyResourceOptions.push(
				plugin.parameters.map((dep: any) => ({
					parameter_metadata: dep,
					parameter_selection_hints_options: [],
				})),
			);
			this.stagePluginDependencyResourceOptions = this.stagePluginDependencyResourceOptions.flat();
			this.stagePluginDependencyResourceOptions.forEach(async (param: any) => {
				param.parameter_selection_hints_options = await this.getResourceOptions(
					param.parameter_metadata.name,
					plugin.object_id,
				);
			});
		},

		async buildTriggerParameters() {
			const parameterValues: any[] = [];

			// Check if there are existing trigger parameters
			const existingTriggerParameters =
				this.pipeline.triggers.length > 0 ? this.pipeline.triggers[0].parameter_values : {};

			// Data Source Parameters
			for (const param of this.pipeline.data_source.plugin_parameters) {
				const key = `DataSource.${this.pipeline.data_source.name}.${param.parameter_metadata.name}`;
				const value =
					existingTriggerParameters[key] !== undefined
						? existingTriggerParameters[key]
						: param.default_value;
				this.pipeline.triggers.forEach((trigger) => {
					if (!trigger.parameter_values[key]) {
						trigger.parameter_values[key] = null;
					}
				});
				const resourceOptions = await this.getResourceOptions(
					param.parameter_metadata.name,
					this.pipeline.data_source.plugin_object_id,
				);
				parameterValues.push({
					parameter_metadata: param.parameter_metadata,
					key,
					value,
					resourceOptions,
				});
			}

			// Recursive function to handle stages and their dependencies
			async function handleStages(stages: any[]) {
				for (const stage of stages) {
					// Stage Parameters
					if (stage.plugin_parameters) {
						for (const param of stage.plugin_parameters) {
							const key = `Stage.${stage.name}.${param.parameter_metadata.name}`;
							const value =
								existingTriggerParameters[key] !== undefined
									? existingTriggerParameters[key]
									: param.default_value;
							const resourceOptions = await this.getResourceOptions(
								param.parameter_metadata.name,
								stage.plugin_object_id,
							);
							parameterValues.push({
								parameter_metadata: param.parameter_metadata,
								key,
								value,
								resourceOptions,
							});
						}
					}

					// Dependency Plugin Parameters
					if (stage.plugin_dependencies) {
						for (const dep of stage.plugin_dependencies) {
							for (const param of dep.plugin_parameters) {
								const depPluginName = dep.plugin_object_id.split('/').pop() || '';
								const key = `Stage.${stage.name}.Dependency.${depPluginName}.${param.parameter_metadata.name}`;
								const value =
									existingTriggerParameters[key] !== undefined
										? existingTriggerParameters[key]
										: param.default_value;
								const resourceOptions = await this.getResourceOptions(
									param.parameter_metadata.name,
									dep.plugin_object_id,
								);
								parameterValues.push({
									parameter_metadata: param.parameter_metadata,
									key,
									value,
									resourceOptions,
								});
							}
						}
					}

					// Handle next stages recursively
					if (stage.next_stages) {
						await handleStages.call(this, stage.next_stages);
					}
				}
			}

			// Start with the initial stages
			await handleStages.call(this, this.pipeline.starting_stages);

			this.triggerParameters = parameterValues;
		},

		handleDragStart(index: number) {
			this.draggedStageIndex = index;
		},

		handleDrop(index: number) {
			if (this.draggedStageIndex !== null) {
				const draggedStage = this.selectedStagePlugins[this.draggedStageIndex];
				this.selectedStagePlugins.splice(this.draggedStageIndex, 1);
				this.selectedStagePlugins.splice(index, 0, draggedStage);
				this.draggedStageIndex = null;
				this.transformPipelineStages();
			}
		},

		addTrigger() {
			const parameterValues = {};
			this.triggerParameters.forEach((param) => {
				parameterValues[param.key] = null;
			});
			this.pipeline.triggers.push({
				name: `Trigger${this.pipeline.triggers.length + 1}`,
				trigger_type: 'Schedule',
				trigger_cron_schedule: '0 6 * * *',
				parameter_values: parameterValues,
			});
		},

		removeTrigger(index) {
			this.pipeline.triggers.splice(index, 1);
		},

		async loadStagePluginDependencies(pluginObjectId: string) {
			const plugin = this.stagePluginsOptions.find((p) => p.object_id === pluginObjectId);
			if (plugin && plugin.dependencies.length > 0) {
				const dependencies = plugin.dependencies[0]?.dependency_plugin_names || [];
				const dependencyPluginPromises = dependencies.map(async (dependency: string) => {
					const dependencyPlugin = await api.getPlugin(dependency);
					return dependencyPlugin[0].resource;
				});
				const resolvedDependencies = await Promise.all(dependencyPluginPromises);
				resolvedDependencies.forEach((dep) => {
					this.resolvedDependencies.push(dep);
					this.buildStagePluginDependencyResourceOptions(dep);
				});

				this.stagePluginsDependenciesOptions.push({
					plugin_object_id: plugin.object_id,
					selection_type: plugin.dependencies[0]?.selection_type || 'Single',
					dependencyInfo: resolvedDependencies.map((dep) => ({
						dependencyLabel: dep.display_name,
						dependencies: {
							plugin_object_id: dep.object_id,
							plugin_parameters: dep.parameters.map((param) => ({
								parameter_metadata: param,
								default_value: null,
							})),
						},
					})),
				});
			}
		},

		async getResourceOptions(paramName: string, pluginObjectId: string) {
			const cacheKey = `${paramName}-${pluginObjectId}`;

			// Return cached options if available
			if (this.resourceOptionsCache[cacheKey]) {
				return this.resourceOptionsCache[cacheKey];
			}

			const plugin =
				this.stagePluginsOptions.find((p) => p.object_id === pluginObjectId) ||
				this.resolvedDependencies.find((dep) => dep.object_id === pluginObjectId) ||
				this.dataSourcePluginOptions.find((p) => p.object_id === pluginObjectId);

			if (
				!plugin ||
				!plugin.parameter_selection_hints ||
				!plugin.parameter_selection_hints[paramName]
			) {
				return [];
			}

			const hints = plugin.parameter_selection_hints[paramName];

			try {
				const response = await api.filterResources(hints.resourcePath, hints.filterActionPayload);
				const options = response.map((resource) => ({
					display_name: resource.display_name ?? resource.name,
					value: resource.object_id,
				}));

				// Cache the response to prevent redundant API calls
				this.resourceOptionsCache[cacheKey] = options;
				return options;
			} catch (error) {
				console.error('Error fetching resources:', error);
				return [];
			}
		},

		async updateResourceOptions(paramName: string, pluginObjectId: string) {
			this.resourceOptions = await this.getResourceOptions(paramName, pluginObjectId);
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
	pointer-events: auto;
}

.step-header {
	font-weight: bold;
	margin-bottom: -10px;
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

.stages-container {
	display: flex;
	flex-direction: column;
	gap: 16px;
}

.stage-item {
	border: 1px solid #e1e1e1;
	padding: 16px;
	border-radius: 4px;
}

.trigger-item {
	border: 1px solid #e1e1e1;
	padding: 16px;
	border-radius: 4px;
}

.stage-header {
	display: flex;
	justify-content: space-between;
	align-items: center;
	margin-bottom: 16px;

	h3 {
		margin: 0;
	}
}

.trigger-header {
	display: flex;
	justify-content: space-between;
	align-items: center;
	margin-bottom: 16px;
}

.stage-content {
	display: flex;
	flex-direction: column;
	gap: 8px;
}

.parameter-item {
	display: flex;
	flex-direction: column;
	gap: 4px;
	margin-bottom: 8px;
}

.trigger-container {
	display: flex;
	flex-direction: column;
	gap: 8px;
}
</style>
