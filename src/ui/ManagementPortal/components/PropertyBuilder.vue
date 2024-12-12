<template>
	<div class="d-flex flex-column gap-4">
		<div
			v-for="(propertyValue, propertyKey) in properties"
			:key="propertyKey"
			class="d-flex justify-content-between gap-4"
		>
			<div class="d-flex flex-1">
				<!-- Property name -->
				<InputText :value="propertyKey" type="text" placeholder="Property Name" disabled />

				<span class="d-flex align-center">=</span>

				<!-- Property value -->
				<InputText :value="propertyValue" type="text" placeholder="Property Value" disabled />
			</div>

			<!-- Delete property -->
			<Button link @click="handleDeleteProperty(propertyKey)">
				<i class="pi pi-trash" style="font-size: 1.2rem"></i>
			</Button>
		</div>

		<div class="d-flex gap-4">
			<div class="d-flex flex-1 gap-4">
				<!-- Property name -->
				<InputText v-model="propertyName" type="text" placeholder="Property Name" />

				<!-- Property value -->
				<InputText v-model="propertyValue" type="text" placeholder="Property Value" />
			</div>

			<!-- Add property -->
			<Button
				label="Add Property"
				severity="primary"
				style="word-wrap: none"
				@click="handleAddProperty"
			/>
		</div>
	</div>
</template>

<script lang="ts">
export default {
	props: {
		modelValue: {
			type: Object,
			required: true,
		},
	},

	data() {
		return {
			properties: {},
			propertyName: '' as string,
			propertyValue: '' as string,
		};
	},

	watch: {
		modelValue: {
			immediate: true,
			deep: true,
			handler() {
				this.properties = this.modelValue;
			},
		},
	},

	methods: {
		handleDeleteProperty(propertyName) {
			delete this.properties[propertyName];
			this.$emit('update:modelValue', this.properties);
		},

		handleAddProperty() {
			const errors = [];

			if (!this.propertyName) {
				errors.push('Please input a property name.');
			}

			if (!this.propertyValue) {
				errors.push('Please input a property value.');
			}

			if (this.properties[this.propertyName]) {
				errors.push('This property name already exists.');
			}

			if (errors.length > 0) {
				this.$toast.add({
					severity: 'error',
					detail: errors.join('\n'),
					life: 5000,
				});

				return;
			}

			this.properties[this.propertyName] = this.propertyValue;
			this.propertyName = '';
			this.propertyValue = '';
			this.$emit('update:modelValue', this.properties);
		},
	},
};
</script>

<style lang="scss"></style>
