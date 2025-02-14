<template>
	<main id="main-content">
		<div style="display: flex">
			<!-- Title -->
			<div style="flex: 1">
				<h2 class="page-header">
					{{ editPrompt ? 'Edit Prompt' : 'Create New Prompt' }}
				</h2>
				<div class="page-subheader">
					{{
						editPrompt
							? 'Edit your prompt below.'
							: 'Complete the settings below to create a new prompt.'
					}}
				</div>
			</div>

			<!-- Edit access control -->
			<AccessControl
				v-if="editPrompt"
				:scopes="[
					{
						label: 'Prompt',
						value: `providers/FoundationaLLM.Prompt/prompts/${prompt.name}`,
					},
				]"
			/>
		</div>

		<div class="steps">
			<!-- Loading overlay -->
			<template v-if="loading">
				<div class="steps__loading-overlay" role="status" aria-live="polite">
					<LoadingGrid />
					<div>{{ loadingStatusText }}</div>
				</div>
			</template>

			<div class="span-2">
				<div id="aria-prompt-name" class="step-header mb-2">Prompt name:</div>
				<div id="aria-prompt-name-desc" class="mb-2">
					No special characters or spaces, use letters and numbers with dashes and underscores only.
				</div>
				<div class="input-wrapper">
					<InputText
						v-model="prompt.name"
						:disabled="editPrompt"
						type="text"
						class="w-100"
						placeholder="Enter prompt name"
						aria-labelledby="aria-prompt-name aria-prompt-name-desc"
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
						:title="validationMessage"
						class="icon invalid"
						:aria-label="validationMessage"
					>
						❌
					</span>
				</div>
			</div>
			<div class="span-2">
				<div class="step-header mb-2">Description:</div>
				<div id="aria-description" class="mb-2">
					Provide a description to help others understand the prompt's purpose.
				</div>
				<InputText
					v-model="prompt.description"
					type="text"
					class="w-100"
					placeholder="Enter prompt description"
					aria-labelledby="aria-description"
				/>
			</div>
			<div class="span-2">
				<div class="step-header mb-2">Category:</div>
				<Dropdown
					v-model="prompt.category"
					:options="categoryOptions"
					option-label="label"
					option-value="value"
					class="dropdown--agent"
					placeholder="--Select--"
					aria-labelledby="aria-source-type"
				/>
			</div>

			<!-- System prompt -->
			<section aria-labelledby="system-prompt" class="span-2 steps">
				<h3 class="step-section-header span-2" id="system-prompt">Prompt Prefix</h3>

				<div class="span-2">
					<Textarea
						v-model="prompt.prefix"
						class="w-100"
						auto-resize
						rows="5"
						type="text"
						placeholder="You are an analytic agent named Khalil that helps people find information about FoundationaLLM. Provide concise answers that are polite and professional."
						aria-labelledby="aria-persona"
					/>
				</div>
			</section>

			<div class="span-2 d-flex justify-content-end" style="gap: 16px">
				<!-- Create prompt -->
				<Button
					:label="editPrompt ? 'Save Changes' : 'Create Prompt'"
					severity="primary"
					:disabled="editable === false"
					@click="handleCreatePrompt"
				/>

				<!-- Cancel -->
				<Button v-if="editPrompt" label="Cancel" severity="secondary" @click="handleCancel" />
			</div>
		</div>
	</main>
</template>

<script lang="ts">
import type { PropType } from 'vue';
import { ref } from 'vue';
import { debounce } from 'lodash';
import api from '@/js/api';
import type { Prompt, CreatePromptRequest } from '@/js/types';

export default {
	name: 'CreatePrompt',

	props: {
		editPrompt: {
			type: [Boolean, String] as PropType<false | string>,
			required: false,
			default: false,
		},
		promptName: {
			type: String as PropType<string>,
			required: false,
			default: '',
		},
	},

	data() {
		return {
			loading: false as boolean,
			loadingStatusText: 'Retrieving data...' as string,

			prompt: {} as Prompt,

			editable: false as boolean,

			nameValidationStatus: null as string | null, // 'valid', 'invalid', or null
			validationMessage: '' as string,

			categoryOptions: [
				{
					label: 'Workflow',
					value: 'Workflow',
				},
				{
					label: 'Tool',
					value: 'Tool',
				},
			],
		};
	},

	async created() {
		this.loading = true;

		if (this.editPrompt && this.promptName !== '') {
			this.loadingStatusText = `Retrieving prompt: ${this.promptName}...`;
			const promptGetResult = await api.getPromptByName(this.promptName);
			this.editable =
				promptGetResult?.actions.includes('FoundationaLLM.Prompt/prompts/write') ?? false;

			const prompt = promptGetResult?.resource;
			this.loadingStatusText = `Mapping prompt values to form...`;
			if (prompt) {
				this.prompt = prompt;
			}
		} else {
			this.editable = true;
		}

		this.debouncedCheckName = debounce(this.checkName, 500);

		this.loading = false;
	},

	methods: {
		handleCancel() {
			if (!confirm('Are you sure you want to cancel?')) {
				return;
			}
			this.$router.push('/prompts');
		},

		async checkName() {
			try {
				const response = await api.checkPromptName(this.prompt.name, this.prompt.type);

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
				console.error('Error checking prompt name: ', error);
				this.nameValidationStatus = 'invalid';
				this.validationMessage = 'Error checking the prompt name. Please try again.';
			}
		},

		handleNameInput(event) {
			const sanitizedValue = this.$filters.sanitizeNameInput(event);
			this.prompt.name = sanitizedValue;
			this.sourceName = sanitizedValue;

			// Check if the name is available if we are creating a new prompt.
			if (!this.editPrompt) {
				this.debouncedCheckName();
			}
		},

		async handleCreatePrompt() {
			const errors = [];
			if (!this.prompt.name) {
				errors.push('Please give the prompt a name.');
			}
			if (this.nameValidationStatus === 'invalid') {
				errors.push(this.validationMessage);
			}

			if (!this.prompt.prefix) {
				errors.push('The prompt requires a prefix.');
			}

			if (!this.prompt.category) {
				errors.push('Please select a category for the prompt.');
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
			this.loadingStatusText = 'Saving prompt...';

			const promptRequest: CreatePromptRequest = {
				type: 'multipart',
				name: this.prompt.name,
				cost_center: this.prompt.cost_center,
				description: this.prompt.description,
				prefix: this.prompt.prefix,
				suffix: this.prompt.suffix,
				object_id: this.prompt.object_id || '',
				display_name: this.prompt.display_name,
				expiration_date: this.prompt.expiration_date,
				properties: this.prompt.properties,
				category: this.prompt.category,
			};

			try {
				await api.createOrUpdatePrompt(this.prompt.name, promptRequest);
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
				detail: `Prompt ${this.editPrompt ? 'updated' : 'created'} successfully.`,
				life: 5000,
			});

			this.loading = false;

			this.$router.push('/prompts');
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
