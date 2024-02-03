<template>
	<div>
		<h2 class="page-header">Create New Agent</h2>
		<div class="page-subheader">
			Complete the settings below to create and deploy your new agent.
		</div>

		<div class="steps" :class="{ 'steps--loading': loading }">
			<!-- Loading overlay -->
			<template v-if="loading">
				<div class="steps__loading-overlay">
					<LoadingGrid />
					<div>{{ loadingStatusText }}</div>
				</div>
			</template>

			<div class="span-2">
				<div class="step-header mb-2">Agent name:</div>
				<div class="mb-2">No special characters or spaces, lowercase letters with dashes and underscores only.</div>
				<InputText v-model="agentName" placeholder="Enter agent name" type="text" class="w-100" @input="handleNameInput" />
			</div>

			<!-- Type -->
			<div class="step-section-header span-2">Type</div>

			<div class="step-header span-2">What type of agent?</div>

			<!-- Knowledge management agent -->
			<div class="step">
				<div
					class="step-container cursor-pointer"
					@click="handleAgentTypeSelect('knowledge-management')"
				>
					<div class="step-container__edit__inner">
						<div class="step__radio">
							<RadioButton v-model="agentType" name="agentType" value="knowledge-management" />
							<div class="step-container__header">Knowledge Management</div>
						</div>
						<div>Best for Q&A, summarization and reasoning over textual data.</div>
					</div>
				</div>
			</div>

			<!-- Analytics agent-->
			<div class="step step--disabled">
				<div class="step-container cursor-pointer" @click="handleAgentTypeSelect('analytics')">
					<div class="step-container__edit__inner">
						<div class="step__radio">
							<RadioButton v-model="agentType" name="agentType" value="analytics" />
							<div class="step-container__header">Analytics</div>
						</div>
						<div>Best to query, analyze, calculate and report on tabular data.</div>
					</div>
				</div>
			</div>

			<!-- Knowledge source -->
			<div class="step-section-header span-2">Knowledge Source</div>

			<div class="step-header">Where is the data?</div>
			<div class="step-header">Where should the data be indexed?</div>

			<!-- Data source -->
			<CreateAgentStepItem v-model="editDataSource">
				<template v-if="selectedDataSource">
					<div class="step-container__header">{{ selectedDataSource.Type }}</div>
					<div>
						<span class="step-option__header">Name:</span>
						<span>{{ selectedDataSource.Name }}</span>
					</div>
					<!-- <div>
						<span class="step-option__header">Container name:</span>
						<span>{{ selectedDataSource.Container.Name }}</span>
					</div> -->
					
					<!-- <div>
						<span class="step-option__header">Data Format(s):</span>
						<span v-for="format in selectedDataSource.Formats" :key="format" class="mr-1">
							{{ format }}
						</span>
					</div> -->
				</template>
				<template v-else>Please select a data source.</template>

				<template #edit>
					<div class="step-container__edit__header">Please select a data source.</div>
					
					<div v-for="(group, type) in groupedDataSources" :key="type">

						<div class="step-container__edit__group-header">{{ type }}</div>

						<div
							v-for="dataSource in group"
							:key="dataSource.Name"
							class="step-container__edit__option"
							:class="{
								'step-container__edit__option--selected':
									dataSource.Name === selectedDataSource?.Name,
							}"
							@click.stop="handleDataSourceSelected(dataSource)"
						>
							<div>
								<span class="step-option__header">Name:</span>
								<span>{{ dataSource.Name }}</span>
							</div>
							<!-- <div>
								<span class="step-option__header">Container name:</span>
								<span>{{ dataSource.Container.Name }}</span>
							</div> -->
							
							<!-- <div>
								<span class="step-option__header">Data Format(s):</span>
								<span v-for="format in dataSource.Formats" :key="format" class="mr-1">
									{{ format }}
								</span>
							</div> -->
						</div>
					</div>
				</template>
			</CreateAgentStepItem>

			<!-- Index source -->
			<CreateAgentStepItem v-model="editIndexSource">
				<template v-if="selectedIndexSource">
					<div class="step-container__header">{{ selectedIndexSource.Name }}</div>
					<div>
						<span class="step-option__header">URL:</span>
						<span>{{ selectedIndexSource.ConfigurationReferences.Endpoint }}</span>
					</div>
					<div>
						<span class="step-option__header">Index Name:</span>
						<span>{{ selectedIndexSource.Settings.IndexName }}</span>
					</div>
				</template>
				<template v-else>Please select an index source.</template>

				<template #edit>
					<div class="step-container__edit__header">Please select an index source.</div>
					<div
						v-for="indexSource in indexSources"
						:key="indexSource.Name"
						class="step-container__edit__option"
						:class="{
							'step-container__edit__option--selected':
								indexSource.Name === selectedIndexSource?.Name,
						}"
						@click.stop="handleIndexSourceSelected(indexSource)"
					>
						<div class="step-container__header">{{ indexSource.Name }}</div>
						<div>
							<span class="step-option__header">URL:</span>
							<span>{{ indexSource.ConfigurationReferences.Endpoint }}</span>
						</div>
						<div>
							<span class="step-option__header">Index Name:</span>
							<span>{{ indexSource.Settings.IndexName }}</span>
						</div>
					</div>
				</template>
			</CreateAgentStepItem>

			<div class="step-header">How should the data be processed for indexing?</div>
			<div class="step-header">When should the data be indexed?</div>

			<!-- Process indexing -->
			<CreateAgentStepItem>
				<div class="step-container__header">Splitting & Chunking</div>

				<div>
					<span class="step-option__header">Chunk size:</span>
					<span>{{ chunkSize }}</span>
				</div>

				<div>
					<span class="step-option__header">Overlap size:</span>
					<span>{{ overlapSize == 0 ? 'No Overlap' : overlapSize }}</span>
				</div>

				<template #edit>
					<div class="step-container__header">Splitting & Chunking</div>

					<div>
						<span class="step-option__header">Chunk size:</span>
						<InputText v-model="chunkSize" type="number" class="mt-2" />
					</div>

					<div>
						<span class="step-option__header">Overlap size:</span>
						<InputText v-model="overlapSize" type="number" class="mt-2" />
					</div>
				</template>
			</CreateAgentStepItem>

			<!-- Trigger -->
			<CreateAgentStepItem>
				<div class="step-container__header">Trigger</div>
				<div>Runs every time a new tile is added to the data source.</div>

				<div class="mt-2">
					<span class="step-option__header">Frequency:</span>
					<span>{{ triggerFrequency.label }}</span>
				</div>

				<div v-if="triggerFrequency.value == 2 && triggerFrequencyScheduled">
					<span class="step-option__header">Schedule:</span>
					<span>{{ triggerFrequencyScheduled.label }}</span>
				</div>

				<template #edit>
					<div class="step-container__header">Trigger</div>
					<div>Runs every time a new tile is added to the data source.</div>

					<div class="mt-2">
						<span class="step-option__header">Frequency:</span>
						<Dropdown
							v-model="triggerFrequency"
							class="dropdown--agent"
							:options="triggerFrequencyOptions"
							option-label="label"
							placeholder="--Select--"
						/>
					</div>

					<div v-if="triggerFrequency.value === 2" class="mt-2">
						<span class="step-option__header">Select schedule:</span>
						<Dropdown
							v-model="triggerFrequencyScheduled"
							class="dropdown--agent"
							:options="triggerFrequencyScheduledOptions"
							option-label="label"
							placeholder="--Select--"
						/>
					</div>
				</template>
			</CreateAgentStepItem>

			<!-- Agent configuration -->
			<div class="step-section-header span-2">Agent Configuration</div>

			<div class="step-header">Should conversations be saved?</div>
			<div class="step-header">How should user-agent interactions be gated?</div>

			<!-- Conversation history -->
			<CreateAgentStepItem>
				<div class="step-container__header">Conversation History</div>

				<div>
					<span class="step-option__header">Enabled:</span>
					<span>
						<span>{{ conversationHistory ? 'Yes' : 'No' }}</span>
						<span
							v-if="conversationHistory"
							class="pi pi-check-circle ml-1"
							style="color: var(--green-400); font-size: 0.8rem"
						></span>
						<span
							v-else
							class="pi pi-times-circle ml-1"
							style="color: var(--red-400); font-size: 0.8rem"
						></span>
					</span>
				</div>

				<div>
					<span class="step-option__header">Max Messages:</span>
					<span>{{ conversationMaxMessages }}</span>
				</div>

				<template #edit>
					<div class="step-container__header">Conversation History</div>

					<div class="d-flex align-center mt-2">
						<span class="step-option__header">Enabled:</span>
						<span>
							<ToggleButton
								v-model="conversationHistory"
								onLabel="Yes"
								onIcon="pi pi-check-circle"
								offLabel="No"
								offIcon="pi pi-times-circle"
							/>
						</span>
					</div>

					<div>
						<span class="step-option__header">Max Messages:</span>
						<InputText v-model="conversationMaxMessages" type="number" class="mt-2" />
					</div>
				</template>
			</CreateAgentStepItem>

			<!-- Gatekeeper -->
			<CreateAgentStepItem>
				<div class="step-container__header">Gatekeeper</div>

				<div>
					<span class="step-option__header">Enabled:</span>
					<span>
						<span>{{ gatekeeperEnabled ? 'Yes' : 'No' }}</span>
						<span
							v-if="gatekeeperEnabled"
							class="pi pi-check-circle ml-1"
							style="color: var(--green-400); font-size: 0.8rem"
						></span>
						<span
							v-else
							class="pi pi-times-circle ml-1"
							style="color: var(--red-400); font-size: 0.8rem"
						></span>
					</span>
				</div>

				<div>
					<span class="step-option__header">Content Safety:</span>
					<span>{{ gatekeeperContentSafety.label }}</span>
				</div>

				<div>
					<span class="step-option__header">Data Protection:</span>
					<span>{{ gatekeeperDataProtection.label }}</span>
				</div>

				<template #edit>
					<div class="step-container__header">Gatekeeper</div>

					<div class="d-flex align-center mt-2">
						<span class="step-option__header">Enabled:</span>
						<span>
							<ToggleButton
								v-model="gatekeeperEnabled"
								onLabel="Yes"
								onIcon="pi pi-check-circle"
								offLabel="No"
								offIcon="pi pi-times-circle"
							/>
						</span>
					</div>

					<div class="mt-2">
						<span class="step-option__header">Content Safety:</span>
						<Dropdown
							v-model="gatekeeperContentSafety"
							class="dropdown--agent"
							:options="gatekeeperContentSafetyOptions"
							option-label="label"
							placeholder="--Select--"
						/>
					</div>

					<div class="mt-2">
						<span class="step-option__header">Data Protection:</span>
						<!-- <span>Microsoft Presidio</span> -->
						<Dropdown
							v-model="gatekeeperDataProtection"
							class="dropdown--agent"
							:options="gatekeeperDataProtectionOptions"
							option-label="label"
							placeholder="--Select--"
						/>
					</div>
				</template>
			</CreateAgentStepItem>

			<!-- System prompt -->
			<div class="step-section-header span-2">System Prompt</div>

			<div class="step-header">What is the persona of the agent?</div>

			<div class="span-2">
				<Textarea v-model="systemPrompt" class="w-100" auto-resize rows="5" type="text" />
			</div>

			<!-- Create agent -->
			<Button
				class="primary-button column-2 justify-self-end"
				style="width: 200px"
				label="Create Agent"
				@click="handleCreateAgent"
			/>
		</div>
	</div>
</template>

<script lang="ts">
import api from '@/js/api';
import type { CreateAgentRequest, AgentIndex } from '@/js/types';

const defaultSystemPrompt: string = 'You are an analytic agent named Khalil that helps people find information about FoundationaLLM. Provide concise answers that are polite and professional.';

export default {
	name: 'CreateAgent',

	data() {
		return {
			loading: false as boolean,
			loadingStatusText: 'Retrieving data...' as string,

			agentName: '',
			agentType: 'knowledge-management' as CreateAgentRequest['type'],

			editDataSource: false as boolean,
			dataSources: [],
			selectedDataSource: null as null | Object,

			editIndexSource: false as boolean,
			indexSources: [] as AgentIndex[],
			selectedIndexSource: null as null | AgentIndex,

			// editProcessing: false as boolean,
			chunkSize: 2000,
			overlapSize: 100,

			// editTrigger: false as boolean,
			triggerFrequency: { label: 'Manual', value: 1 },
			triggerFrequencyOptions: [
				{
					label: 'Manual',
					value: 1,
				},
				// {
				// 	label: 'Auto',
				// 	value: null,
				// },
				// {
				// 	label: 'Scheduled',
				// 	value: 2,
				// },
			],
			triggerFrequencyScheduled: null,
			triggerFrequencyScheduledOptions: [
				{
					label: 'Never',
					value: null,
				},
				{
					label: 'Every 30 minutes',
					value: 1,
				},
				{
					label: 'Hourly',
					value: 2,
				},
				{
					label: 'Every 12 hours',
					value: 2,
				},
				{
					label: 'Daily',
					value: 2,
				},
			],

			// editConversationHistory: false as boolean,
			conversationHistory: false as boolean,
			conversationMaxMessages: 5 as number,

			// editGatekeeper: false as boolean,
			gatekeeperEnabled: false as boolean,
			gatekeeperContentSafety: { label: 'None', value: null },
			gatekeeperContentSafetyOptions: [
				{
					label: 'None',
					value: null,
				},
				{
					label: 'Azure Content Safety',
					value: 1,
				},
			],
			gatekeeperDataProtection: { label: 'None', value: null },
			gatekeeperDataProtectionOptions: [
				{
					label: 'None',
					value: null,
				},
				{
					label: 'Microsoft Presidio',
					value: 1,
				},
			],

			systemPrompt: defaultSystemPrompt as string,
		};
	},

	computed: {
		groupedDataSources() {
			const grouped = {};
			this.dataSources.forEach(dataSource => {
				if (!grouped[dataSource.Type]) {
					grouped[dataSource.Type] = [];
				}

				grouped[dataSource.Type].push(dataSource);
			});
			
			return grouped;
		}
	},

	async created() {
		this.loading = true;

		// Uncomment to remove mock loading screen
		// api.mockLoadTime = 0;

		try {
			this.loadingStatusText = 'Retrieving indexes...';
			this.indexSources = await api.getAgentIndexes();

			this.loadingStatusText = 'Retrieving data sources...';
			this.dataSources = await api.getAgentDataSources();
		} catch(error) {
			this.$toast.add({
				severity: 'error',
				detail: error?.response?._data || error,
			});
		}

		this.loading = false;
	},

	methods: {
		handleNameInput(event) {
			let element = event.target;

			// Remove spaces
			let sanitizedValue = element.value.replace(/\s/g, '');

			// Remove any characters that are not lowercase letters, digits, dashes, or underscores
			sanitizedValue = sanitizedValue.replace(/[^a-z0-9-_]/g, '');

			element.value = sanitizedValue;
			this.agentName = sanitizedValue;
		},

		handleAgentTypeSelect(type: AgentType) {
			this.agentType = type;
		},

		handleDataSourceSelected(dataSource) {
			this.selectedDataSource = dataSource;
			this.editDataSource = false;
		},

		handleIndexSourceSelected(indexSource) {
			this.selectedIndexSource = indexSource;
			this.editIndexSource = false;
		},

		async handleCreateAgent() {
			const errors = [];
			if (!this.agentName) {
				errors.push('Please give the agent a name.');
			}

			if (!this.selectedDataSource) {
				errors.push('Please select a data source.');
			}

			if (!this.selectedIndexSource) {
				errors.push('Please select an index source.');
			}

			if (errors.length > 0) {
				this.$toast.add({
					severity: 'error',
					detail: errors.join('\n'),
					life: 5000,
				});

				return;
			}

			this.loading = true;
			this.loadingStatusText = 'Creating agent...';

			try {
				await api.createAgent({
					name: this.agentName,
					type: this.agentType,

					embedding_profile: this.selectedDataSource?.ObjectId,
					indexing_profile: this.selectedIndexSource?.ObjectId,

					// embedding_profile: string;
					// sessions_enabled: boolean;
					// orchestrator: string;

					conversation_history: {
						enabled: this.conversationHistory,
						// max_history: number,
					},

					gatekeeper: {
						use_system_setting: this.gatekeeperEnabled,
						options: {
							content_safety: this.gatekeeperContentSafety,
							data_protection: this.gatekeeperDataProtection,
						},
					},

					prompt: this.systemPrompt,
				});
			} catch(error) {
				this.$toast.add({
					severity: 'error',
					detail: 'There was an error creating the agent. Please check the settings and try again.',
					life: 5000,
				});
			}

			this.loading = false;
			// Route to created agent's page
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
	position: absolute;
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
	display: flex;
	flex-direction: column;
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
</style>
