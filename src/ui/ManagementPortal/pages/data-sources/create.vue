<template>
	<main id="main-content">
		<div style="display: flex">
			<!-- Title -->
			<div style="flex: 1">
				<h2 class="page-header">{{ editId ? 'Edit Data Source' : 'Create Data Source' }}</h2>
				<div class="page-subheader">
					{{
						editId
							? 'Edit your data source settings below.'
							: 'Complete the settings below to configure the data source.'
					}}
				</div>
			</div>

			<!-- Edit access control -->
			<AccessControl
				v-if="editId"
				:scope="`providers/FoundationaLLM.DataSource/dataSources/${dataSource.name}`"
			/>
		</div>

		<!-- Steps -->
		<div class="steps" :class="{ 'steps--loading': loading }">
			<!-- Loading overlay -->
			<template v-if="loading">
				<div class="steps__loading-overlay" role="status" aria-live="polite">
					<LoadingGrid />
					<div>{{ loadingStatusText }}</div>
				</div>
			</template>

			<!-- Name -->
			<div class="step-header span-2">What is the name of the data source?</div>
			<div class="span-2">
				<div id="aria-source-name" class="mb-2">Data source name:</div>
				<div id="aria-source-name-desc" class="mb-2">
					No special characters or spaces, use letters and numbers with dashes and underscores only.
				</div>
				<div class="input-wrapper">
					<InputText
						v-model="dataSource.name"
						:disabled="editId"
						type="text"
						class="w-100"
						placeholder="Enter data source name"
						aria-labelledby="aria-source-name aria-source-name-desc"
						@input="handleNameInput"
					/>
					<span
						v-if="nameValidationStatus === 'valid'"
						class="icon valid"
						title="Name is available"
						aria-label="Name is available"
					>
						✔️
					</span>
					<span
						v-else-if="nameValidationStatus === 'invalid'"
						:title="validationMessage"
						class="icon invalid"
						:aria-label="validationMessage"
					>
						❌
					</span>
				</div>

				<div id="aria-data-desc" class="mb-2 mt-2">Data description:</div>
				<div class="input-wrapper">
					<InputText
						v-model="dataSource.description"
						type="text"
						class="w-100"
						placeholder="Enter a description for this data source"
						aria-labelledby="aria-data-desc"
					/>
				</div>
			</div>

			<!-- Type -->
			<div id="aria-source-type" class="step-header span-2">
				What is the type of the data source?
			</div>
			<div class="span-2">
				<Dropdown
					v-model="dataSource.type"
					:options="sourceTypeOptions"
					option-label="label"
					option-value="value"
					class="dropdown--agent"
					placeholder="--Select--"
					aria-labelledby="aria-source-type"
				/>
			</div>

			<!-- Connection details -->
			<!-- Show this section only if a source type is selected -->
			<div v-if="dataSource.type" class="span-2">
				<div class="step-header mb-2">What are the connection details?</div>

				<!-- Azure data lake -->
				<div v-if="isAzureDataLakeDataSource(dataSource)">
					<div id="aria-auth-type" class="mb-2">Authentication type:</div>
					<Dropdown
						v-model="dataSource.resolved_configuration_references.AuthenticationType"
						:options="authenticationTypeOptions"
						option-label="label"
						option-value="value"
						class="dropdown--agent"
						placeholder="--Select--"
						aria-labelledby="aria-auth-type"
					/>

					<!-- Connection string -->
					<div
						v-if="
							dataSource.resolved_configuration_references.AuthenticationType === 'ConnectionString'
						"
						class="span-2"
					>
						<div id="aria-connection-string" class="mb-2 mt-2">Connection string:</div>
						<SecretKeyInput
							v-model="dataSource.resolved_configuration_references.ConnectionString"
							textarea
							placeholder="Enter connection string"
							aria-labelledby="aria-connection-string"
						/>
					</div>

					<!-- API Key -->
					<div
						v-if="dataSource.resolved_configuration_references.AuthenticationType === 'AccountKey'"
						class="span-2"
					>
						<div id="aria-api-key" class="mb-2 mt-2">API Key:</div>
						<SecretKeyInput
							v-model="dataSource.resolved_configuration_references.APIKey"
							placeholder="Enter API key"
							aria-labelledby="aria-api-key"
						/>

						<div id="aria-endpoint" class="mb-2 mt-2">Endpoint:</div>
						<InputText
							v-model="dataSource.resolved_configuration_references.Endpoint"
							class="w-100"
							type="text"
							placeholder="Enter API endpoint"
							aria-labelledby="aria-endpoint"
						/>
					</div>

					<!-- Account name -->
					<div
						v-if="
							dataSource.resolved_configuration_references.AuthenticationType === 'AzureIdentity'
						"
						class="span-2"
					>
						<div id="aria-account-name" class="mb-2 mt-2">Account name:</div>
						<InputText
							v-model="dataSource.resolved_configuration_references.AccountName"
							class="w-100"
							type="text"
							placeholder="Enter account name"
							aria-labelledby="aria-account-name"
						/>
					</div>

					<div id="aria-folders" class="step-header mb-2 mt-2">Folder(s):</div>
					<div id="aria-folders-desc" class="mb-2">
						Press <strong>Enter</strong> or <strong>,</strong> after typing each folder name.
					</div>
					<Chips
						v-model="folders"
						v-create-chip-on-blur:folders
						class="w-100"
						separator=","
						aria-labelledby="aria-folders"
						aria-describedby="aria-folders-desc"
						:pt="{ input: { 'aria-labelledby': 'aria-folders aria-folders-desc' } }"
					/>
				</div>

				<!-- OneLake -->
				<div v-if="isOneLakeDataSource(dataSource)">
					<div id="aria-auth-type" class="mb-2">Authentication type:</div>
					<Dropdown
						v-model="dataSource.resolved_configuration_references.AuthenticationType"
						:options="authenticationTypeOptions"
						option-label="label"
						option-value="value"
						class="dropdown--agent"
						placeholder="--Select--"
						aria-labelledby="aria-auth-type"
					/>

					<!-- Connection string -->
					<div
						v-if="
							dataSource.resolved_configuration_references.AuthenticationType === 'ConnectionString'
						"
						class="span-2"
					>
						<div id="aria-connection-string" class="mb-2 mt-2">Connection string:</div>
						<SecretKeyInput
							v-model="dataSource.resolved_configuration_references.ConnectionString"
							textarea
							placeholder="Enter connection string"
							aria-labelledby="aria-connection-string"
						/>
					</div>

					<!-- API Key -->
					<div
						v-if="dataSource.resolved_configuration_references.AuthenticationType === 'AccountKey'"
						class="span-2"
					>
						<div id="aria-api-key" class="mb-2 mt-2">API Key:</div>
						<SecretKeyInput
							v-model="dataSource.resolved_configuration_references.APIKey"
							placeholder="Enter API key"
							aria-labelledby="aria-api-key"
						/>

						<div id="aria-endpoint" class="mb-2 mt-2">Endpoint:</div>
						<InputText
							v-model="dataSource.resolved_configuration_references.Endpoint"
							class="w-100"
							type="text"
							placeholder="Enter API endpoint"
							aria-labelledby="aria-endpoint"
						/>
					</div>

					<!-- Account name -->
					<div
						v-if="
							dataSource.resolved_configuration_references.AuthenticationType === 'AzureIdentity'
						"
						class="span-2"
					>
						<div id="aria-account-name" class="mb-2 mt-2">Account name:</div>
						<InputText
							v-model="dataSource.resolved_configuration_references.AccountName"
							class="w-100"
							type="text"
							placeholder="Enter account name"
							aria-labelledby="aria-account-name"
						/>
					</div>

					<div id="aria-workspaces" class="step-header mb-2 mt-2">Workspace(s):</div>
					<div id="aria-workspaces-desc" class="mb-2">
						Press <strong>Enter</strong> or <strong>,</strong> after typing each workspace name.
					</div>
					<Chips
						v-model="workspaces"
						v-create-chip-on-blur:workspaces
						class="w-100"
						separator=","
						aria-labelledby="aria-workspaces aria-workspaces-desc"
						:pt="{ input: { 'aria-labelledby': 'aria-workspaces aria-workspaces-desc' } }"
					/>
				</div>

				<!-- Azure SQL database -->
				<div v-if="isAzureSQLDatabaseDataSource(dataSource)">
					<!-- Connection string -->
					<div class="span-2">
						<div id="aria-connection-string" class="mb-2">Connection string:</div>
						<SecretKeyInput
							v-model="dataSource.resolved_configuration_references.ConnectionString"
							textarea
							placeholder="Enter connection string"
							aria-labelledby="aria-connection-string"
						/>

						<template v-if="dataSource.tables">
							<div id="aria-table-names" class="step-header mb-2 mt-2">Table Name(s):</div>
							<div id="aria-table-names-desc" class="mb-2">
								Press <strong>Enter</strong> or <strong>,</strong> after typing each table name.
							</div>
							<Chips
								v-model="tables"
								v-create-chip-on-blur:tables
								class="w-100"
								separator=","
								aria-labelledby="aria-table-names aria-table-names-desc"
								:pt="{ input: { 'aria-labelledby': 'aria-table-names aria-table-names-desc' } }"
							/>
						</template>
					</div>
				</div>

				<!-- Sharepoint online -->
				<div v-if="isSharePointOnlineSiteDataSource(dataSource)">
					<div class="span-2">
						<div id="aria-client-id" class="mb-2">App ID (Client ID):</div>
						<InputText
							v-model="dataSource.resolved_configuration_references.ClientId"
							class="w-100"
							type="text"
							placeholder="Enter app ID (client ID)"
							aria-labelledby="aria-client-id"
						/>

						<div id="aria-tenant-id" class="mb-2 mt-2">Tenant ID:</div>
						<InputText
							v-model="dataSource.resolved_configuration_references.TenantId"
							class="w-100"
							type="text"
							placeholder="Enter tenant ID"
							aria-labelledby="aria-tenant-id"
						/>

						<div id="aria-cert-name" class="mb-2 mt-2">Certificate Name:</div>
						<InputText
							v-model="dataSource.resolved_configuration_references.CertificateName"
							class="w-100"
							type="text"
							placeholder="Enter certificate name"
							aria-labelledby="aria-cert-name"
						/>

						<div id="aria-key-vault" class="mb-2 mt-2">Key Vault URL:</div>
						<InputText
							v-model="dataSource.resolved_configuration_references.KeyVaultURL"
							class="w-100"
							type="text"
							placeholder="Enter key vault URL"
							aria-labelledby="aria-key-vault"
						/>

						<div id="aria-site-url" class="mb-2 mt-2">Site URL:</div>
						<InputText
							v-model="dataSource.site_url"
							class="w-100"
							type="text"
							placeholder="Enter site URL"
							aria-labelledby="aria-site-url"
						/>

						<template v-if="dataSource.document_libraries">
							<div id="aria-document-libs" class="step-header mb-2 mt-2">Document Library(s):</div>
							<div id="aria-document-libs-desc" class="mb-2">
								Press <strong>Enter</strong> or <strong>,</strong> after typing each document
								library name.
							</div>
							<Chips
								v-model="documentLibraries"
								v-create-chip-on-blur:documentLibraries
								class="w-100"
								separator=","
								aria-labelledby="aria-document-libs aria-document-libs-desc"
								:pt="{ input: { 'aria-labelledby': 'aria-document-libs aria-document-libs-desc' } }"
							/>
						</template>
					</div>
				</div>
			</div>

			<div id="aria-cost-center" class="step-header span-2">
				Would you like to assign this data source to a cost center?
			</div>
			<div class="span-2">
				<InputText
					v-model="dataSource.cost_center"
					type="text"
					class="w-50"
					placeholder="Enter cost center name"
					aria-labelledby="aria-cost-center"
				/>
			</div>

			<!-- Buttons -->
			<div class="button-container column-2 justify-self-end">
				<!-- Create data source -->
				<Button
					:label="editId ? 'Save Changes' : 'Create Data Source'"
					severity="primary"
					@click="handleCreateDataSource"
				/>

				<!-- Cancel -->
				<Button
					v-if="editId"
					style="margin-left: 16px"
					label="Cancel"
					severity="secondary"
					@click="handleCancel"
				/>
			</div>
		</div>
	</main>
</template>

<script lang="ts">
import type { PropType } from 'vue';
import { debounce } from 'lodash';
import api from '@/js/api';
import type {
	DataSource,
	ConfigurationReferenceMetadata,
	// AzureDataLakeDataSource,
	// SharePointOnlineSiteDataSource,
	// AzureSQLDatabaseDataSource,
} from '@/js/types';
import {
	isAzureDataLakeDataSource,
	isOneLakeDataSource,
	isAzureSQLDatabaseDataSource,
	isSharePointOnlineSiteDataSource,
	convertToDataSource,
} from '@/js/types';

export default {
	name: 'CreateDataSource',

	props: {
		editId: {
			type: [Boolean, String] as PropType<false | string>,
			required: false,
			default: false,
		},
	},

	data() {
		return {
			accessControlModalOpen: false,

			loading: false as boolean,
			loadingStatusText: 'Retrieving data...' as string,

			nameValidationStatus: null as string | null, // 'valid', 'invalid', or null
			validationMessage: null as string | null,

			folders: [] as string[],
			workspaces: [] as string[],
			documentLibraries: [] as string[],
			tables: [] as string[],

			// Create a default Azure Data Lake data source.
			dataSource: {
				type: 'azure-data-lake',
				name: '',
				display_name: '',
				object_id: '',
				description: '',
				cost_center: '',
				resolved_configuration_references: {
					AuthenticationType: '',
					ConnectionString: '',
					APIKey: '',
					Endpoint: '',
					AccountName: '',
				},
				configuration_references: {
					AuthenticationType: '',
					ConnectionString: '',
					APIKey: '',
					Endpoint: '',
					AccountName: '',
				},
				configuration_reference_metadata: {} as { [key: string]: ConfigurationReferenceMetadata },
			} as null | DataSource,

			sourceTypeOptions: [
				{
					label: 'OneLake',
					value: 'onelake',
				},
				{
					label: 'Azure Data Lake',
					value: 'azure-data-lake',
				},
				{
					label: 'Azure SQL Database',
					value: 'azure-sql-database',
				},
				{
					label: 'SharePoint List',
					value: 'sharepoint-online-site',
				},
			],

			authenticationTypeOptions: [
				{
					label: 'Connection String',
					value: 'ConnectionString',
				},
				{
					label: 'Account Key',
					value: 'AccountKey',
				},
				{
					label: 'Azure Identity',
					value: 'AzureIdentity',
				},
			],
		};
	},

	watch: {
		'dataSource.type'() {
			this.dataSource = convertToDataSource(this.dataSource);
		},
	},

	async created() {
		this.loading = true;

		if (this.editId) {
			this.loadingStatusText = `Retrieving data source "${this.editId}"...`;
			const dataSourceResult = await api.getDataSource(this.editId);
			const dataSource = dataSourceResult.resource;
			this.dataSource = dataSource;

			if (this.dataSource.folders) {
				this.folders = this.dataSource.folders;
			}
			if (this.dataSource.workspaces) {
				this.workspaces = this.dataSource.workspaces;
			}
			if (this.dataSource.document_libraries) {
				this.documentLibraries = this.dataSource.document_libraries;
			}
			if (this.dataSource.tables) {
				this.tables = this.dataSource.tables;
			}
		} else {
			// Create a new DataSource object of type Azure Data Lake.
			const newDataSource: DataSource = {
				type: 'azure-data-lake',
				name: '',
				display_name: '',
				object_id: '',
				description: '',
				cost_center: '',
				resolved_configuration_references: {
					AuthenticationType: '',
					ConnectionString: '',
					APIKey: '',
					Endpoint: '',
					AccountName: '',
				},
				configuration_references: {
					AuthenticationType: '',
					ConnectionString: '',
					APIKey: '',
					Endpoint: '',
					AccountName: '',
				},
				configuration_reference_metadata: {} as { [key: string]: ConfigurationReferenceMetadata },
			};
			this.dataSource = convertToDataSource(newDataSource);
		}

		this.debouncedCheckName = debounce(this.checkName, 500);

		this.loading = false;
	},

	methods: {
		isAzureDataLakeDataSource,
		isOneLakeDataSource,
		isSharePointOnlineSiteDataSource,
		isAzureSQLDatabaseDataSource,
		convertToDataSource,

		async checkName() {
			try {
				const response = await api.checkDataSourceName(this.dataSource.name, this.dataSource.type);

				// Handle response based on the status
				if (response.status === 'Allowed') {
					// Name is available
					this.nameValidationStatus = 'valid';
					this.validationMessage = null;
				} else if (response.status === 'Denied') {
					// Name is taken
					this.nameValidationStatus = 'invalid';
					this.validationMessage = response.message;
				}
			} catch (error) {
				console.error('Error checking data source name: ', error);
				this.nameValidationStatus = 'invalid';
				this.validationMessage = 'Error checking the data source name. Please try again.';
			}
		},

		handleCancel() {
			if (!confirm('Are you sure you want to cancel?')) {
				return;
			}

			this.$router.push('/data-sources');
		},

		handleNameInput(event) {
			const sanitizedValue = this.$filters.sanitizeNameInput(event);
			this.dataSource.name = sanitizedValue;

			// Check if the name is available if we are creating a new data source.
			if (!this.editId) {
				this.debouncedCheckName();
			}
		},

		async handleCreateDataSource() {
			const errors: string[] = [];
			if (!this.dataSource.name) {
				errors.push('Please give the data source a name.');
			}
			if (this.nameValidationStatus === 'invalid') {
				errors.push(this.validationMessage);
			}

			if (!this.dataSource.type) {
				errors.push('Please specify a data source type.');
			}

			this.dataSource = convertToDataSource(this.dataSource);

			// Convert string representations of array fields back to arrays.
			if (isAzureDataLakeDataSource(this.dataSource)) {
				this.dataSource.folders = this.folders;
			} else if (isOneLakeDataSource(this.dataSource)) {
				this.dataSource.workspaces = this.workspaces;
			} else if (isSharePointOnlineSiteDataSource(this.dataSource)) {
				this.dataSource.document_libraries = this.documentLibraries;
			} else if (isAzureSQLDatabaseDataSource(this.dataSource)) {
				this.dataSource.tables = this.tables;
			}

			if (errors.length > 0) {
				this.$toast.add({
					severity: 'error',
					detail: errors.join('\n'),
					life: 5000,
				});

				return;
			}

			this.loading = true;
			let successMessage = null as null | string;
			try {
				this.loadingStatusText = 'Saving data source...';
				await api.upsertDataSource(this.dataSource);
				successMessage = `Data source "${this.dataSource.name}" was successfully saved.`;
			} catch (error) {
				this.loading = false;
				return this.$toast.add({
					severity: 'error',
					detail: error?.response?._data || error,
					life: 5000,
				});
			}

			this.$toast.add({
				severity: 'success',
				detail: successMessage,
				life: 5000,
			});

			this.loading = false;

			if (!this.editId) {
				this.$router.push('/data-sources');
			}
		},
	},
};
</script>

<style lang="scss">
.steps {
	display: grid;
	grid-template-columns: minmax(auto, 50%) minmax(auto, 50%);
	gap: 24px;
	position: relative;
}

.steps--loading {
	pointer-events: none;
}

.steps__loading-overlay {
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

.step-section-header {
	background-color: rgba(150, 150, 150, 1);
	color: white;
	font-size: 1rem;
	font-weight: 600;
	padding: 16px;
}

.step-header {
	font-weight: bold;
	margin-bottom: -10px;
}

.step {
	// display: flex;
	// flex-direction: column;
}

.step--disabled {
	pointer-events: none;
	opacity: 0.5;
}

.step-container {
	// padding: 16px;
	border: 2px solid #e1e1e1;
	flex-grow: 1;
	position: relative;

	&:hover {
		background-color: rgba(217, 217, 217, 0.4);
	}

	&__header {
		font-weight: bold;
		margin-bottom: 8px;
	}
}

.step-container__view {
	// padding: 16px;
	height: 100%;
	display: flex;
	flex-direction: row;
}

.step-container__view__inner {
	padding: 16px;
	flex-grow: 1;
	word-break: break-word;
}

.step-container__view__arrow {
	background-color: #e1e1e1;
	color: rgb(150, 150, 150);
	width: 40px;
	min-width: 40px;
	display: flex;
	justify-content: center;
	align-items: center;

	&:hover {
		background-color: #cacaca;
	}
}

$editStepPadding: 16px;
.step-container__edit {
	border: 2px solid #e1e1e1;
	position: absolute;
	width: calc(100% + 4px);
	background-color: white;
	top: -2px;
	left: -2px;
	z-index: 5;
	box-shadow: 0 5px 20px 0 rgba(27, 29, 33, 0.2);
	min-height: calc(100% + 4px);
	// padding: $editStepPadding;
	display: flex;
	flex-direction: row;
}

.step-container__edit__inner {
	padding: $editStepPadding;
	flex-grow: 1;
}

.step-container__edit__arrow {
	background-color: #e1e1e1;
	color: rgb(150, 150, 150);
	min-width: 40px;
	width: 40px;
	display: flex;
	justify-content: center;
	align-items: center;
	transform: rotate(180deg);

	&:hover {
		background-color: #cacaca;
	}
}

.step-container__edit-dropdown {
	border: 2px solid #e1e1e1;
	position: absolute;
	width: calc(100% + 4px);
	background-color: white;
	top: -2px;
	left: -2px;
	z-index: 5;
	box-shadow: 0 5px 20px 0 rgba(27, 29, 33, 0.2);
	display: flex;
	flex-direction: column;
	min-height: calc(100% + 4px);
}

.step-container__edit__header {
	padding: $editStepPadding;
}

.step-container__edit__group-header {
	font-weight: bold;
	padding: $editStepPadding;
	padding-bottom: 0px;
}

.step-container__edit__option {
	padding: $editStepPadding;
	word-break: break-word;
	&:hover {
		background-color: rgba(217, 217, 217, 0.4);
	}
}

// .step-container__edit__option + .step-container__edit__option{
// 	border-top: 2px solid #e1e1e1;
// }

.step-container__edit__option--selected {
	// outline: 2px solid #e1e1e1;
	// background-color: rgba(217, 217, 217, 0.4);
}

.step__radio {
	display: flex;
	gap: 10px;
}

.step-option__header {
	text-decoration: underline;
	margin-right: 8px;
}

.input-wrapper {
	position: relative;
	display: flex;
	align-items: center;
}

input {
	width: 100%;
	padding-right: 30px;
}

.icon {
	position: absolute;
	right: 10px;
	cursor: default;
}

.valid {
	color: green;
}

.invalid {
	color: red;
}

.flex-container {
	display: flex;
	align-items: center; /* Align items vertically in the center */
}

.flex-item {
	flex-grow: 1; /* Allow the textarea to grow and fill the space */
}

.flex-item-button {
	margin-left: 8px; /* Add some space between the textarea and the button */
}

.p-chips {
	ul {
		width: 100%;
		li {
			input {
				width: 100% !important;
			}
		}
	}
}
</style>
