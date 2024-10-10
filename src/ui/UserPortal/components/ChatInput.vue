<template>
	<div class="chat-input p-inputgroup" role="group" aria-label="Chat input group">
		<div class="input-wrapper">
			<div class="tooltip-component">
				<VTooltip :auto-hide="isMobile" :popper-triggers="isMobile ? [] : ['hover']">
					<i
						class="pi pi-info-circle"
						tabindex="0"
						aria-label="info icon"
						@keydown.esc="hideAllPoppers"
					></i>
					<template #popper>
						<div role="tooltip">Use Shift+Enter to add a new line</div>
					</template>
				</VTooltip>
			</div>
			<VTooltip :auto-hide="isMobile" :popper-triggers="isMobile ? [] : ['hover']">
				<Button
					ref="fileUploadButton"
					type="button"
					:badge="fileArrayFiltered.length.toString() || null"
					:aria-label="'Upload file (' + fileArrayFiltered.length.toString() + ' files attached)'"
					icon="pi pi-paperclip"
					class="file-upload-button"
					severity="secondary"
					aria-controls="overlay_menu"
					aria-haspopup="true"
					style="height: 100%"
					@click="toggle"
					@keydown.esc="hideAllPoppers"
				/>
				<OverlayPanel
					ref="menu"
					:dismissable="false"
					style="max-width: 98%">
					<div class="file-upload-header">
						<Button
							:icon="!isMobile ? 'pi pi-times' : undefined"
							label="Close"
							class="file-upload-container-button"
							@click="toggle"
						/>
					</div>
					<FileUpload
						ref="fileUpload"
						:multiple="true"
						:auto="false"
						:custom-upload="true"
						@uploader="handleUpload"
						@select="fileSelected"
					>
						<template #content>
							<div v-if="fileArrayFiltered.length === 0 && oneDriveFiles.length === 0 && localFiles.length === 0" class="file-upload-empty-desktop">
								<p>No files have been added to this message.</p>
							</div>
							<!-- Progress bar -->
							<div v-if="isUploading" style="padding: 60px 10px">
								<ProgressBar
									:value="uploadProgress"
									:show-value="false"
									style="display: flex; width: 95%; margin: 10px 2.5%"
								/>
								<p style="text-align: center">Uploading...</p>
							</div>

							<!-- File list -->
							<div v-else class="file-upload-file-container">
								<div
									v-for="file in fileArrayFiltered"
									:key="file.fileName"
									class="file-upload-file"
								>
									<div class="file-upload-file_info">
										<i
											v-if="!isMobile"
											class="pi pi-file"
											style="font-size: 2rem; margin-right: 1rem"
										></i>
										<span style="font-weight: 600">{{ file.fileName }}</span>
									</div>
									<div style="display: flex; align-items: center; margin-left: 10px; gap: 0.5rem">
										<Badge v-if="!isMobile" :value="file.source" />
										<Badge value="Uploaded" severity="success" />
										<Button
											icon="pi pi-times"
											text
											severity="danger"
											aria-label="Delete attachment"
											@click="fileToDelete = { name: file.fileName, type: 'attachment', file: file }"
										/>
									</div>
								</div>
								<Divider v-if="fileArrayFiltered.length > 0" />
								<div
									v-for="(file) of localFiles"
									:key="file.name + file.type + file.size"
									class="file-upload-file"
								>
									<div class="file-upload-file_info">
										<i
											v-if="!isMobile"
											class="pi pi-file"
											style="font-size: 2rem; margin-right: 1rem"
										></i>
										<span style="font-weight: 600">{{ file.name }}</span>
										<div v-if="!isMobile">{{ formatSize(file.size) }}</div>
									</div>
									<div style="display: flex; align-items: center; margin-left: 10px; gap: 0.5rem">
										<Badge v-if="!isMobile" value="Local Computer" />
										<Badge value="Pending" />
										<Button
											icon="pi pi-times"
											text
											severity="danger"
											aria-label="Remove file"
											@click="fileToDelete = { name: file.name, type: 'local', file: file }"
										/>
									</div>
								</div>
								<div v-if="oneDriveFiles && oneDriveFiles.length > 0">
									<div
										v-for="(file) of oneDriveFiles"
										:key="file.name + file.size"
										class="file-upload-file"
									>
										<div class="file-upload-file_info">
											<i
												v-if="!isMobile"
												class="pi pi-file"
												style="font-size: 2rem; margin-right: 1rem"
											></i>
											<span style="font-weight: 600">{{ file.name }}</span>
											<div v-if="!isMobile">{{ formatSize(file.size) }}</div>
										</div>
										<div style="display: flex; align-items: center; margin-left: 10px; gap: 0.5rem">
											<Badge v-if="!isMobile" value="OneDrive Work/School" />
											<Badge value="Pending" />
											<Button
												icon="pi pi-times"
												text
												severity="danger"
												aria-label="Remove file"
												@click="fileToDelete = { name: file.name, type: 'oneDrive', file: file }"
											/>
										</div>
									</div>
								</div>
							</div>
							<div
								v-if="oneDriveFiles.length > 0 || localFiles.length > 0"
								class="file-upload-button-container"
							>
								<Button
									icon="pi pi-upload"
									label="Upload"
									class="file-upload-container-button"
									style="margin-top: 0.5rem"
									:disabled="isUploading || (localFiles.length === 0 && oneDriveFiles.length === 0)"
									@click="handleUpload"
								/>
							</div>
						</template>
					</FileUpload>
					<Divider v-if="oneDriveFiles.length > 0 || localFiles.length > 0" />
					<div class="file-overlay-panel__footer">
						<Button
							:icon="!isMobile ? 'pi pi-file-plus' : undefined"
							label="Select file from Computer"
							class="file-upload-container-button"
							@click="browseFiles"
						/>
						<template v-if="$appStore.oneDriveWorkSchool">
							<Button
								label="Select file from OneDrive"
								class="file-upload-container-button"
								:icon="!isMobile ? 'pi pi-cloud-upload' : undefined"
								:disabled="disconnectingOneDrive"
								:loading="oneDriveBaseURL === null"
								@click="oneDriveWorkSchoolDownload"
							/>
							<Button
								label="Disconnect OneDrive"
								class="file-upload-container-button"
								:icon="!isMobile ? 'pi pi-sign-out' : undefined"
								:loading="disconnectingOneDrive"
								@click="oneDriveWorkSchoolDisconnect"
							/>
						</template>
						<template v-else>
							<Button
								label="Connect to OneDrive"
								class="file-upload-container-button"
								:icon="!isMobile ? 'pi pi-sign-in' : undefined"
								:loading="connectingOneDrive || $appStore.oneDriveWorkSchool === null"
								@click="connectOneDriveWorkSchool"
							/>
						</template>
					</div>
				</OverlayPanel>
				<template #popper>
					<div role="tooltip">
						Attach files ({{
							fileArrayFiltered.length === 1 ? '1 file' : fileArrayFiltered.length + ' files'
						}})
					</div>
				</template>
			</VTooltip>
			<Dialog
				v-if="fileToDelete !== null"
				v-focustrap
				:visible="fileToDelete !== null"
				:closable="false"
				modal
				:header="fileToDelete.type === 'local' | 'oneDrive' ? 'Remove a file' : 'Delete a file'"
				@keydown="deleteFileKeydown"
			>
				<div v-if="deleteFileProcessing" class="delete-dialog-content">
					<div role="status">
						<i
							class="pi pi-spin pi-spinner"
							style="font-size: 2rem"
							role="img"
							aria-label="Loading"
						></i>
					</div>
				</div>
				<div v-else>
					<p>Do you want to {{ fileToDelete.type === "local" | "oneDrive" ? "remove" : "delete" }} the file "{{ fileToDelete.name }}" ?</p>
				</div>
				<template #footer>
					<Button label="Cancel" text :disabled="deleteFileProcessing" @click="fileToDelete = null" />
					<Button
						:label="fileToDelete.type === 'local' | 'oneDrive' ? 'Remove' : 'Delete'"
						severity="danger"
						autofocus
						:disabled="deleteFileProcessing"
						@click="removeDialogFile"
					/>
				</template>
			</Dialog>
			<Dialog
				v-model:visible="showOneDriveIframeDialog"
				modal
				aria-label="OneDrive File Picker Dialog"
				style="max-width: 98%; min-width: 50%; max-height: 98%"
				class="onedrive-iframe-dialog"
			>
				<div id="oneDriveIframeDialogContent" class="onedrive-iframe-content" />
			</Dialog>
			<Mentionable
				:keys="['@']"
				:items="agents"
				offset="6"
				:limit="1000"
				insert-space
				class="mentionable"
				@keydown.enter.prevent
				@open="agentListOpen = true"
				@close="agentListOpen = false"
			>
				<p id="chat-input-label" class="sr-only">
					The agent can make mistakes. Please check important information carefully.
				</p>
				<textarea
					id="chat-input"
					ref="inputRef"
					v-model="text"
					class="input"
					:disabled="disabled"
					placeholder="What would you like to ask?"
					autofocus
					aria-label="What would you like to ask?"
					aria-describedby="chat-input-label"
					@keydown="handleKeydown"
				/>
				<template #no-result>
					<div class="dim">No result</div>
				</template>

				<template #item="{ item }">
					<div class="user">
						<span class="dim">
							{{ item.label }}
						</span>
					</div>
				</template>
			</Mentionable>
		</div>
		<Button
			:disabled="disabled"
			class="submit"
			icon="pi pi-send"
			label="Send"
			@click="handleSend"
		/>
	</div>
</template>

<script lang="ts">
import { Mentionable } from 'vue-mention';
import 'floating-vue/dist/style.css';
import { hideAllPoppers } from 'floating-vue';

export default {
	name: 'ChatInput',

	components: {
		Mentionable,
	},

	props: {
		disabled: {
			type: Boolean,
			required: false,
			default: false,
		},
	},

	emits: ['send'],

	data() {
		return {
			text: '' as string,
			targetRef: null as HTMLElement | null,
			inputRef: null as HTMLElement | null,
			agents: [],
			agentListOpen: false,
			showFileUploadDialog: false,
			showOneDriveIframeDialog: false,
			isUploading: false,
			uploadProgress: 0,
			isMobile: window.screen.width < 950,
			win: null as any,
			port: null as any,
			filePickerParams: {
				sdk: '8.0',
				entry: {
					oneDrive: {
						files: {},
					},
				},
				authentication: {},
				messaging: {
					origin: document.location.origin,
					channelId: '27',
				},
				typesAndSources: {
					mode: 'files',
					pivots: {
						oneDrive: true,
						recent: false,
						shared: false,
						sharedLibraries: true,
						myOrganization: true,
						favorites: true,
					},
				},
				access: { mode: 'read' },
				search: { enabled: true },
				selection: {
					mode: 'multiple',
				},
			},
			oneDriveFiles: [] as any[],
			localFiles: [] as any[],
			oneDriveBaseURL: null as string | null,
			disconnectingOneDrive: false,
			fileToDelete: null as any,
			deleteFileProcessing: false,
			connectingOneDrive: true,
			maxFiles: 10,
		};
	},

	computed: {
		fileArrayFiltered() {
			return this.$appStore.attachments.filter(
				(attachment) => attachment.sessionId === this.$appStore.currentSession.sessionId,
			);
		},
	},

	watch: {
		text: {
			handler() {
				this.adjustTextareaHeight();
			},
			immediate: true,
		},
		disabled: {
			handler(newValue) {
				if (!newValue) {
					this.$nextTick(() => {
						const textInput = this.$refs.inputRef as HTMLTextAreaElement;
						textInput.focus();
					});
				}
			},
			immediate: true,
		},
	},

	async created() {
		if (localStorage.getItem('oneDriveWorkSchoolConsentRedirect') === 'true') {
			await this.oneDriveWorkSchoolConnect();
			localStorage.setItem('oneDriveWorkSchoolConsentRedirect', JSON.stringify(false));
		}else{
			this.connectingOneDrive = false;
		}

		await this.$appStore.getFileStoreConfiguration();
		await this.$appStore.getAgents();

		this.agents = this.$appStore.agents.map((agent) => ({
			label: agent.resource.name,
			value: agent.resource.name,
		}));

		this.oneDriveBaseURL = this.$appStore.fileStoreConfiguration.fileStoreConnectors?.find(
			(connector) => connector.subcategory === 'OneDriveWorkSchool',
		)?.url;

		if (this.$appStore.fileStoreConfiguration.maxUploadsPerMessage) {
			this.maxFiles = this.$appStore.fileStoreConfiguration.maxUploadsPerMessage;
		}
	},

	mounted() {
		this.adjustTextareaHeight();
		window.addEventListener('resize', this.handleResize);
	},

	beforeUnmount() {
		window.removeEventListener('resize', this.handleResize);
	},

	methods: {
		toggle(event: any) {
			this.$refs.menu.toggle(event);
		},

		handleKeydown(event: KeyboardEvent) {
			if (event.key === 'Enter' && !event.shiftKey && !this.agentListOpen) {
				event.preventDefault();
				this.handleSend();
			}
		},

		handleResize() {
			console.log('resize');
			this.isMobile = window.screen.width < 950;
			this.$nextTick(() => {
				if (this.$refs.menu) {
					this.$refs.menu.alignOverlay();
				}
			});
		},

		adjustTextareaHeight() {
			this.$nextTick(() => {
				this.$refs.inputRef.style.height = 'auto';
				this.$refs.inputRef.style.height = this.$refs.inputRef.scrollHeight + 'px';
			});
		},

		handleSend() {
			this.$emit('send', this.text);
			this.text = '';
		},

		handleUpload() {
			this.isUploading = true;

			this.$nextTick(() => {
				this.$refs.menu.alignOverlay();
			});

			const totalFiles = this.localFiles.length + this.oneDriveFiles.length;
			const combinedFiles = [...this.localFiles, ...this.oneDriveFiles];
			combinedFiles.forEach((file) => {
				if (file instanceof File) {
					file.source = 'local';
				} else {
					file.source = 'oneDrive';
				}
			});
			let filesUploaded = 0;
			let filesFailed = 0;
			const filesProgress = [];

			combinedFiles.forEach(async (file: any, index) => {
				try {
					if (file.source === 'local') {
						const formData = new FormData();
						formData.append('file', file);

						const onProgress = (event) => {
							if (event.lengthComputable) {
								filesProgress[index] = (event.loaded / event.total) * 100;

								let totalUploadProgress = 0;
								filesProgress.forEach((fileProgress) => {
									totalUploadProgress += fileProgress / totalFiles;
								});

								this.uploadProgress = totalUploadProgress;
							}
						};

						await this.$appStore.uploadAttachment(
							formData,
							this.$appStore.currentSession.sessionId,
							onProgress,
						);
					} else if (file.source === 'oneDrive') {
						await this.callCoreApiOneDriveWorkSchoolDownloadEndpoint(file.id, file.parentReference.driveId);
					}
					filesUploaded += 1;
				} catch (error) {
					filesFailed += 1;
					this.$toast.add({
						severity: 'error',
						summary: 'Error',
						detail: `File upload failed for "${file.name}". ${
							error.message ? error.message : error.title ? error.title : ''
						}`,
						life: 5000,
					});
				} finally {
					if (totalFiles === filesUploaded + filesFailed) {
						// this.showFileUploadDialog = false;
						this.isUploading = false;
						this.uploadProgress = 0;
						this.oneDriveFiles = [];
						this.localFiles = [];
						this.$nextTick(() => {
							this.$refs.menu.alignOverlay();
						});
						this.toggle();
						if (filesUploaded > 0) {
							this.$toast.add({
								severity: 'success',
								summary: 'Success',
								detail: `Successfully uploaded ${filesUploaded} file${totalFiles > 1 ? 's' : ''}.`,
								life: 5000,
							});
						}
					}
				}
			});
		},

		deleteFileKeydown(event: KeyboardEvent) {
			if (event.key === 'Escape') {
				this.fileToDelete = null;
			}
		},

		removeDialogFile() {
			this.deleteFileProcessing = true;
			if (this.fileToDelete.type === 'local') {
				this.removeLocalFile(this.fileToDelete.file);
			} else if (this.fileToDelete.type === 'oneDrive') {
				this.removeOneDriveFile(this.fileToDelete.file);
			} else if (this.fileToDelete.type === 'attachment') {
				this.removeAttachment(this.fileToDelete.file);
			}
		},

		async removeAttachment(file: any) {
			await this.$appStore.deleteAttachment(file);
			this.fileToDelete = null;
			this.deleteFileProcessing = false;

			this.$nextTick(() => {
				this.$refs.menu.alignOverlay();
			});
		},

		removeLocalFile(file: any) {
			this.localFiles = this.localFiles.filter((localFile) => localFile.name !== file.name);
			this.fileToDelete = null;
			this.deleteFileProcessing = false;

			this.$nextTick(() => {
				this.$refs.menu.alignOverlay();
			});
		},

		removeOneDriveFile(file: any) {
			this.oneDriveFiles = this.oneDriveFiles.filter((oneDriveFile) => oneDriveFile.name !== file.name);
			this.fileToDelete = null;
			this.deleteFileProcessing = false;

			this.$nextTick(() => {
				this.$refs.menu.alignOverlay();
			});
		},

		browseFiles() {
			this.$refs.fileUpload.$el.querySelector('input[type="file"]').click();
		},

		formatSize(bytes) {
			const k = 1024;
			const dm = 3;
			const sizes = this.$primevue.config.locale.fileSizeTypes;

			if (bytes === 0) {
				return `0 ${sizes[0]}`;
			}

			const i = Math.floor(Math.log(bytes) / Math.log(k));
			const formattedSize = parseFloat((bytes / Math.pow(k, i)).toFixed(dm));

			return `${formattedSize} ${sizes[i]}`;
		},

		validateUploadedFiles(files: any) {
			const allowedFileTypes = this.$appConfigStore.allowedUploadFileExtensions;
			const filteredFiles: any[] = [];

			files.forEach((file: any) => {
				const localFileAlreadyExists = this.localFiles.some(
					(existingFile: any) => existingFile.name === file.name && existingFile.size === file.size,
				);
				const oneDriveFileAlreadyExists = this.oneDriveFiles.some(
					(existingFile: any) => existingFile.name === file.name && existingFile.size === file.size,
				);
				const fileAlreadyExists = localFileAlreadyExists || oneDriveFileAlreadyExists;

				if (fileAlreadyExists) return;

				if (file.size > 536870912) {
					this.$toast.add({
						severity: 'error',
						summary: 'Error',
						detail: 'File size exceeds the limit of 512MB.',
						life: 5000,
					});
				} else if (allowedFileTypes && allowedFileTypes !== '') {
					const fileExtension = file.name.split('.').pop()?.toLowerCase();
					const isFileTypeAllowed = allowedFileTypes
						.split(',')
						.map((type: string) => type.trim().toLowerCase())
						.includes(fileExtension);

					if (!isFileTypeAllowed) {
						this.$toast.add({
							severity: 'error',
							summary: 'Error',
							detail: `File type not supported. File: ${file.name}`,
							life: 5000,
						});
					} else {
						filteredFiles.push(file);
					}
				} else {
					filteredFiles.push(file);
				}
			});

			if (this.localFiles.length + this.oneDriveFiles.length + filteredFiles.length > this.maxFiles) {
				this.$toast.add({
					severity: 'error',
					summary: 'Error',
					detail: `You can only upload a maximum of ${this.maxFiles} files at a time.`,
					life: 5000,
				});
				filteredFiles.splice((this.maxFiles - (this.localFiles.length + this.oneDriveFiles.length)));
			}

			return filteredFiles;
		},

		fileSelected(event: any) {
			const filteredFiles: any[] = [];
			filteredFiles.push(...this.validateUploadedFiles(event.files));

			this.localFiles = [...this.localFiles, ...filteredFiles];

			if (this.$refs.fileUpload) {
				this.$refs.fileUpload.clear();
			}

			this.$nextTick(() => {
				this.$refs.menu.alignOverlay();
			});
		},

		handleDrop(files) {
			const mockFileEvent = { files };

			this.fileSelected(mockFileEvent);

			if (this.$refs.menu && this.$refs.fileUploadButton) {
				const fileUploadButton = this.$refs.fileUploadButton.$el;

				this.$refs.menu.show({ currentTarget: fileUploadButton });
			}
		},

		async connectOneDriveWorkSchool() {
			await this.$authStore.requestOneDriveWorkSchoolConsent();
			if (localStorage.getItem('oneDriveWorkSchoolConsentRedirect') !== 'true') {
				await this.oneDriveWorkSchoolConnect();
			}
		},

		async oneDriveWorkSchoolConnect() {
			this.connectingOneDrive = true;
			await this.$appStore.oneDriveWorkSchoolConnect().then(() => {
				this.$toast.add({
					severity: 'success',
					summary: 'Success',
					detail: `Your account is now connected to OneDrive.`,
					life: 5000,
				});
				this.connectingOneDrive = false;
			});
		},

		async oneDriveWorkSchoolDisconnect() {
			this.disconnectingOneDrive = true;
			await this.$appStore.oneDriveWorkSchoolDisconnect().then(() => {
				this.$toast.add({
					severity: 'success',
					summary: 'Success',
					detail: `Your account is now disconnected from OneDrive.`,
					life: 5000,
				});
				this.disconnectingOneDrive = false;
			});
		},

		async oneDriveWorkSchoolDownload() {
			this.showOneDriveIframeDialog = true;

			let oneDriveToken;
			try {
				oneDriveToken = await this.$authStore.getOneDriveWorkSchoolToken();
			} catch (error) {
				console.error(error);
				oneDriveToken = await this.$authStore.requestOneDriveWorkSchoolConsent();
			}

			const iframe = document.createElement('iframe');
			iframe.style.width = '100%';
			iframe.style.height = '100%';
			iframe.style.border = 'none';

			const dialogContent = document.getElementById('oneDriveIframeDialogContent');
			dialogContent.innerHTML = ''; // Clear any existing content
			dialogContent.appendChild(iframe);

			const queryString = new URLSearchParams({
				filePicker: JSON.stringify(this.filePickerParams),
				locale: 'en-us',
			});

			const url = `${this.oneDriveBaseURL}_layouts/15/FilePicker.aspx?${queryString}`;

			const form = document.createElement('form');
			form.setAttribute('action', url);
			form.setAttribute('method', 'POST');
			iframe.contentWindow.document.body.append(form);

			const input = iframe.contentWindow.document.createElement('input');
			input.setAttribute('type', 'hidden');
			input.setAttribute('name', 'access_token');
			input.setAttribute('value', oneDriveToken.accessToken);
			form.appendChild(input);

			form.submit();

			const handleWindowMessage = (event) => {
				const message = event.data;

				if (
					message.type === 'initialize' &&
					message.channelId === this.filePickerParams.messaging.channelId
				) {
					this.port = event.ports[0];
					this.port.addEventListener('message', this.messageListener);
					this.port.start();
					this.port.postMessage({
						type: 'activate',
					});
				}
			};

			window.removeEventListener('message', handleWindowMessage);
			window.addEventListener('message', handleWindowMessage);
		},

		async messageListener(event) {
			const message = event.data;
			let dialogContent;

			switch (message.type) {
				case 'notification':
					console.log(`notification: ${JSON.stringify(message)}`);
					break;

				case 'command': {
					this.port.postMessage({
						type: 'acknowledge',
						id: message.id,
					});

					const command: any = message.data;

					switch (command.command) {
						case 'authenticate': {
							const token = await this.$authStore.getOneDriveWorkSchoolToken();

							if (token) {
								this.port.postMessage({
									type: 'result',
									id: message.id,
									data: {
										result: 'token',
										token: token.accessToken,
									},
								});
							} else {
								console.error(`Could not get auth token for command: ${JSON.stringify(command)}`);
							}
							break;
						}

						case 'close':
							console.log(`Closed: ${JSON.stringify(command)}`);

							dialogContent = document.getElementById('oneDriveIframeDialogContent');
							dialogContent.innerHTML = '';
							window.removeEventListener('message', this.messageListener);
							this.port.close();
							this.showOneDriveIframeDialog = false;
							break;

						case 'pick':
							const filteredFiles: any[] = [];
							filteredFiles.push(...this.validateUploadedFiles(command.items));

							this.oneDriveFiles.push(...filteredFiles);

							this.$nextTick(() => {
								this.$refs.menu.alignOverlay();
							});

							dialogContent = document.getElementById('oneDriveIframeDialogContent');
							dialogContent.innerHTML = '';
							window.removeEventListener('message', this.messageListener);
							this.port.close();
							this.showOneDriveIframeDialog = false;

							this.port.postMessage({
								type: 'result',
								id: message.id,
								data: {
									result: 'success',
								},
							});
							break;

						default:
							console.warn(`Unsupported command: ${JSON.stringify(command)}`, 2);
							this.port.postMessage({
								result: 'error',
								error: {
									code: 'unsupportedCommand',
									message: command.command,
								},
								isExpected: true,
							});
							break;
					}
					break;
				}
			}
		},

		async callCoreApiOneDriveWorkSchoolDownloadEndpoint(id, driveId) {
			const oneDriveToken = await this.$authStore.requestOneDriveWorkSchoolConsent();

			await this.$appStore.oneDriveWorkSchoolDownload(this.$appStore.currentSession.sessionId, {
				id,
				driveId,
				access_token: oneDriveToken,
			});
		},

		hideAllPoppers() {
			hideAllPoppers();
		},
	},
};
</script>

<style lang="scss" scoped>
.chat-input {
	display: flex;
	background-color: white;
	border-radius: 8px;
	width: 100%;
}

.pre-input {
	flex: 0 0 10%;
}

.chat-input .input-wrapper {
	display: flex;
	align-items: stretch;
	width: 100%;
}

.tooltip-component {
	height: 100%;
	margin-right: 0.5rem;
	display: flex;
	align-items: center;
}

.mentionable {
	width: 100%;
	height: auto;
	max-height: 128px;
	display: flex;
	flex-direction: column;
	flex: 1;
}

.input {
	width: 100%;
	height: 64px;
	max-height: 128px;
	overflow-y: auto;
	border-radius: 0px;
	font-size: 1rem;
	color: #6c6c6c;
	padding: 1.05rem 0.75rem 0.5rem 0.75rem;
	border: 2px solid #e1e1e1;
	transition:
		background-color 0.3s,
		color 0.3s,
		border-color 0.3s,
		box-shadow 0.3s;
	resize: none;
}

.input:focus-visible {
	border-radius: 0px !important;
	outline: none;
}

.mention-item {
	padding: 4px 10px;
}

.mention-selected {
	background: rgb(192, 250, 153);
}

.input:focus {
	// height: 192px;
}

.sr-only {
	display: none;
}

.context-menu {
	position: absolute;
	bottom: 100%;
}

.submit {
	flex: 0 0 10%;
	text-align: left;
	flex-basis: auto;
}

.file-upload-button {
	height: 100%;
}

.attached-files-container {
	padding-bottom: 1rem;
}

.attached-files {
	display: flex;
	flex-direction: row;
	align-items: center;
	justify-content: space-between;
	flex-wrap: nowrap;
}

.file-remove {
	margin-left: 1rem;
}

.file-upload-drag-and-drop {
	display: flex;
	flex: 1;
}

.p-fileupload-content {
	border-top-left-radius: 6px;
	border-top-right-radius: 6px;
}

.upload-files-header {
	width: 100%;
	max-width: 500px;
}

.upload-files-header button {
	margin-right: 0.5rem;
}

.file-upload-button-container {
	display: flex;
	justify-content: right;
	margin-bottom: 0.5rem;
}

.file-overlay-panel__footer {
	display: flex;
	gap: 0.5rem;
}

.onedrive-iframe-content {
	height: 500px;
	width: 100%;
}

.delete-dialog-content {
	display: flex;
	justify-content: center;
	padding: 20px 150px;
}

.file-upload-header {
	display: flex;
	justify-content: flex-end;
	margin-bottom: 0.5rem;
}

.file-upload-empty-desktop {
	text-align: center;
	margin-bottom: 0.5rem;
}

@media only screen and (max-width: 405px) {
	.upload-files-header button {
		padding: 0.1rem 0.25rem !important;
	}
}

@media only screen and (max-width: 620px) {
	.upload-files-header {
		width: auto !important;
	}

	.upload-files-header button {
		padding: 0.25rem 0.5rem;
		margin-right: 0.25rem !important;
		margin-bottom: 0.25rem !important;
	}

	.tooltip-component {
		display: none;
	}

	.onedrive-iframe-dialog {
		width: 98vw;
	}

	.file-upload-container-button {
		padding: 0.5rem;
		font-size: 0.8rem;
	}

	.onedrive-iframe-content {
		height: 85vh;
		width: 90vw;
	}
}

@media only screen and (max-width: 950px) {
	.file-upload-empty-desktop {
		display: none;
	}

	.file-overlay-panel__footer {
		flex-direction: column;
	}
}
</style>

<style lang="scss">
@media only screen and (max-width: 545px) {
	.submit .p-button-label {
		display: none;
	}

	.submit .p-button-icon {
		margin: 0;
	}
}

.mention-item {
	padding: 4px 10px;
}

.mention-selected {
	background-color: #131833;
	color: #fff;
}

.file-upload-icon {
	width: 100%;
	text-align: center;
	font-size: 5rem;
	color: #000;
}

.file-upload-file-container {
	max-height: 50vh;
	overflow-y: auto;
}

.file-upload-file {
	border-color: rgb(226, 232, 240);
	border-radius: 6px;
	border-style: solid;
	border-width: 1px;
	display: flex;
	flex-direction: row;
	justify-content: space-between;
	padding: 0.5rem;
	width: 100%;
	align-items: center;
	margin-bottom: 0.5rem;
}

.file-upload-file_info {
	flex: 1;
	display: flex;
	flex-direction: row;
	align-items: center;
	gap: 10px;
	overflow: hidden;
	flex-shrink: 1;
	max-width: calc(100% - 50px);

	span {
		font-weight: 600;
		overflow: hidden;
		text-overflow: ellipsis;
		white-space: wrap;
		flex-shrink: 1;
		min-width: 0;
	}
}
.onedrive-iframe-dialog .p-dialog-content {
	padding: 0;
	overflow: hidden;
}

@media only screen and (max-width: 405px) {
	.file-upload-file_info div {
		display: none;
	}
}

.p-fileupload-content {
	padding: 0px;
}

.p-fileupload-buttonbar {
	display: none;
}

.p-fileupload-content {
	border: none;
}

.labelPadding {
	padding-left: 10px;
}
</style>
