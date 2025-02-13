<template>
	<div>
		<!-- Header -->
		<h2 class="page-header">{{ editId ? 'Edit Model' : 'Create Model' }}</h2>
		<div class="page-subheader">
			{{
				editId
					? 'Edit your model settings below.'
					: 'Complete the settings below to configure the model.'
			}}
		</div>

		<!-- Steps -->
		<div class="steps" :class="{ 'steps--loading': loading }">
			<!-- Loading overlay -->
			<template v-if="loading">
				<div class="steps__loading-overlay">
					<LoadingGrid />
					<div>{{ loadingStatusText }}</div>
				</div>
			</template>

			<!-- Name -->
			<div class="step-header span-2">What is the model name?</div>
			<div class="span-2">
				<div id="aria-source-name-desc" class="mb-2">
					No special characters or spaces, use letters and numbers with dashes and underscores only.
				</div>
				<div class="input-wrapper">
					<InputText
						v-model="aiModel.name"
						:disabled="editId"
						type="text"
						class="w-100"
						placeholder="Enter model name"
						aria-labelledby="aria-source-name aria-source-name-desc"
						@input="handleNameInput"
					/>
					<span
						v-if="nameValidationStatus === 'valid'"
						class="icon valid"
						title="Name is available"
					>
						✔️
					</span>
					<span
						v-else-if="nameValidationStatus === 'invalid'"
						:title="validationMessage"
						class="icon invalid"
					>
						❌
					</span>
				</div>
			</div>

			<!-- Model type -->
			<div class="step-header span-2">What is the model type?</div>
			<div class="span-2">
				<div class="mb-2">Model Type:</div>
				<Dropdown
					v-model="aiModel.type"
					:options="aiModelTypeOptions"
					option-label="label"
					option-value="value"
					placeholder="--Select--"
				/>
			</div>

			<!-- Model endpoint -->
			<div class="step-header span-2">What is the model endpoint?</div>
			<div class="span-2">
				<div class="mb-2">Model Endpoint:</div>
				<Dropdown
					v-model="aiModel.endpoint_object_id"
					:options="aiModelEndpointOptions"
					option-label="name"
					option-value="object_id"
					placeholder="--Select--"
				/>
			</div>

			<!-- Model parameters -->
			<div class="step-header span-2">What are the model parameters?</div>
			<div class="span-2">
				<PropertyBuilder v-model="aiModel.model_parameters" />
			</div>

			<!-- Buttons -->
			<div class="button-container column-2 justify-self-end">
				<!-- Create model -->
				<Button
					:label="editId ? 'Save Changes' : 'Create Model'"
					severity="primary"
					@click="handleCreate"
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
	</div>
</template>

<script lang="ts">
import type { PropType } from 'vue';
import { debounce } from 'lodash';

import api from '@/js/api';

export default {
	name: 'CreateModel',

	props: {
		editId: {
			type: [Boolean, String] as PropType<false | string>,
			required: false,
			default: false,
		},
	},

	data() {
		return {
			loading: false as boolean,
			loadingStatusText: 'Retrieving data...' as string,

			nameValidationStatus: null as string | null, // 'valid', 'invalid', or null
			validationMessage: '' as string,

			aiModelEndpointOptions: [],

			aiModel: {
				name: null as string | null,
				type: null as string | null,
				// object_id: '',
				endpoint_object_id: '' as string,
				// display_name: '' as string,
				// deployment_name: 'completions4' as string,
				// version: '0.0' as string,
				model_parameters: {} as object,
			},

			aiModelTypeOptions: [
				{
					label: 'Basic',
					value: 'basic',
				},
				{
					label: 'Embedding',
					value: 'embedding',
				},
				{
					label: 'Completion',
					value: 'completion',
				},
				{
					label: 'ImageGeneration',
					value: 'image-generation',
				},
			],
		};
	},

	async created() {
		this.loading = true;
		this.loadingStatusText = `Retrieving AI model endpoints...`;
		const modelEndpoints = (await api.getAPIEndpointConfigurations()).map(
			(endpoint) => endpoint.resource,
		);
		this.aiModelEndpointOptions = modelEndpoints.filter((resource) =>
			['AIModel'].includes(resource.subcategory),
		);
		this.loading = false;

		if (this.editId) {
			this.loading = true;
			this.loadingStatusText = `Retrieving AI model "${this.editId}"...`;
			this.aiModel = (await api.getAIModel(this.editId))[0].resource;
			this.loading = false;
		}

		this.debouncedCheckName = debounce(this.checkName, 500);
	},

	methods: {
		async checkName() {
			try {
				const response = await api.checkAIModelName(this.aiModel.name);

				// Handle response based on the status
				if (response.status === 'Allowed') {
					// Name is available
					this.nameValidationStatus = 'valid';
					this.validationMessage = null;
				} else if (response.status === 'Denied') {
					// Name is taken
					this.nameValidationStatus = 'invalid';
					this.validationMessage = response.message;
				}
			} catch (error) {
				console.error('Error checking AI model name: ', error);
				this.nameValidationStatus = 'invalid';
				this.validationMessage = 'Error checking the AI model name. Please try again.';
			}
		},

		handleCancel() {
			if (!confirm('Are you sure you want to cancel?')) {
				return;
			}

			this.$router.push('/models');
		},

		handleNameInput(event) {
			const sanitizedValue = this.$filters.sanitizeNameInput(event);
			this.aiModel.name = sanitizedValue;

			// Check if the name is available if we are creating a new data source.
			if (!this.editId) {
				this.debouncedCheckName();
			}
		},

		async handleCreate() {
			const errors: string[] = [];

			if (!this.aiModel.name) {
				errors.push('Please give the AI model a name.');
			}

			if (!this.aiModel.type) {
				errors.push('Please specify an AI model type.');
			}

			if (!this.aiModel.endpoint_object_id) {
				errors.push('Please specify an AI model endpoint.');
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
			let successMessage = null as null | string;
			try {
				this.loadingStatusText = 'Saving AI model...';
				await api.upsertAIModel(this.editId || this.aiModel.name, this.aiModel);
				successMessage = `AI model "${this.aiModel.name}" was successfully saved.`;
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

			if (!this.editId) {
				this.$router.push('/models');
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
	// display: flex;
	// flex-direction: column;
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

.primary-button {
	background-color: var(--primary-button-bg) !important;
	border-color: var(--primary-button-bg) !important;
	color: var(--primary-button-text) !important;
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

.flex-container {
	display: flex;
	align-items: center; /* Align items vertically in the center */
}

.flex-item {
	flex-grow: 1; /* Allow the textarea to grow and fill the space */
}

.flex-item-button {
	margin-left: 8px; /* Add some space between the textarea and the button */
}
</style>
