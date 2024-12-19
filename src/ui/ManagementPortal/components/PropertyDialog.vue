<template>
	<Dialog :visible="visible" modal :header="title" :style="{ minWidth: '50%' }" :closable="false">
		<!-- Property name -->
		<div class="mb-1">Property Key:</div>
		<InputText v-model="propertyKey" type="text" placeholder="Property Key" />

		<!-- Property type -->
		<div class="mb-1 mt-4">Property Type:</div>
		<Dropdown
			v-model="propertyType"
			:options="propertyTypeOptions"
			option-label="label"
			option-value="value"
			placeholder="--Select--"
		/>

		<!-- Property Value -->
		<div class="mb-1 mt-4">Property Value:</div>

		<!-- String -->
		<InputText
			v-if="propertyType === 'string'"
			v-model="propertyValue"
			type="text"
			placeholder="Property Value"
		/>

		<!-- Number -->
		<InputNumber
			v-if="propertyType === 'number'"
			:minFractionDigits="0"
			:maxFractionDigits="6"
			v-model="propertyValue"
			placeholder="Property Value"
		/>

		<!-- Boolean -->
		<Dropdown
			v-else-if="propertyType === 'boolean'"
			v-model="propertyValue"
			:options="[
				{
					label: 'True',
					value: true,
				},
				{
					label: 'False',
					value: false,
				},
			]"
			option-label="label"
			option-value="value"
			placeholder="--Select--"
		/>

		<!-- Object -->
		<JsonEditorVue v-else-if="propertyType === 'object'" v-model="propertyValue" />

		<template #footer>
			<!-- Save -->
			<Button severity="primary" label="Save" @click="handleSave" />

			<!-- Cancel -->
			<Button class="ml-2" label="Close" text @click="handleClose" />
		</template>
	</Dialog>
</template>

<script lang="ts">
import JsonEditorVue from 'json-editor-vue';

export default {
	components: {
		JsonEditorVue,
	},

	props: {
		modelValue: {
			type: [Object],
			required: false,
			default: () => ({
				key: '',
				value: '',
			}),
		},

		visible: {
			type: Boolean,
			required: false,
		},

		title: {
			type: String,
			required: false,
			default: 'Create Property',
		},
	},

	data() {
		return {
			propertyKey: '' as string,
			propertyType: 'string' as string,
			propertyValue: '' as any,
			propertyTypeOptions: [
				{
					label: 'String',
					value: 'string',
				},
				{
					label: 'Number',
					value: 'number',
				},
				{
					label: 'Boolean',
					value: 'boolean',
				},
				{
					label: 'Object / Array',
					value: 'object',
				},
			],
		};
	},

	watch: {
		modelValue: {
			immediate: true,
			deep: true,
			handler() {
				if (!this.modelValue?.key || this.modelValue?.value === undefined) return;
				this.propertyKey = this.modelValue.key;
				this.propertyValue = this.modelValue.value;
				this.propertyType = typeof this.modelValue.value;
			},
		},

		propertyType() {
			switch (this.propertyType) {
				case 'string':
					this.propertyValue = '';
					break;
				case 'number':
					this.propertyValue = 0;
					break;
				case 'boolean':
					this.propertyValue = true;
					break;
				case 'object':
					this.propertyValue = undefined;
					break;
			}
		},
	},

	methods: {
		handleSave() {
			const errors = [];

			if (!this.propertyKey) {
				errors.push('Please input a property key.');
			}

			if (!this.propertyType) {
				errors.push('Please select a property type.');
			}

			// if (this.propertyValue === null || this.propertyValue === undefined) {
			// 	errors.push('Please select a property value.');
			// }

			let parsedPropertyValue;
			if (this.propertyType === 'object' && typeof this.propertyValue === 'string') {
				try {
					parsedPropertyValue = JSON.parse(this.propertyValue);
				} catch (error) {
					errors.push(error);
				}
			} else {
				parsedPropertyValue = this.propertyValue;
			}

			if (errors.length > 0) {
				this.$toast.add({
					severity: 'error',
					detail: errors.join('\n'),
					life: 5000,
				});

				return;
			}

			this.$emit('update:modelValue', { key: this.propertyKey, value: parsedPropertyValue });
		},

		handleClose() {
			this.$emit('update:visible', false);
		},
	},
};
</script>

<style lang="scss" scoped></style>
