<template>
	<div class="step">
		<div class="step-container">
			<!-- Editing view -->
			<div
				v-if="editing"
				class="step-container__edit"
			>
				<div class="step-container__edit__inner">

					<slot name="edit" />

					<div class="d-flex justify-content-end">
						<Button
							class="primary-button mt-2"
							label="Done"
							@click="editing = false"
						/>
					</div>
				</div>

				<div class="step-container__edit__arrow" @click="editing = false">
					<span class="pi pi-arrow-down" style="font-size: 1rem;"></span>
				</div>
			</div>

			<!-- Default view -->
			<div class="step-container__view" @click="editing = true">
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

	watch: {
		editing() {
			this.$emit('update:modelValue', this.editing);
		},

		modelValue() {
			this.editing = this.modelValue;
		}
	},

	data() {
		return {
			editing: false as boolean,
		};
	},
};
</script>

<style lang="scss" scoped>

</style>

