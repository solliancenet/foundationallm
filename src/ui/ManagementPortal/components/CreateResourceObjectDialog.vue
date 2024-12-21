<template>
	<Dialog
		:visible="visible"
		modal
		header="Create Resource Object"
		:style="{ minWidth: '50%' }"
		:closable="false"
	>
		<template v-if="loading">
			<div class="loading-overlay" role="status" aria-live="polite">
				<LoadingGrid />
				<div>{{ loadingStatusText }}</div>
			</div>
		</template>

		<!-- Resource type -->
		<div class="mb-1">Resource Type:</div>
		<Dropdown
			v-model="resourceType"
			:options="resourceTypeOptions"
			option-label="label"
			option-value="value"
			placeholder="--Select--"
		/>

		<!-- Resource selection -->
		<div class="mb-1 mt-4">Resource:</div>
		<Dropdown
			v-model="resourceId"
			:options="resourceOptions"
			option-label="name"
			option-value="object_id"
			placeholder="--Select--"
		/>

		<!-- Resource role -->
		<div class="mb-1 mt-4">Resource Role:</div>
		<InputText
			v-model="resourceRole"
			type="text"
			class="w-100"
			placeholder="Enter resource role"
			aria-labelledby="aria-cost-center"
		/>

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
import api from '@/js/api';

export default {
	components: {
		JsonEditorVue,
	},

	props: {
		modelValue: {
			type: [Object, String],
			required: false,
			default: () => ({
				name: '' as string,
				description: '' as string,
				package_name: '' as string,
				resource_object_ids: [] as object[],
			}),
		},

		visible: {
			type: Boolean,
			required: false,
		},
	},

	data() {
		return {
			loading: false,
			loadingStatusText: '',

			resourceId: '' as string,
			resourceRole: '' as string,

			resourceType: null,
			resourceTypeOptions: [
				{
					label: 'Model',
					value: 'model',
				},
				{
					label: 'Indexing Profile',
					value: 'indexingProfile',
				},
			],

			resourceOption: null,
			resourceOptions: [],
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

		async resourceType(newType, oldType) {
			if (newType === oldType) return;

			let apiMethod = null;
			if (this.resourceType === 'model') {
				this.loadingStatusText = 'Loading models...';
				apiMethod = api.getAIModels;
			} else if (this.resourceType === 'indexingProfile') {
				this.loadingStatusText = 'Loading indexing profiles...';
				apiMethod = api.getAgentIndexes;
			}

			this.loading = true;
			try {
				this.resourceOptions = (await apiMethod.call(api)).map((resource) => resource.resource);
			} catch (error) {
				this.$toast.add({
					severity: 'error',
					detail: error?.response?._data || error,
					life: 5000,
				});
			}
			this.loading = false;
		},
	},

	methods: {
		handleSave() {
			const errors = [];

			if (!this.resourceId) {
				errors.push('Please select a resource.');
			}

			if (!this.resourceRole) {
				errors.push('Please input a resource role.');
			}

			if (errors.length > 0) {
				this.$toast.add({
					severity: 'error',
					detail: errors.join('\n'),
					life: 5000,
				});

				return;
			}

			this.$emit('update:modelValue', {
				object_id: this.resourceId,
				properties: {
					object_role: this.resourceRole,
				},
			});
		},

		handleClose() {
			this.$emit('update:visible', false);
		},
	},
};
</script>

<style lang="scss" scoped>
.loading-overlay {
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
</style>
