<template>
	<Dialog
		:visible="visible"
		modal
		header="Edit JSON"
		:style="{ minWidth: '50%' }"
		:closable="false"
	>
		<JsonEditorVue v-model="json" />

		<template #footer>
			<!-- Save -->
			<Button severity="primary" label="Save" @click="handleSave" />

			<!-- Cancel -->
			<Button class="ml-2" label="Close" text @click="handleClose" />
		</template>
	</Dialog>
</template>

<script>
import JsonEditorVue from 'json-editor-vue';

export default {
	components: {
		JsonEditorVue,
	},

	props: {
		modelValue: {
			type: [Object, String],
			required: true,
		},

		visible: {
			type: Boolean,
			required: false,
		},
	},

	data() {
		return {
			json: {},
		};
	},

	watch: {
		modelValue: {
			immediate: true,
			deep: true,
			handler() {
				if (JSON.stringify(this.modelValue) === JSON.stringify(this.json)) return;
				this.json = this.modelValue;
			},
		},
	},

	methods: {
		handleSave() {
			this.$emit('update:modelValue', this.json);
			this.handleClose();
		},

		handleClose() {
			this.$emit('update:visible', false);
		},
	},
};
</script>

<style lang="scss" scoped></style>
