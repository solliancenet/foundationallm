<template>
	<Dialog
		:visible="visible"
		modal
		:header="modelValue?.url ? 'Edit URL Exception' : 'Create URL Exception'"
		:style="{ minWidth: '80%' }"
		:closable="false"
	>
		<div id="aria-url-exception-url" class="mb-2 font-weight-bold">URL:</div>
		<InputText
			v-model="urlException.url"
			type="text"
			class="w-100"
			placeholder="Enter url"
			aria-labelledby="aria-url-exception-url"
		/>

		<div id="aria-url-exception-user-principal-name" class="mt-6 mb-2 font-weight-bold">
			User Principal Name:
		</div>
		<InputText
			v-model="urlException.user_principal_name"
			type="text"
			class="w-100"
			placeholder="Enter user principal name"
			aria-labelledby="aria-url-exception-user-principal-name"
		/>

		<div id="aria-url-exception-enabled" class="mt-6 mb-2 font-weight-bold">Enabled:</div>
		<InputSwitch v-model="urlException.enabled" aria-labelledby="aria-url-exception-enabled" />

		<template #footer>
			<!-- Save -->
			<Button severity="primary" label="Save" @click="handleSave" />

			<!-- Cancel -->
			<Button class="ml-2" label="Close" text @click="handleClose" />
		</template>
	</Dialog>
</template>

<script lang="ts">
import { cloneDeep } from 'lodash';

export default {
	props: {
		modelValue: {
			type: Object,
			required: false,
			default: () => ({
				url: '' as string,
				user_principal_name: '' as string,
				enabled: true as boolean,
			}),
		},

		visible: {
			type: Boolean,
			required: false,
		},
	},

	data() {
		return {
			urlException: {
				url: '' as string,
				user_principal_name: '' as string,
				enabled: true as boolean,
			},
		};
	},

	watch: {
		modelValue: {
			immediate: true,
			deep: true,
			handler() {
				if (JSON.stringify(this.modelValue) === JSON.stringify(this.urlException)) return;
				this.urlException = cloneDeep(this.modelValue);
			},
		},
	},

	methods: {
		handleSave() {
			const errors = [];

			if (!this.urlException.url) {
				errors.push('Please provide a url for the url exception.');
			}

			if (!this.urlException.user_principal_name) {
				errors.push('Please provide a user principal name for the url exception.');
			}

			if (errors.length > 0) {
				this.$toast.add({
					severity: 'error',
					detail: errors.join('\n'),
					life: 5000,
				});

				return;
			}

			this.$emit('update:modelValue', this.urlException);
		},

		handleClose() {
			this.$emit('update:visible', false);
		},
	},
};
</script>

<style lang="scss" scoped></style>
