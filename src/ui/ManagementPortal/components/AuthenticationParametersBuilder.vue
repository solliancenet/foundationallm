<template>
	<div class="d-flex flex-column gap-4">
		<!-- Authenication parameters table -->
		<DataTable
			:value="Object.keys(parameters).map((key) => ({ key, value: parameters[key] }))"
			striped-rows
			scrollable
			table-style="max-width: 100%"
			size="small"
		>
			<template #empty>No authentication parameters added.</template>

			<!-- Parameter secret toggle -->
			<!-- 	<Column
				field="secret"
				header="Secret"
				sortable
				:pt="{
					headerCell: {
						style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
					},
					sortIcon: { style: { color: 'var(--primary-text)' } },
				}"
			>
				<template #body="{ data }">
					<i v-if="data.secret" class="pi pi-lock mr-2"></i>
					<span>{{ data.secret }}</span>
				</template>
			</Column> -->

			<!-- Parameter key -->
			<Column
				field="key"
				header="Parameter Key"
				sortable
				:pt="{
					headerCell: {
						style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
					},
					sortIcon: { style: { color: 'var(--primary-text)' } },
				}"
			/>

			<!-- Parameter value -->
			<Column
				field="value"
				header="Parameter Value"
				sortable
				:pt="{
					headerCell: {
						style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
					},
					sortIcon: { style: { color: 'var(--primary-text)' } },
				}"
			>
				<!-- <template #body="{ data }">
					{{ data.secret ? '[VALUE HIDDEN]' : data.value }}
				</template> -->
			</Column>

			<!-- Edit parameter -->
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
					<Button link @click="handleEditParameter(data.key)">
						<i class="pi pi-cog" style="font-size: 1.2rem"></i>
					</Button>
				</template>
			</Column>

			<!-- Delete parameter -->
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
					<Button link @click="handleDeleteParameter(data.key)">
						<i class="pi pi-trash" style="font-size: 1.2rem"></i>
					</Button>
				</template>
			</Column>
		</DataTable>

		<!-- Add parameter button -->
		<div class="d-flex w-100 justify-content-end">
			<Button
				severity="primary"
				style="word-wrap: none"
				@click="showCreateOrEditParameterDialog = true"
			>
				Add Authentication Parameter
			</Button>
		</div>

		<!-- Create / edit parameter dialog -->
		<div class="d-flex gap-4">
			<Dialog
				v-if="showCreateOrEditParameterDialog"
				:visible="showCreateOrEditParameterDialog"
				modal
				:header="
					parameterToEdit.currentKey
						? 'Edit Authentication Parameter'
						: 'Add Authentication Parameter'
				"
				:style="{ minWidth: '50%' }"
				:closable="false"
			>
				<!-- Parameter key -->
				<div class="mb-1">Parameter Key:</div>
				<InputText
					v-model="parameterToEdit.key"
					type="text"
					placeholder="Enter parameter key"
					@input="handleParameterKeyInput"
				/>

				<!-- Parameter secret toggle -->
				<!-- <div class="mb-1 mt-4">Is the parameter secret?</div>
				<ToggleButton
					v-model="parameterToEdit.secret"
					onIcon="pi pi-lock"
					offIcon="pi pi-lock-open"
					class="w-36"
					aria-label="Do you confirm"
				/> -->

				<!-- Parameter value -->
				<div class="mb-1 mt-4">Parameter Value:</div>

				<!-- Secret value -->
				<!-- <SecretKeyInput
					v-if="parameterToEdit.secret"
					v-model="parameterToEdit.value"
					placeholder="Enter parameter secret value"
					aria-labelledby="aria-api-key"
				/> -->

				<!-- Plain value -->
				<InputText v-model="parameterToEdit.value" placeholder="Enter parameter value" />

				<template #footer>
					<!-- Save -->
					<Button severity="primary" label="Save" @click="handleAddParameter" />

					<!-- Cancel -->
					<Button class="ml-2" label="Close" text @click="handleClose" />
				</template>
			</Dialog>
		</div>
	</div>
</template>

<script lang="ts">
import { renameObjectKey } from '@/js/helpers';

export default {
	props: {
		modelValue: {
			type: Object,
			required: false,
			default: () => ({}),
		},
	},

	data() {
		return {
			parameters: {},

			showCreateOrEditParameterDialog: false,
			parameterToEdit: {
				key: '',
				value: '',
				secret: false,
			},
		};
	},

	watch: {
		modelValue: {
			immediate: true,
			deep: true,
			handler() {
				this.parameters = this.modelValue;
			},
		},
	},

	methods: {
		handleParameterKeyInput(event) {
			// Filter to only allow lowercase letters and underscores
			let sanitizedValue = event.target.value.replace(/[^a-zA-Z_]/g, '').toLowerCase();

			event.target.value = sanitizedValue;
			this.parameterToEdit.key = sanitizedValue;
		},

		handleClose() {
			this.showCreateOrEditParameterDialog = false;
			this.parameterToEdit = {
				currentKey: '',
				key: '',
				value: '',
				// secret: false,
			};
		},

		handleEditParameter(propertyKey) {
			this.parameterToEdit = {
				currentKey: propertyKey,
				key: propertyKey,
				value: this.parameters[propertyKey],
			};
			this.showCreateOrEditParameterDialog = true;
		},

		handleDeleteParameter(propertyKey) {
			delete this.parameters[propertyKey];
			this.$emit('update:modelValue', this.parameters);
		},

		handleAddParameter() {
			const errors = [];

			if (!this.parameterToEdit.key) {
				errors.push('The parameter requires a key name.');
			}

			if (!this.parameterToEdit.value) {
				errors.push('The parameter requires a value.');
			}

			if (errors.length > 0) {
				this.$toast.add({
					severity: 'error',
					detail: errors.join('\n'),
					life: 5000,
				});

				return;
			}

			// Rename the parameter key while preserving the key order
			this.parameters = renameObjectKey(
				this.parameters,
				this.parameterToEdit.currentKey,
				this.parameterToEdit.key,
			);
			this.parameters[this.parameterToEdit.key] = this.parameterToEdit.value;

			this.showCreateOrEditParameterDialog = false;
			this.parameterToEdit = {
				currentKey: '',
				key: '',
				value: '',
				// secret: false,
			};
			this.$emit('update:modelValue', this.parameters);
		},
	},
};
</script>

<style lang="scss"></style>
