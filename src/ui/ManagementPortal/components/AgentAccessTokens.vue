<template>
	<div>
		<!-- Loading overlay -->
		<template v-if="loadingAccessTokens">
			<div class="d-flex flex-column justify-content-center align-center">
				<LoadingGrid />
				<div>Loading agent access tokens...</div>
			</div>
		</template>

		<template v-else>
			<!-- Access tokens table -->
			<DataTable
				:value="accessTokens"
				striped-rows
				scrollable
				table-style="max-width: 100%"
				size="small"
			>
				<template #empty>No access tokens found.</template>

				<template #loading>Loading access tokens. Please wait.</template>

				<!-- Description -->
				<Column
					field="description"
					header="Description"
					sortable
					:pt="{
						headerCell: {
							style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
						},
						sortIcon: { style: { color: 'var(--primary-text)' } },
					}"
				/>

				<!-- Expiration date -->
				<Column
					field="expiration_date"
					header="Expiration Date"
					sortable
					:pt="{
						headerCell: {
							style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
						},
						sortIcon: { style: { color: 'var(--primary-text)' } },
					}"
				>
					<template #body="{ data }">
						{{ new Date(data.expiration_date) }}
					</template>
				</Column>

				<!-- Delete -->
				<Column
					header="Delete"
					header-style="width:6rem"
					style="text-align: center"
					:pt="{
						headerCell: {
							style: { backgroundColor: 'var(--primary-color)', color: 'var(--primary-text)' },
						},
						headerContent: { style: { justifyContent: 'center' } },
					}"
				>
					<template #body="{ data }">
						<Button link @click="accessTokenToDelete = data">
							<i class="pi pi-trash" style="font-size: 1.2rem"></i>
						</Button>
					</template>
				</Column>
			</DataTable>

			<div class="d-flex justify-content-end mt-4">
				<Button @click="createAccessTokenDialogOpen = true"> Create Access Token </Button>
			</div>
		</template>

		<!-- Create access token dialog -->
		<Dialog
			v-model:visible="createAccessTokenDialogOpen"
			modal
			:header="resourceKey ? 'View Access Token Details' : 'Create Access Token'"
			:style="{ minWidth: '70%' }"
			@hide="handleCloseAccessTokenDialog"
		>
			<template v-if="resourceKey">
				<div style="font-size: 1.1rem">
					<div class="mb-2">Remember and store this key in a safe place.</div>
					<div style="font-weight: bold; margin-bottom: 16px">It will never be shown again.</div>
				</div>

				<SecretKeyInput
					v-model="resourceKey"
					textarea
					readonly
					placeholder="Enter API key"
					aria-labelledby="aria-api-key"
				/>
			</template>

			<template v-else>
				<template v-if="loadingCreateAccessToken">
					<div class="steps__loading-overlay">
						<LoadingGrid />
						<div>Creating access token...</div>
					</div>
				</template>

				<div v-else class="mb-4">
					<div id="aria-description" class="mb-2">Token description:</div>
					<div class="input-wrapper">
						<InputText
							v-model="accessToken.description"
							placeholder="Enter a description for this access token"
							type="text"
							class="w-100"
							aria-labelledby="aria-description"
						/>
					</div>
				</div>

				<div>
					<div id="aria-description" class="mb-2">Token expiration date:</div>
					<div class="input-wrapper">
						<Calendar
							v-model="accessToken.expiration_date"
							show-icon
							show-button-bar
							placeholder="Enter expiration date"
							type="text"
						/>
					</div>
				</div>
			</template>

			<template #footer>
				<template v-if="resourceKey">
					<!-- Close -->
					<Button
						class="sidebar-dialog__button"
						label="Close"
						text
						:disabled="loadingCreateAccessToken"
						@click="handleCloseAccessTokenDialog"
					/>

					<!-- Copy -->
					<Button
						label="Copy"
						severity="primary"
						:disabled="loadingCreateAccessToken"
						@click="handleCopyResourceKey"
					/>

					<!-- Save -->
					<Button
						label="Save"
						severity="primary"
						:disabled="loadingCreateAccessToken"
						@click="handleSaveResourceKey"
					/>
				</template>

				<template v-else>
					<!-- Cancel -->
					<Button
						class="sidebar-dialog__button"
						label="Cancel"
						text
						:disabled="loadingDeleteAccessToken"
						@click="handleCloseAccessTokenDialog"
					/>

					<!-- Create -->
					<Button @click="handleCreateAccessToken">
						<i class="pi pi-plus" style="color: var(--text-primary); margin-right: 8px"></i>
						Create access token
					</Button>
				</template>
			</template>
		</Dialog>

		<!-- Delete access token dialog -->
		<Dialog
			:visible="accessTokenToDelete !== null"
			modal
			header="Delete Access Token"
			:style="{ minWidth: '50%' }"
			:closable="false"
		>
			<div>
				Are you sure you want to delete the access token "{{ accessTokenToDelete.description }}"?
			</div>
			<template #footer>
				<!-- Cancel -->
				<Button
					class="sidebar-dialog__button"
					label="Cancel"
					text
					:disabled="loadingDeleteAccessToken"
					@click="accessTokenToDelete = null"
				/>

				<!-- Delete -->
				<Button :disabled="loadingDeleteAccessToken" @click="handleDeleteAccessToken">
					<i class="pi pi-trash" style="color: var(--text-primary); margin-right: 8px"></i>
					Delete access token
				</Button>
			</template>
		</Dialog>
	</div>
</template>

<script lang="ts">
import api from '@/js/api';
import type {
	AgentAccessToken,
	ResourceProviderGetResult,
	ResourceProviderUpsertResult,
} from '@/js/types';

export default {
	props: {
		agentName: {
			type: String,
			required: true,
		},
	},

	data() {
		return {
			loadingAccessTokens: false,
			accessTokens: [] as AgentAccessToken[],

			accessToken: null as AgentAccessToken | null,
			resourceKey: null as string | null,

			loadingCreateAccessToken: false,
			createAccessTokenDialogOpen: false,

			loadingDeleteAccessToken: false,
			accessTokenToDelete: null as AgentAccessToken | null,
		};
	},

	async created() {
		this.resetAccessToken();
	},

	watch: {
		agentName: {
			immediate: true,
			deep: true,
			async handler(newVal) {
				if (newVal) {
					await this.fetchAccessTokens();
				}
			},
		},
	},

	methods: {
		resetAccessToken() {
			var newId = this.generateGuid();
			this.accessToken = {
				id: newId,
				object_id: '',
				type: '',
				name: newId,
				display_name: '',
				description: '',
				cost_center: '',
				expiration_date: new Date(new Date().setDate(new Date().getDate() + 120)),
				active: true,
			};
			this.resourceKey = null;
		},

		generateGuid() {
			return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
				const r = (Math.random() * 16) | 0,
					v = c === 'x' ? r : (r & 0x3) | 0x8;
				return v.toString(16);
			});
		},

		async fetchAccessTokens() {
			this.loadingAccessTokens = true;
			const accessTokensResponse = await api.getAgentAccessTokens(this.agentName);
			this.accessTokens = (
				accessTokensResponse as ResourceProviderGetResult<AgentAccessToken>[]
			).map((result: ResourceProviderGetResult<AgentAccessToken>) => result.resource);
			this.loadingAccessTokens = false;
		},

		handleCloseAccessTokenDialog() {
			this.createAccessTokenDialogOpen = false;
			this.resetAccessToken();
		},

		handleCopyResourceKey() {
			if (this.resourceKey !== null) {
				navigator.clipboard.writeText(this.resourceKey);
				this.$toast.add({
					severity: 'success',
					detail: 'Access token key copied to clipboard!',
					life: 5000,
				});
			} else {
				this.$toast.add({
					severity: 'error',
					summary: 'Error',
					detail: 'Failed to copy access token key. Please try again.',
					life: 5000,
				});
			}
		},

		handleSaveResourceKey() {
			if (this.resourceKey === null) {
				return;
			}
			const link = document.createElement('a');
			const file = new Blob([this.resourceKey], { type: 'text/plain' });
			link.href = URL.createObjectURL(file);
			link.download = `${this.agentName}-${this.accessToken?.description}-key.txt`.replace(
				/\s+/g,
				'-',
			);
			link.click();
			URL.revokeObjectURL(link.href);
		},

		async handleDeleteAccessToken() {
			this.loadingDeleteAccessToken = true;
			try {
				if (!this.accessTokenToDelete) {
					this.$toast.add({
						severity: 'error',
						summary: 'Error',
						detail: 'Failed to delete access token. Please try again.',
						life: 5000,
					});
					return;
				}

				await api.deleteAgentAccessToken(this.agentName, this.accessTokenToDelete.name);

				this.$toast.add({
					severity: 'success',
					summary: 'Success',
					detail: 'Successfully deleted access token.',
					life: 5000,
				});

				this.loadingDeleteAccessToken = false;
				this.accessTokenToDelete = null;
				await this.fetchAccessTokens();
			} catch (error) {
				this.$toast.add({
					severity: 'error',
					summary: 'Error',
					detail: 'Failed to delete access token. Please try again.',
					life: 5000,
				});
			}
			this.loadingDeleteAccessToken = false;
			this.accessTokenToDelete = null;
		},

		async handleCreateAccessToken() {
			const errors = [];
			if (!this.accessToken?.description) {
				errors.push('Please give the access token a description.');
			}

			if (!this.accessToken?.expiration_date) {
				errors.push('Please make sure the access token has an expiration date.');
			}

			if (errors.length > 0) {
				this.$toast.add({
					severity: 'error',
					detail: errors.join('\n'),
					life: 5000,
				});

				return;
			}

			this.loadingCreateAccessToken = true;
			try {
				if (!this.accessToken) {
					this.$toast.add({
						severity: 'error',
						summary: 'Error',
						detail: 'Failed to create access token. Please try again.',
						life: 5000,
					});
					return;
				}

				const result = (await api.createAgentAccessToken(
					this.agentName,
					this.accessToken,
				)) as ResourceProviderUpsertResult;
				this.resourceKey = result.resource;

				this.$toast.add({
					severity: 'success',
					summary: 'Success',
					detail: 'Successfully created access token.',
					life: 5000,
				});

				this.loadingCreateAccessToken = false;
				await this.fetchAccessTokens();
			} catch (error) {
				this.$toast.add({
					severity: 'error',
					summary: 'Error',
					detail: 'Failed to create access token. Please try again.',
					life: 5000,
				});
			}
			this.loadingCreateAccessToken = false;
		},
	},
};
</script>
