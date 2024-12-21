<template>
	<div class="step">
		<div class="step-container">
			<!-- Editing view -->
			<div
				v-if="isOpen"
				class="step-container__edit"
				:id="'step-content-' + id"
				role="region"
				:aria-labelledby="'step-header-' + id"
			>
				<div class="step-container__edit__inner">
					<slot name="edit" />

					<div class="d-flex justify-content-end">
						<Button
							class="mt-2"
							label="Done"
							aria-label="Close step editing"
							@click="handleClose"
						/>
					</div>
				</div>

				<div
					class="step-container__edit__arrow"
					role="button"
					aria-label="Close editing"
					tabindex="0"
					@click="handleClose"
					@keydown.enter.prevent="handleClose"
					@keydown.space.prevent="handleClose"
				>
					<span class="pi pi-arrow-down" style="font-size: 1rem"></span>
				</div>
			</div>

			<!-- Default view -->
			<div
				class="step-container__view"
				@click="handleOpen"
				@keydown.enter.prevent="handleOpen"
				@keydown.space.prevent="handleOpen"
				role="button"
				tabindex="0"
				:aria-expanded="isOpen"
				:aria-controls="'step-content-' + id"
			>
				<div class="step-container__view__inner">
					<slot />
				</div>

				<div class="step-container__view__arrow">
					<span class="pi pi-arrow-down" style="font-size: 1rem"></span>
				</div>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
export default {
	name: 'CreateAgentStepItem',

	props: {
		modelValue: {
			type: Boolean,
			required: false,
			default: false,
		},
		focusQuery: {
			type: String,
			required: false,
			default: '',
		},
	},

	emits: ['update:modelValue'],

	data() {
		return {
			editing: false as boolean,
			id: Math.random(),
		};
	},

	computed: {
		isOpen() {
			return this.editing && this.$appStore.createAgentOpenItemId === this.id;
		},
	},

	watch: {
		isOpen() {
			this.$emit('update:modelValue', this.isOpen);
		},

		modelValue() {
			this.modelValue ? this.handleOpen() : this.handleClose();
		},
	},

	methods: {
		handleOpen() {
			this.editing = true;
			this.$appStore.createAgentOpenItemId = this.id;

			this.$nextTick(() => {
				const focusElement = this.$el.querySelector(this.focusQuery);
				focusElement?.focus();
			});
		},

		handleClose() {
			this.editing = false;
			if (this.$appStore.createAgentOpenItemId === this.id) {
				this.$appStore.createAgentOpenItemId = null;
			}

			this.$nextTick(() => {
				const stepHeader = this.$el.querySelector('.step-container__view');
				stepHeader?.focus();
			});
		},
	},
};
</script>

<style lang="scss" scoped></style>
