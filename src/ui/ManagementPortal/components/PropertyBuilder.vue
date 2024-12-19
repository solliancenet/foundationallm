<template>
	<div class="d-flex flex-column gap-4">
		<div
			v-for="(propertyValue, propertyKey) in properties"
			:key="propertyKey"
			class="d-flex justify-content-between gap-4"
		>
			<div class="d-flex flex-1 gap-4">
				<!-- Property name -->
				<InputText :value="propertyKey" type="text" placeholder="Property Name" disabled />

				<span class="d-flex align-center">=</span>

				<!-- Property value -->
				<InputText
					:value="JSON.stringify(propertyValue)"
					type="text"
					placeholder="Property Value"
					disabled
				/>
			</div>

			<!-- Edit property -->
			<Button link @click="handleEditProperty(propertyKey)">
				<i class="pi pi-cog" style="font-size: 1.2rem"></i>
			</Button>

			<!-- Delete property -->
			<Button link @click="handleDeleteProperty(propertyKey)">
				<i class="pi pi-trash" style="font-size: 1.2rem"></i>
			</Button>
		</div>

		<div class="d-flex gap-4">
			<PropertyDialog
				v-if="showCreateOrEditPropertyDialog"
				v-model="propertyToEdit"
				:title="propertyToEdit ? 'Edit Property' : 'Create Property'"
				:visible="showCreateOrEditPropertyDialog"
				@update:modelValue="handleAddProperty($event)"
				@update:visible="showCreateOrEditPropertyDialog = false"
			/>

			<!-- Add property -->
			<Button
				label="Add Property"
				severity="primary"
				style="word-wrap: none"
				@click="showCreateOrEditPropertyDialog = true"
			/>
		</div>
	</div>
</template>

<script lang="ts">
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
			showCreateOrEditPropertyDialog: false,
			propertyToEdit: null,

			properties: {},
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
		handleEditProperty(propertyKey) {
			this.propertyToEdit = { key: propertyKey, value: this.properties[propertyKey] };
			this.showCreateOrEditPropertyDialog = true;
		},

		handleDeleteProperty(propertyKey) {
			delete this.properties[propertyKey];
			this.$emit('update:modelValue', this.properties);
		},

		handleAddProperty(propertyObject) {
			const errors = [];

			// if (!this.propertyKey) {
			// 	errors.push('Please input a property name.');
			// }

			// if (!this.propertyValue) {
			// 	errors.push('Please input a property value.');
			// }

			// If we are not editing a property, prevent overwriting an existing one
			if (!this.propertyToEdit && this.properties[propertyObject.key] !== undefined) {
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

			this.properties[propertyObject.key] = propertyObject.value;
			this.showCreateOrEditPropertyDialog = false;
			this.propertyToEdit = null;
			this.$emit('update:modelValue', this.properties);
		},
	},
};
</script>

<style lang="scss"></style>
