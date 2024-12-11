<template>
	<main id="main-content">
		<div style="display: flex">
			<!-- Title -->
			<div style="flex: 1">
				<h2 class="page-header">{{ editAgent ? 'Edit Agent' : 'Create New Agent' }}</h2>
				<div class="page-subheader">
					{{
						editAgent
							? 'Edit your agent settings below.'
							: 'Complete the settings below to create and deploy your new agent.'
					}}
				</div>
			</div>

			<div style="display: flex; align-items: center">
				<!-- Private storage -->
				<PrivateStorage v-if="hasOpenAIAssistantCapability" :agent-name="agentName" />

				<!-- Edit access control -->
				<AccessControl
					v-if="editAgent"
					:scope="`providers/FoundationaLLM.Agent/agents/${agentName}`"
				/>
			</div>
		</div>

		<div class="steps" :class="{ 'steps--loading': loading }">
			<!-- Loading overlay -->
			<template v-if="loading">
				<div class="steps__loading-overlay" role="status" aria-live="polite">
					<LoadingGrid />
					<div>{{ loadingStatusText }}</div>
				</div>
			</template>

			<div class="span-2">
				<div id="aria-agent-name" class="step-header mb-2">Agent name:</div>
				<div id="aria-agent-name-desc" class="mb-2">
					No special characters or spaces, use letters and numbers with dashes and underscores only.
				</div>
				<div class="input-wrapper">
					<InputText
						v-model="agentName"
						:disabled="editAgent"
						type="text"
						class="w-100"
						placeholder="Enter agent name"
						aria-labelledby="aria-agent-name aria-agent-name-desc"
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
						class="icon invalid"
						:title="validationMessage"
						:aria-label="validationMessage"
					>
						❌
					</span>
				</div>
			</div>
			<div class="span-2">
				<div class="step-header mb-2">Description:</div>
				<div id="aria-description" class="mb-2">
					Provide a description to help others understand the agent's purpose.
				</div>
				<InputText
					v-model="agentDescription"
					type="text"
					class="w-100"
					placeholder="Enter agent description"
					aria-labelledby="aria-description"
				/>
			</div>
			<div class="span-2">
				<div class="step-header mb-2">Welcome message:</div>
				<div id="aria-welcome-message-desc" class="mb-2">
					Provide a message to display when a user starts a new conversation with the agent. If a
					message is not provided, the default welcome message will be displayed.
				</div>
				<CustomQuillEditor
					v-model="agentWelcomeMessage"
					:initial-content="JSON.parse(JSON.stringify(agentWelcomeMessage))"
					class="w-100"
					placeholder="Enter agent welcome message"
					aria-labelledby="aria-welcome-message-desc"
					@content-update="updateAgentWelcomeMessage($event)"
				/>
			</div>

			<!-- Knowledge source -->
			<section aria-labelledby="knowledge-source" class="span-2 steps">
				<h3 class="step-section-header span-2" id="knowledge-source">Knowledge Source</h3>

				<div id="aria-inline-context" class="step-header span-2">
					Does this agent have an inline context?
				</div>
				<div class="span-2">
					<div class="d-flex align-center mt-2">
						<span>
							<ToggleButton
								v-model="inline_context"
								on-label="Yes"
								on-icon="pi pi-check-circle"
								off-label="No"
								off-icon="pi pi-times-circle"
								aria-labelledby="aria-inline-context"
							/>
						</span>
					</div>
				</div>

				<template v-if="!inline_context">
					<div id="aria-dedicated-pipeline" class="step-header span-2">
						Do you want this agent to have a dedicated pipeline?
					</div>
					<div class="span-2">
						<div class="d-flex align-center mt-2">
							<span>
								<ToggleButton
									v-model="dedicated_pipeline"
									on-label="Yes"
									on-icon="pi pi-check-circle"
									off-label="No"
									off-icon="pi pi-times-circle"
									aria-labelledby="aria-dedicated-pipeline"
								/>
							</span>
						</div>
					</div>

					<template v-if="dedicated_pipeline">
						<div class="step-header">Where is the data?</div>
					</template>
					<template v-if="dedicated_pipeline">
						<div class="step-header">Where should the data be indexed?</div>
					</template>
					<template v-else>
						<div class="step-header">Select your index</div>
						<div class="step-header">Select the text embedding profile</div>
					</template>

					<!-- Data source -->
					<div v-if="dedicated_pipeline">
						<CreateAgentStepItem v-model="editDataSource" focusQuery=".step-container__edit__option">
							<template v-if="selectedDataSource">
								<div class="step-container__header">{{ selectedDataSource.type }}</div>
								<div>
									<div v-if="selectedDataSource.object_id !== ''">
										<span class="step-option__header">Name:</span>
									</div>
									<span>{{ selectedDataSource.name }}</span>
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
										:key="dataSource.name"
										class="step-container__edit__option"
										:class="{
											'step-container__edit__option--selected':
												dataSource.name === selectedDataSource?.name,
										}"
										tabindex="0"
										@click.stop="handleDataSourceSelected(dataSource)"
										@keydown.enter="handleDataSourceSelected(dataSource)"
									>
										<div>
											<div v-if="dataSource.object_id !== ''">
												<span class="step-option__header">Name:</span>
											</div>
											<span>{{ dataSource.name }}</span>
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
					</div>

					<!-- Index source -->
					<CreateAgentStepItem v-model="editIndexSource" focusQuery=".step-container__edit__option">
						<template v-if="selectedIndexSource">
							<div v-if="selectedIndexSource.object_id !== ''">
								<div class="step-container__header">{{ selectedIndexSource.name }}</div>
								<div>
									<span class="step-option__header">URL:</span>
									<span>{{ selectedIndexSource.resolved_configuration_references.Endpoint }}</span>
								</div>
								<div>
									<span class="step-option__header">Index Name:</span>
									<span>{{ selectedIndexSource.settings.IndexName }}</span>
								</div>
							</div>
							<div v-else>
								<div class="step-container__header">DEFAULT</div>
								{{ selectedIndexSource.name }}
							</div>
						</template>
						<template v-else>Please select an index source.</template>

						<template #edit>
							<div class="step-container__edit__header">Please select an index source.</div>
							<div
								v-for="indexSource in indexSources"
								:key="indexSource.name"
								class="step-container__edit__option"
								:class="{
									'step-container__edit__option--selected':
										indexSource.name === selectedIndexSource?.name,
								}"
								tabindex="0"
								@click.stop="handleIndexSourceSelected(indexSource)"
								@keydown.enter="handleIndexSourceSelected(indexSource)"
							>
								<div v-if="indexSource.object_id !== ''">
									<div class="step-container__header">{{ indexSource.name }}</div>
									<div v-if="indexSource.resolved_configuration_references.Endpoint">
										<span class="step-option__header">URL:</span>
										<span>{{ indexSource.resolved_configuration_references.Endpoint }}</span>
									</div>
									<div v-if="indexSource.settings.IndexName">
										<span class="step-option__header">Index Name:</span>
										<span>{{ indexSource.settings.IndexName }}</span>
									</div>
								</div>
								<div v-else>
									<div class="step-container__header">DEFAULT</div>
									{{ indexSource.name }}
								</div>
							</div>
						</template>
					</CreateAgentStepItem>

					<template v-if="dedicated_pipeline">
						<div class="step-header">Select the text embedding profile</div>
						<div class="step-header"></div>
					</template>

					<!-- Text embedding profiles -->
					<CreateAgentStepItem v-model="editTextEmbeddingProfile" focusQuery=".step-container__edit__option">
						<template v-if="selectedTextEmbeddingProfile">
							<div v-if="selectedTextEmbeddingProfile.object_id !== ''">
								<div class="step-container__header">{{ selectedTextEmbeddingProfile.name }}</div>
								<div v-if="selectedTextEmbeddingProfile.resolved_configuration_references?.Endpoint">
									<span class="step-option__header">URL:</span>
									<span>{{
										selectedTextEmbeddingProfile.resolved_configuration_references.Endpoint
									}}</span
									><br />
									<span class="step-option__header">Deployment:</span>
									<span>{{
										selectedTextEmbeddingProfile.resolved_configuration_references.DeploymentName
									}}</span>
								</div>
								<div v-if="selectedTextEmbeddingProfile.settings?.model_name">
									<span class="step-option__header">Model Name:</span>
									<span>{{ selectedTextEmbeddingProfile.settings.model_name }}</span>
								</div>
							</div>
							<div v-else>
								<div class="step-container__header">DEFAULT</div>
								{{ selectedTextEmbeddingProfile.name }}
							</div>
						</template>
						<template v-else>Please select text embedding profile.</template>

						<template #edit>
							<div class="step-container__edit__header">Please select text embedding profile.</div>
							<div
								v-for="textEmbeddingProfile in textEmbeddingProfileSources"
								:key="textEmbeddingProfile.name"
								class="step-container__edit__option"
								:class="{
									'step-container__edit__option--selected':
										textEmbeddingProfile.name === selectedTextEmbeddingProfile?.name,
								}"
								tabindex="0"
								@click.stop="handleTextEmbeddingProfileSelected(textEmbeddingProfile)"
								@keydown.enter="handleTextEmbeddingProfileSelected(textEmbeddingProfile)"
							>
								<div v-if="textEmbeddingProfile.object_id !== ''">
									<div class="step-container__header">{{ textEmbeddingProfile.name }}</div>
									<div v-if="textEmbeddingProfile.resolved_configuration_references?.Endpoint">
										<span class="step-option__header">URL:</span>
										<span>{{ textEmbeddingProfile.resolved_configuration_references.Endpoint }}</span
										><br />
										<span class="step-option__header">Deployment:</span>
										<span>{{
											textEmbeddingProfile.resolved_configuration_references.DeploymentName
										}}</span>
									</div>
									<div v-if="textEmbeddingProfile.settings?.model_name">
										<span class="step-option__header">Model Name:</span>
										<span>{{ textEmbeddingProfile.settings.model_name }}</span>
									</div>
								</div>
								<div v-else>
									<div class="step-container__header">DEFAULT</div>
									{{ textEmbeddingProfile.name }}
								</div>
							</div>
						</template>
					</CreateAgentStepItem>
					<div></div>

					<template v-if="dedicated_pipeline">
						<div class="step-header">How should the data be processed for indexing?</div>
						<div class="step-header">When should the data be indexed?</div>

						<!-- Process indexing -->

						<CreateAgentStepItem focusQuery=".chunk-size-input">
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
									<span id="aria-chunk-size" class="step-option__header">Chunk size:</span>
									<InputText
										v-model="chunkSize"
										type="number"
										class="mt-2 chunk-size-input"
										aria-label="aria-chunk-size"
									/>
								</div>

								<div>
									<span id="aria-overlap-size" class="step-option__header">Overlap size:</span>
									<InputText
										v-model="overlapSize"
										type="number"
										class="mt-2"
										aria-label="aria-overlap-size"
									/>
								</div>
							</template>
						</CreateAgentStepItem>

						<!-- Trigger -->
						<CreateAgentStepItem focusQuery=".frequency-dropdown span">
							<div class="step-container__header">Trigger</div>
							<div>Runs every time a new item is added to the data source.</div>

							<div class="mt-2">
								<span class="step-option__header">Frequency:</span>
								<span>{{ triggerFrequency }}</span>
							</div>

							<div v-if="triggerFrequency === 'Schedule' && triggerFrequencyScheduled">
								<span class="step-option__header">Schedule:</span>
								<span>{{ triggerFrequencyScheduled }}</span>
							</div>

							<template #edit>
								<div class="step-container__header">Trigger</div>
								<div>Runs every time a new item is added to the data source.</div>

								<div class="mt-2">
									<span id="aria-frequency" class="step-option__header">Frequency:</span>
									<Dropdown
										v-model="triggerFrequency"
										class="dropdown--agent frequency-dropdown"
										:options="triggerFrequencyOptions"
										placeholder="--Select--"
										aria-label="aria-frequency"
									/>
								</div>

								<div v-if="triggerFrequency === 'Schedule'" class="mt-2">
									<CronLight
										v-model="triggerFrequencyScheduled"
										format="quartz"
										@error="error = $event"
									/>
									<!-- editable cron expression -->
									<InputText
										class="mt-4"
										label="cron expression"
										:model-value="triggerFrequencyScheduled"
										:error-messages="error"
										aria-label="cron expression"
										@update:model-value="triggerFrequencyNextScheduled = $event"
										@blur="triggerFrequencyScheduled = triggerFrequencyNextScheduled"
									/>
								</div>
							</template>
						</CreateAgentStepItem>
					</template>
				</template>
			</section>
			<!-- End of Knowledge Source -->

			<!-- Agent configuration -->
			<section aria-labelledby="agent-configuration" class="span-2 steps">
				<h3 class="step-section-header span-2" id="agent-configuration">Agent Configuration</h3>

				<div class="step-header">Should conversations be included in the context?</div>
				<div class="step-header">How should user-agent interactions be gated?</div>

				<!-- Conversation history -->
				<CreateAgentStepItem focusQuery=".conversation-history-toggle input">
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
						<div id="aria-conversation-history" class="step-container__header">
							Conversation History
						</div>

						<div class="d-flex align-center mt-2">
							<span id="aria-conversation-history-enabled" class="step-option__header">Enabled:</span>
							<span>
								<ToggleButton
									v-model="conversationHistory"
									on-label="Yes"
									on-icon="pi pi-check-circle"
									off-label="No"
									off-icon="pi pi-times-circle"
									aria-labelledby="aria-conversation-history aria-conversation-history-enabled"
									class="conversation-history-toggle"
								/>
							</span>
						</div>

						<div>
							<span id="aria-max-messages" class="step-option__header">Max Messages:</span>
							<InputText
								v-model="conversationMaxMessages"
								type="number"
								class="mt-2"
								aria-label="aria-max-messages"
							/>
						</div>
					</template>
				</CreateAgentStepItem>

				<!-- Gatekeeper -->
				<CreateAgentStepItem focusQuery=".gatekeeper-toggle input">
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
						<span>{{
							Array.isArray(selectedGatekeeperContentSafety)
								? selectedGatekeeperContentSafety.map((item) => item.name).join(', ')
								: ''
						}}</span>
					</div>

					<div>
						<span class="step-option__header">Data Protection:</span>
						<span>{{
							Array.isArray(selectedGatekeeperDataProtection)
								? selectedGatekeeperDataProtection.map((item) => item.name).join(', ')
								: ''
						}}</span>
					</div>

					<template #edit>
						<div id="aria-gatekeeper" class="step-container__header">Gatekeeper</div>

						<!-- Gatekeeper toggle -->
						<div class="d-flex align-center mt-2">
							<span id="aria-gatekeeper-enabled" class="step-option__header">Enabled:</span>
							<span>
								<ToggleButton
									v-model="gatekeeperEnabled"
									on-label="Yes"
									on-icon="pi pi-check-circle"
									off-label="No"
									off-icon="pi pi-times-circle"
									aria-labelledby="aria-gatekeeper aria-gatekeeper-enabled"
									class="gatekeeper-toggle"
								/>
							</span>
						</div>

						<!-- Content safety -->
						<div class="mt-2">
							<span id="aria-content-safety" class="step-option__header">Content Safety:</span>
							<MultiSelect
								v-model="selectedGatekeeperContentSafety"
								class="dropdown--agent"
								:options="gatekeeperContentSafetyOptions"
								option-label="name"
								display="chip"
								placeholder="--Select--"
								aria-labelledby="aria-content-safety"
							/>
						</div>

						<!-- Data protection -->
						<div class="mt-2">
							<span id="aria-data-prot" class="step-option__header">Data Protection:</span>
							<!-- <span>Microsoft Presidio</span> -->
							<MultiSelect
								v-model="selectedGatekeeperDataProtection"
								class="dropdown--agent"
								:options="gatekeeperDataProtectionOptions"
								option-label="name"
								display="chip"
								placeholder="--Select--"
								aria-labelledby="aria-data-prot"
							/>
						</div>
					</template>
				</CreateAgentStepItem>

				<!-- Orchestrator -->
				<div id="aria-orchestrator" class="step-header span-2">
					How should the agent communicate with the AI model?
				</div>
				<div class="span-2">
					<Dropdown
						v-model="orchestration_settings.orchestrator"
						:options="orchestratorOptions"
						option-label="label"
						option-value="value"
						class="dropdown--agent"
						placeholder="--Select--"
						aria-labelledby="aria-orchestrator"
					/>
				</div>

				<div class="step-header">Which AI model should the orchestrator use?</div>
				<div class="step-header">Which capabilities should the agent have?</div>

				<!-- AI model -->
				<CreateAgentStepItem v-model="editAIModel" focusQuery=".step-container__edit__option">
					<template v-if="selectedAIModel">
						<div v-if="selectedAIModel.object_id !== ''">
							<div class="step-container__header">{{ selectedAIModel.name }}</div>
							<div>
								<span class="step-option__header">Deployment name:</span>
								<span>{{ selectedAIModel.deployment_name }}</span>
							</div>
						</div>
					</template>
					<template v-else>Please select an AI model.</template>

					<template #edit>
						<div class="step-container__edit__header">Please select an AI model.</div>
						<div
							v-for="aiModel in aiModelOptions"
							:key="aiModel.name"
							class="step-container__edit__option"
							:class="{
								'step-container__edit__option--selected': aiModel.name === selectedAIModel?.name,
							}"
							tabindex="0"
							@click.stop="handleAIModelSelected(aiModel)"
							@keydown.enter="handleAIModelSelected(aiModel)"
						>
							<div v-if="aiModel.object_id !== ''">
								<div class="step-container__header">{{ aiModel.name }}</div>
								<div v-if="aiModel.deployment_name">
									<span class="step-option__header">Deployment name:</span>
									<span>{{ aiModel.deployment_name }}</span>
								</div>
							</div>
						</div>
					</template>
				</CreateAgentStepItem>

				<!-- Agent capabilities -->
				<CreateAgentStepItem focusQuery=".agent-capabilities-dropdown input">
					<div>
						<span class="step-option__header">Agent Capabilities:</span>
						<span>{{
							Array.isArray(selectedAgentCapabilities)
								? selectedAgentCapabilities.map((item) => item.name).join(', ')
								: ''
						}}</span>
					</div>

					<template #edit>
						<div class="mt-2">
							<span class="step-option__header">Agent Capabilities:</span>
							<MultiSelect
								v-model="selectedAgentCapabilities"
								class="dropdown--agent agent-capabilities-dropdown"
								:options="agentCapabilitiesOptions"
								option-label="name"
								display="chip"
								placeholder="--Select--"
								aria-labelledby="aria-content-safety"
							/>
						</div>
					</template>
				</CreateAgentStepItem>

				<!-- Cost center -->
				<div id="aria-cost-center" class="step-header span-2">
					Would you like to assign this agent to a cost center?
				</div>
				<div class="span-2">
					<InputText
						v-model="cost_center"
						type="text"
						class="w-50"
						placeholder="Enter cost center name"
						aria-labelledby="aria-cost-center"
					/>
				</div>

				<!-- Expiration -->
				<div class="step-header span-2">Would you like to set an expiration on this agent?</div>
				<div class="span-2">
					<Calendar
						v-model="expirationDate"
						show-icon
						show-button-bar
						placeholder="Enter expiration date"
						type="text"
					/>
				</div>
			</section>

			<!-- System prompt -->
			<section aria-labelledby="system-prompt" class="span-2 steps">
				<h3 class="step-section-header span-2" id="system-prompt">System Prompt</h3>

				<div id="aria-persona" class="step-header">What is the persona of the agent?</div>

				<div class="span-2">
					<Textarea
						v-model="systemPrompt"
						class="w-100"
						auto-resize
						rows="5"
						type="text"
						placeholder="You are an analytic agent named Khalil that helps people find information about FoundationaLLM. Provide concise answers that are polite and professional."
						aria-labelledby="aria-persona"
					/>
				</div>
			</section>
			
			<!-- Security -->
			<div v-if="virtualSecurityGroupId" class="step-section-header span-2">Security</div>

			<!-- Virtual security group id -->
			<template v-if="virtualSecurityGroupId">
				<div class="step-header">Virtual security group ID</div>
				<div class="span-2" style="display: flex; gap: 16px">
					<InputText
						:value="virtualSecurityGroupId"
						disabled
						type="text"
						class="w-50"
						placeholder="Enter cost center name"
						aria-labelledby="aria-cost-center"
					/>
					<Button
						label="Copy"
						severity="primary"
						@click="handleCopySecurityGroupId"
					/>
				</div>
			</template>

			<!-- Access tokens -->
			<template v-if="virtualSecurityGroupId">
				<div class="step-header">Agent access tokens</div>
				<div class="span-2">
					<AgentAccessTokens :agent-name="this.agentName" />
				</div>
			</template>
			
			
			<div class="span-2 d-flex justify-content-end" style="gap: 16px">
				<!-- Create agent -->
				<Button
					:label="editAgent ? 'Save Changes' : 'Create Agent'"
					severity="primary"
					:disabled="editable === false"
					@click="handleCreateAgent"
				/>

				<!-- Cancel -->
				<Button
					v-if="editAgent"
					label="Cancel"
					severity="secondary"
					@click="handleCancel"
				/>
			</div>
		</div>
	</main>
</template>

<script lang="ts">
import '@vue-js-cron/light/dist/light.css';
import type { PropType } from 'vue';
import { ref } from 'vue';
import { debounce } from 'lodash';
import { CronLight } from '@vue-js-cron/light';
import api from '@/js/api';
import type {
	Agent,
	AgentIndex,
	AgentDataSource,
	AgentTool,
	AIModel,
	DataSource,
	CreateAgentRequest,
	ExternalOrchestrationService,
	TextEmbeddingProfile,
	// AgentCheckNameResponse,
} from '@/js/types';

const defaultSystemPrompt: string = '';

const getDefaultFormValues = () => {
	return {
		accessControlModalOpen: false,

		agentName: '',
		agentDescription: '',
		agentWelcomeMessage: '',
		object_id: '',
		text_partitioning_profile_object_id: '',
		text_embedding_profile_object_id: '',
		vectorization_data_pipeline_object_id: '',
		prompt_object_id: '',
		dedicated_pipeline: true,
		inline_context: false,
		agentType: 'knowledge-management' as CreateAgentRequest['type'],

		cost_center: '',
		expirationDate: null as string | null,

		editDataSource: false as boolean,
		selectedDataSource: null as null | AgentDataSource,

		editIndexSource: false as boolean,
		selectedIndexSource: null as null | AgentIndex,

		editTextEmbeddingProfile: false as boolean,
		selectedTextEmbeddingProfile: null as null | TextEmbeddingProfile,

		editAIModel: false as boolean,
		selectedAIModel: null as null | AIModel,

		chunkSize: 500,
		overlapSize: 50,

		triggerFrequency: 'Event' as string,
		triggerFrequencyScheduled: '* * * * *' as string,
		triggerFrequencyNextScheduled: '' as string,
		error: '',

		conversationHistory: false as boolean,
		conversationMaxMessages: 5 as number,

		gatekeeperEnabled: false as boolean,

		selectedGatekeeperContentSafety: ref(),
		selectedGatekeeperDataProtection: ref(),
		gatekeeperContentSafety: { label: 'None', value: null },
		gatekeeperDataProtection: { label: 'None', value: null },

		selectedAgentCapabilities: ref(),
		agentCapabilities: { label: 'None', value: null },

		systemPrompt: defaultSystemPrompt as string,

		orchestration_settings: {
			orchestrator: 'LangChain' as string,
		},
	};
};

export default {
	name: 'CreateAgent',

	components: {
		CronLight,
	},

	props: {
		editAgent: {
			type: [Boolean, String] as PropType<false | string>,
			required: false,
			default: false,
		},
	},

	data() {
		return {
			...getDefaultFormValues(),

			loading: false as boolean,
			loadingStatusText: 'Retrieving data...' as string,

			editable: false as boolean,
			hasOpenAIAssistantCapability: false as boolean,

			nameValidationStatus: null as string | null, // 'valid', 'invalid', or null
			validationMessage: '' as string,

			dataSources: [] as DataSource[],
			indexSources: [] as AgentIndex[],
			textEmbeddingProfileSources: [] as TextEmbeddingProfile[],
			externalOrchestratorOptions: [] as ExternalOrchestrationService[],
			aiModelOptions: [] as AIModel[],
			tools: [] as AgentTool[],
            workflowOptions: [],

			virtualSecurityGroupId: null as string | null,

			orchestratorOptions: [
				{
					label: 'LangChain',
					value: 'LangChain',
				},
				{
					label: 'AzureOpenAIDirect',
					value: 'AzureOpenAIDirect',
				},
				{
					label: 'AzureAIDirect',
					value: 'AzureAIDirect',
				},
				{
					label: 'SemanticKernel',
					value: 'SemanticKernel',
				},
			],

			triggerFrequencyOptions: ['Event', 'Manual', 'Schedule'],

			triggerFrequencyScheduledOptions: [
				'Never',
				'Every 30 minutes',
				'Hourly',
				'Every 12 hours',
				'Daily',
			],

			gatekeeperContentSafetyOptions: ref([
				{
					name: 'Azure Content Safety',
					code: 'AzureContentSafety',
				},
				{
					name: 'Azure Content Safety Prompt Shield',
					code: 'AzureContentSafetyPromptShield',
				},
				{
					name: 'Lakera Guard',
					code: 'LakeraGuard',
				},
				{
					name: 'Enkrypt Guardrails',
					code: 'EnkryptGuardrails',
				},
			]),

			gatekeeperDataProtectionOptions: ref([
				{
					name: 'Microsoft Presidio',
					code: 'MicrosoftPresidio',
				},
			]),

			agentCapabilitiesOptions: ref([
				{
					name: 'OpenAI Assistants',
					code: 'OpenAI.Assistants',
				},
				{
					name: 'FLLM Knowledge Management',
					code: 'FoundationaLLM.KnowledgeManagement',
				},
			]),
		};
	},

	computed: {
		groupedDataSources() {
			const grouped = {};
			this.dataSources.forEach((dataSource) => {
				if (!grouped[dataSource.type]) {
					grouped[dataSource.type] = [];
				}

				grouped[dataSource.type].push(dataSource);
			});

			return grouped;
		},
	},

	async created() {
		this.loading = true;
		// Uncomment to remove mock loading screen
		// api.mockLoadTime = 0;

		try {
			this.loadingStatusText = 'Retrieving indexes...';
			const indexSourcesResult = await api.getAgentIndexes(true);
			this.indexSources = indexSourcesResult.map((result) => result.resource);

			this.loadingStatusText = 'Retrieving text embedding profiles...';
			const embeddingProfileSourcesResult = await api.getTextEmbeddingProfiles();
			this.textEmbeddingProfileSources = embeddingProfileSourcesResult.map(
				(result) => result.resource,
			);

			this.loadingStatusText = 'Retrieving data sources...';
			const agentDataSourcesResult = await api.getAgentDataSources(true);
			this.dataSources = agentDataSourcesResult.map((result) => result.resource);

			this.loadingStatusText = 'Retrieving external orchestration services...';
			const externalOrchestrationServicesResult = await api.getExternalOrchestrationServices();
			this.externalOrchestratorOptions = externalOrchestrationServicesResult.map(
				(result) => result.resource,
			) as ExternalOrchestrationService[];
			this.externalOrchestratorOptions = this.externalOrchestratorOptions.filter(
				(service) => service.category === 'ExternalOrchestration',
			);

			this.loadingStatusText = 'Retrieving AI models...';
			const aiModelsResult = await api.getAIModels();
			this.aiModelOptions = aiModelsResult.map((result) => result.resource);
			// Filter the AIModels so we only display the ones where the type is 'completion'.
			this.aiModelOptions = this.aiModelOptions.filter((model) => model.type === 'completion');

			this.loadingStatusText = 'Retrieving workflows...';
			const workflowOptions = await api.getAgentWorkflows();
			this.workflowOptions = workflowOptions.map((result) => result.resource);

			// Update the orchestratorOptions with the externalOrchestratorOptions.
			this.orchestratorOptions = this.orchestratorOptions.concat(
				this.externalOrchestratorOptions.map((service) => ({
					label: service.name,
					value: service.name,
				})),
			);
		} catch (error) {
			this.$toast.add({
				severity: 'error',
				detail: error?.response?._data || error,
				life: 5000,
			});
		}

		if (this.editAgent) {
			this.loadingStatusText = `Retrieving agent "${this.editAgent}"...`;
			const agentGetResult = await api.getAgent(this.editAgent);
			this.editable = agentGetResult.actions.includes('FoundationaLLM.Agent/agents/write');

			const agent = agentGetResult.resource;
			this.virtualSecurityGroupId = agent.virtual_security_group_id;

			if (agent.vectorization && agent.vectorization.text_partitioning_profile_object_id) {
				this.loadingStatusText = `Retrieving text partitioning profile...`;
				const textPartitioningProfile = await api.getTextPartitioningProfile(
					agent.vectorization.text_partitioning_profile_object_id,
				);
				if (textPartitioningProfile && textPartitioningProfile.resource) {
					this.chunkSize = Number(textPartitioningProfile.resource.settings.ChunkSizeTokens);
					this.overlapSize = Number(textPartitioningProfile.resource.settings.OverlapSizeTokens);
				}
			}
			if (agent.prompt_object_id !== '') {
				this.loadingStatusText = `Retrieving prompt...`;
				const prompt = await api.getPrompt(agent.prompt_object_id);
				if (prompt && prompt.resource) {
					this.systemPrompt = prompt.resource.prefix;
				}
			}
			this.loadingStatusText = `Mapping agent values to form...`;
			this.mapAgentToForm(agent);
			this.tools = agent.tools;
		} else {
			this.editable = true;
		}

		this.debouncedCheckName = debounce(this.checkName, 500);

		this.loading = false;
	},

	methods: {
		mapAgentToForm(agent: Agent) {
			this.agentName = agent.name || this.agentName;
			this.agentDescription = agent.description || this.agentDescription;
			this.agentWelcomeMessage = agent.properties?.welcome_message || this.agentWelcomeMessage;
			this.agentType = agent.type || this.agentType;
			this.object_id = agent.object_id || this.object_id;
			this.inline_context = agent.inline_context || this.inline_context;
			this.cost_center = agent.cost_center || this.cost_center;
			this.hasOpenAIAssistantCapability = agent.capabilities?.includes('OpenAI.Assistants');
			this.expirationDate = agent.expiration_date
				? new Date(agent.expiration_date)
				: this.expirationDate;

			this.orchestration_settings.orchestrator =
				agent.orchestration_settings?.orchestrator || this.orchestration_settings.orchestrator;

			if (agent.vectorization) {
				this.dedicated_pipeline = agent.vectorization.dedicated_pipeline;
			}
			this.text_embedding_profile_object_id =
				agent.vectorization?.text_embedding_profile_object_id ||
				this.text_embedding_profile_object_id;

			this.triggerFrequency = agent.vectorization?.trigger_type || this.triggerFrequency;
			this.triggerFrequencyScheduled =
				agent.vectorization?.trigger_cron_schedule || this.triggerFrequencyScheduled;

			this.selectedIndexSource =
				this.indexSources.find(
					(indexSource) =>
						indexSource.object_id &&
						agent.vectorization?.indexing_profile_object_ids?.includes(indexSource.object_id),
				) || null;

			this.selectedTextEmbeddingProfile =
				this.textEmbeddingProfileSources.find(
					(textEmbeddingProfile) =>
						textEmbeddingProfile.object_id ===
						agent.vectorization?.text_embedding_profile_object_id,
				) || null;

			this.selectedDataSource =
				this.dataSources.find(
					(dataSource) => dataSource.object_id === agent.vectorization?.data_source_object_id,
				) || null;

			this.selectedAIModel =
				this.aiModelOptions.find((aiModel) => aiModel.object_id === agent.ai_model_object_id) ||
				null;

			this.conversationHistory =
				agent.conversation_history_settings?.enabled || this.conversationHistory;
			this.conversationMaxMessages =
				agent.conversation_history_settings?.max_history || this.conversationMaxMessages;

			this.gatekeeperEnabled = Boolean(agent.gatekeeper_settings?.use_system_setting);

			if (agent.gatekeeper_settings && agent.gatekeeper_settings.options) {
				this.selectedGatekeeperContentSafety =
					this.gatekeeperContentSafetyOptions.filter((localOption) =>
						agent.gatekeeper_settings?.options?.includes(localOption.code),
					) || this.selectedGatekeeperContentSafety;

				this.selectedGatekeeperDataProtection =
					this.gatekeeperDataProtectionOptions.filter((localOption) =>
						agent.gatekeeper_settings?.options?.includes(localOption.code),
					) || this.selectedGatekeeperDataProtection;
			}

			if (agent.capabilities) {
				this.selectedAgentCapabilities =
					this.agentCapabilitiesOptions.filter((localOption) =>
						agent.capabilities?.includes(localOption.code),
					) || this.selectedAgentCapabilities;
			}
		},

		updateAgentWelcomeMessage(newContent: string) {
			this.agentWelcomeMessage = newContent;
		},

		async checkName() {
			try {
				const response = await api.checkAgentName(this.agentName, this.agentType);

				// Handle response based on the status
				if (response.status === 'Allowed') {
					// Name is available
					this.nameValidationStatus = 'valid';
					this.validationMessage = null;
				} else if (response.status === 'Denied') {
					// Name is taken
					this.nameValidationStatus = 'invalid';
					this.validationMessage = response.message;
					// this.$toast.add({
					// 	severity: 'warn',
					// 	detail: `Agent name "${this.agentName}" is already taken for the selected ${response.type} agent type. Please choose another name.`,
					// life: 5000,
					// });
				}
			} catch (error) {
				console.error('Error checking agent name: ', error);
				this.nameValidationStatus = 'invalid';
				this.validationMessage = 'Error checking the agent name. Please try again.';
			}
		},

		resetForm() {
			const defaultFormValues = getDefaultFormValues();
			for (const key in defaultFormValues) {
				this[key] = defaultFormValues[key];
			}
		},

		handleCancel() {
			if (!confirm('Are you sure you want to cancel?')) {
				return;
			}
			this.$router.push('/agents/public');
		},

		handleNameInput(event) {
			const sanitizedValue = this.$filters.sanitizeNameInput(event);
			this.agentName = sanitizedValue;

			// Check if the name is available if we are creating a new agent.
			if (!this.editAgent) {
				this.debouncedCheckName();
			}
		},

		handleAgentTypeSelect(type: Agent['type']) {
			this.agentType = type;
		},

		handleDataSourceSelected(dataSource: AgentDataSource) {
			this.selectedDataSource = dataSource;
			this.editDataSource = false;
		},

		handleIndexSourceSelected(indexSource: AgentIndex) {
			this.selectedIndexSource = indexSource;
			this.editIndexSource = false;
		},

		handleTextEmbeddingProfileSelected(textEmbeddingProfile: TextEmbeddingProfile) {
			this.selectedTextEmbeddingProfile = textEmbeddingProfile;
			this.editTextEmbeddingProfile = false;
		},

		handleAIModelSelected(aiModel: AIModel) {
			this.selectedAIModel = aiModel;
			this.editAIModel = false;
		},

		handleCopySecurityGroupId() {
			if (this.virtualSecurityGroupId) {
				navigator.clipboard.writeText(this.virtualSecurityGroupId);
				this.$toast.add({
					severity: 'success',
					detail: 'Virtual Security Group ID copied to clipboard!',
					life: 5000,
				});
			}
		},

		async handleCreateAgent() {
			const errors = [];
			if (!this.agentName) {
				errors.push('Please give the agent a name.');
			}
			if (this.nameValidationStatus === 'invalid') {
				errors.push(this.validationMessage);
			}

			if (!this.inline_context && !this.selectedTextEmbeddingProfile) {
				errors.push('Please select a text embedding profile.');
			} else {
				this.text_embedding_profile_object_id = this.selectedTextEmbeddingProfile?.object_id ?? '';
			}

			if (this.systemPrompt === '') {
				errors.push('Please provide a system prompt.');
			}

			if (!this.orchestration_settings.orchestrator) {
				errors.push('Please select an orchestrator.');
			}

			if (!this.selectedAIModel) {
				errors.push('Please select an AI model for the orchestrator.');
			}

			// if (!this.selectedDataSource) {
			// 	errors.push('Please select a data source.');
			// }

			// if (!this.selectedIndexSource) {
			// 	errors.push('Please select an index source.');
			// }

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

			const promptRequest = {
				type: 'multipart',
				name: this.agentName,
				cost_center: this.cost_center,
				description: `System prompt for the ${this.agentName} agent`,
				prefix: this.systemPrompt,
				suffix: '',
			};

			const tokenTextPartitionRequest = {
				text_splitter: 'TokenTextSplitter',
				name: this.agentName,
				settings: {
					Tokenizer: 'MicrosoftBPETokenizer',
					TokenizerEncoder: 'cl100k_base',
					ChunkSizeTokens: this.chunkSize.toString(),
					OverlapSizeTokens: this.overlapSize.toString(),
				},
			};

			let successMessage = null;
			try {
				// Handle Prompt creation/update.
				let promptObjectId = '';
				if (promptRequest.prefix !== '') {
					const promptResponse = await api.createOrUpdatePrompt(this.agentName, promptRequest);
					promptObjectId = promptResponse.objectId;
				}

				let textPartitioningProfileObjectId = '';
				let dataSourceObjectId = '';
				let indexingProfileObjectId = [''];

				if (!this.inline_context) {
					// Handle TextPartitioningProfile creation/update.
					const tokenTextPartitionResponse = await api.createOrUpdateTextPartitioningProfile(
						this.agentName,
						tokenTextPartitionRequest,
					);
					textPartitioningProfileObjectId = tokenTextPartitionResponse.objectId;

					// Select the default data source, if any.
					dataSourceObjectId = this.selectedDataSource?.object_id ?? '';
					if (dataSourceObjectId === '' && this.dedicated_pipeline) {
						const defaultDataSource = await api.getDefaultDataSource();
						if (defaultDataSource !== null) {
							dataSourceObjectId = defaultDataSource.object_id;
						}
					}

					// Select the default indexing profile, if any.
					indexingProfileObjectId = [this.selectedIndexSource?.object_id ?? ''];
					if (indexingProfileObjectId.length === 0) {
						const defaultAgentIndex = await api.getDefaultAgentIndex();
						if (defaultAgentIndex !== null) {
							indexingProfileObjectId = [defaultAgentIndex.object_id];
						}
					}
				}

				const agentRequest: CreateAgentRequest = {
					type: this.agentType,
					name: this.agentName,
					description: this.agentDescription,
					properties: { welcome_message: this.agentWelcomeMessage },
					object_id: this.object_id,
					inline_context: this.inline_context,
					cost_center: this.cost_center,
					expiration_date: this.expirationDate?.toISOString(),

					vectorization: {
						dedicated_pipeline: this.dedicated_pipeline,
						text_embedding_profile_object_id: this.text_embedding_profile_object_id,
						indexing_profile_object_ids: indexingProfileObjectId,
						text_partitioning_profile_object_id: textPartitioningProfileObjectId,
						data_source_object_id: dataSourceObjectId,
						vectorization_data_pipeline_object_id: this.vectorization_data_pipeline_object_id,
						trigger_type: this.triggerFrequency,
						trigger_cron_schedule: this.triggerFrequencyScheduled,
					},

					conversation_history_settings: {
						enabled: this.conversationHistory,
						max_history: Number(this.conversationMaxMessages),
					},

					gatekeeper_settings: {
						use_system_setting: this.gatekeeperEnabled,
						options: [
							...(this.selectedGatekeeperContentSafety || []).map((option: any) => option.code),
							...(this.selectedGatekeeperDataProtection || []).map((option: any) => option.code),
						].filter((option) => option !== null),
					},

					capabilities: (this.selectedAgentCapabilities || []).map((option: any) => option.code),

					sessions_enabled: true,

					prompt_object_id: promptObjectId,
					orchestration_settings: this.orchestration_settings,
					ai_model_object_id: this.selectedAIModel.object_id,

					tools: this.tools,
				};

				if (this.editAgent) {
					await api.upsertAgent(this.editAgent, agentRequest);
					successMessage = `Agent "${this.agentName}" was successfully updated!`;
				} else {
					await api.upsertAgent(agentRequest.name, agentRequest);
					successMessage = `Agent "${this.agentName}" was successfully created!`;
					this.resetForm();
				}
			} catch (error) {
				this.loading = false;
				return this.$toast.add({
					severity: 'error',
					detail: error?.response?._data || error,
					life: 5000,
				});
			}

			this.$toast.add({
				severity: 'success',
				detail: successMessage,
				life: 5000,
			});

			this.loading = false;

			if (!this.editAgent) {
				this.$router.push('/agents/public');
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
	margin: 0px;
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

// .p-button-icon {
// 	color: var(--primary-button-text) !important;
// }

.valid {
	color: green;
}

.invalid {
	color: red;
}

.virtual-security-group-id {
	margin: 0 1rem 0 0;
	width: auto;
}
</style>
