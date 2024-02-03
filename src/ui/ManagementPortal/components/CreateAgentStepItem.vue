<template>
	<div class="step">
		<div class="step-container">
			<!-- Editing view -->
			<div
				v-if="isOpen"
				class="step-container__edit"
			>
				<div class="step-container__edit__inner">

					<slot name="edit" />

					<div class="d-flex justify-content-end">
						<Button
							class="primary-button mt-2"
							label="Done"
							@click="handleClose"
						/>
					</div>
				</div>

				<div class="step-container__edit__arrow" @click="handleClose">
					<span class="pi pi-arrow-down" style="font-size: 1rem;"></span>
				</div>
			</div>

			<!-- Default view -->
			<div class="step-container__view" @click="handleOpen">
				<div class="step-container__view__inner">
					<slot />
				</div>

				<div class="step-container__view__arrow">
					<span class="pi pi-arrow-down" style="font-size: 1rem;"></span>
				</div>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
export default {
	name: 'CreateAgentStepItem',

	props: ['modelValue'],

	emits: ['update:modelValue'],

	data() {
		return {
			editing: false as boolean,
			id: Math.random(),
		};
	},

	computed: {
		isOpen() {
			return this.editing && (this.$appStore.createAgentOpenItemId === this.id);
		}
	},

	watch: {
		isOpen() {
			this.$emit('update:modelValue', this.isOpen);
		},

		modelValue() {
			this.modelValue ? this.handleOpen() : this.handleClose();
		}
	},

	methods: {
		handleOpen() {
			this.editing = true;
			this.$appStore.createAgentOpenItemId = this.id;
		},

		handleClose() {
			this.editing = false;
			if (this.$appStore.createAgentOpenItemId === this.id) {
				this.$appStore.createAgentOpenItemId = null;
			}
		},
	},
};
</script>

<style lang="scss" scoped>

</style>

