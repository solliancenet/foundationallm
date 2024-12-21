<template>
	<Dialog
		:visible="visible"
		modal
		header="Configure Tool"
		:style="{ minWidth: '80%' }"
		:closable="false"
	>
		<div id="aria-tool-name" class="mb-2 font-weight-bold">Tool name:</div>
		<InputText
			v-model="toolObject.name"
			type="text"
			class="w-100"
			placeholder="Enter tool name"
			aria-labelledby="aria-tool-name"
		/>

		<div id="aria-tool-description" class="mt-6 mb-2 font-weight-bold">Tool description:</div>
		<Textarea
			v-model="toolObject.description"
			auto-resize
			rows="5"
			type="text"
			class="w-100"
			placeholder="Enter tool description"
			aria-labelledby="aria-tool-description"
		/>

		<div id="aria-tool-package-name" class="mt-6 mb-2 font-weight-bold">Tool package name:</div>
		<InputText
			v-model="toolObject.package_name"
			type="text"
			class="w-100"
			placeholder="Enter tool package name"
			aria-labelledby="aria-tool-package-name"
		/>

		<div class="mt-6 mb-2 font-weight-bold">Resource objects:</div>
		<div v-for="(resourceObject, resourceObjectId) in toolObject.resource_object_ids" class="ml-2">
			<div class="mt-6 mb-2">
				<span id="aria-resource-object-id" class="font-weight-bold"
					>{{ getResourceTypeFromId(resourceObjectId) }}:
				</span>
				<span id="aria-resource-object-name">{{ getResourceNameFromId(resourceObjectId) }}</span>
			</div>

			<PropertyBuilder v-model="toolObject.resource_object_ids[resourceObjectId].properties" />
		</div>

		<CreateResourceObjectDialog
			v-if="showCreateResourceObjectDialog"
			:visible="showCreateResourceObjectDialog"
			@update:visible="showCreateResourceObjectDialog = false"
			@update:modelValue="handleAddResourceObject"
		/>
		<div class="d-flex justify-content-end mt-4">
			<Button
				severity="primary"
				label="Add Resource Object"
				@click="showCreateResourceObjectDialog = true"
			/>
		</div>

		<div class="mt-6 mb-2 font-weight-bold">Tool properties:</div>
		<PropertyBuilder v-model="toolObject.properties" />

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
			type: [Object, String],
			required: false,
			default: () => ({
				name: '' as string,
				description: '' as string,
				package_name: '' as string,
				resource_object_ids: {},
			}),
		},

		visible: {
			type: Boolean,
			required: false,
		},
	},

	data() {
		return {
			toolObject: {
				name: '' as string,
				description: '' as string,
				package_name: '' as string,
				resource_object_ids: {},
			},
			showCreateResourceObjectDialog: false,
		};
	},

	watch: {
		modelValue: {
			immediate: true,
			deep: true,
			handler() {
				if (JSON.stringify(this.modelValue) === JSON.stringify(this.toolObject)) return;
				this.toolObject = this.modelValue;
			},
		},
	},

	methods: {
		getResourceNameFromId(resourceId) {
			const parts = resourceId.split('/').filter(Boolean);
			return parts.slice(-1)[0];
		},

		getResourceTypeFromId(resourceId) {
			const parts = resourceId.split('/').filter(Boolean);
			const type = parts.slice(-2)[0];

			if (type === 'prompts') {
				return 'Prompt';
			} else if (type === 'aiModels') {
				return 'AI Model';
			} else if (type === 'indexingProfiles') {
				return 'Indexing Profile';
			}

			return type;
		},

		handleAddResourceObject(resourceObject) {
			this.toolObject.resource_object_ids[resourceObject.object_id] = resourceObject;
			this.showCreateResourceObjectDialog = false;
		},

		handleSave() {
			this.$emit('update:modelValue', this.toolObject);
		},

		handleClose() {
			this.$emit('update:visible', false);
		},
	},
};
</script>

<style lang="scss" scoped></style>
