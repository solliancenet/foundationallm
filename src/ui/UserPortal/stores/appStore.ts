import { defineStore } from 'pinia';
import { useAppConfigStore } from './appConfigStore';
import { useAuthStore } from './authStore';
import type {
	Session,
	ChatSessionProperties,
	Message,
	UserProfile,
	Agent,
	FileStoreConfiguration,
	OneDriveWorkSchool,
	ResourceProviderGetResult,
	ResourceProviderUpsertResult,
	ResourceProviderDeleteResults,
	Attachment,
	MessageContent,
} from '@/js/types';
import api from '@/js/api';
import eventBus from '@/js/eventBus';

export const useAppStore = defineStore('app', {
	state: () => ({
		sessions: [] as Session[],
		currentSession: null as Session | null,
		renamedSessions: [] as Session[],
		deletedSessions: [] as Session[],
		currentMessages: [] as Message[],
		isSidebarClosed: false as boolean,
		agents: [] as ResourceProviderGetResult<Agent>[],
		selectedAgents: new Map(),
		lastSelectedAgent: null as ResourceProviderGetResult<Agent> | null,
		attachments: [] as Attachment[],
		longRunningOperations: new Map<string, string>(), // sessionId -> operationId
		fileStoreConfiguration: null as FileStoreConfiguration | null,
		oneDriveWorkSchool: null as boolean | null,
		userProfiles: null as UserProfile | null,
	}),

	getters: {},

	actions: {
		async init(sessionId: string) {
			const appConfigStore = useAppConfigStore();

			// No need to load sessions if in kiosk mode, simply create a new one and skip.
			if (appConfigStore.isKioskMode) {
				const newSession = await api.addSession(this.getDefaultChatSessionProperties());
				await this.changeSession(newSession);
				return;
			}

			await this.getSessions();

			if (this.sessions.length === 0) {
				await this.addSession(this.getDefaultChatSessionProperties());
				await this.changeSession(this.sessions[0]);
			} else {
				const existingSession = this.sessions.find((session: Session) => session.id === sessionId);
				await this.changeSession(existingSession || this.sessions[0]);
			}

			await this.getUserProfiles();

			// if (this.currentSession) {
			// 	await this.getMessages();
			// 	this.updateSessionAgentFromMessages(this.currentSession);
			// }
		},

		getDefaultChatSessionProperties(): ChatSessionProperties {
			const now = new Date();
			// Using the 'sv-SE' locale since it uses the 'YYY-MM-DD' format.
			const formattedNow = now
				.toLocaleString('sv-SE', {
					year: 'numeric',
					month: '2-digit',
					day: '2-digit',
					hour: '2-digit',
					minute: '2-digit',
					second: '2-digit',
					hour12: false,
				})
				.replace(' ', 'T')
				.replace('T', ' ');
			return {
				name: formattedNow,
			};
		},

		async getSessions(session?: Session) {
			const sessions = await api.getSessions();

			if (session) {
				// If the passed in session is already in the list, replace it.
				// This is because the passed in session has been updated, most likely renamed.
				// Since there is a slight delay in the backend updating the session name, this
				// ensures the session name is updated in the sidebar immediately.
				const index = sessions.findIndex((s) => s.id === session.id);
				if (index !== -1) {
					sessions.splice(index, 1, session);
				}
				this.sessions = sessions;
			} else {
				this.sessions = sessions;
			}

			// Handle inconsistencies in displaying the renamed session due to potential delays in the backend updating the session name.
			this.renamedSessions.forEach((renamedSession: Session) => {
				const existingSession = this.sessions.find((s: Session) => s.id === renamedSession.id);
				if (existingSession) {
					existingSession.display_name = renamedSession.display_name;
				}
			});

			// Handle inconsistencies in displaying the deleted session due to potential delays in the backend updating the session list.
			this.deletedSessions.forEach((deletedSession: Session) => {
				const existingSession = this.sessions.find((s: Session) => s.id === deletedSession.id);
				if (existingSession) {
					this.removeSession(deletedSession.id);
				}
			});
		},

		async addSession(properties: ChatSessionProperties) {
			if (!properties) {
				properties = this.getDefaultChatSessionProperties();
			}

			const newSession = await api.addSession(properties);
			await this.getSessions(newSession);

			// Only add newSession to the list if it doesn't already exist.
			// We optionally add it because the backend is sometimes slow to update the session list.
			if (!this.sessions.find((session: Session) => session.id === newSession.id)) {
				this.sessions = [newSession, ...this.sessions];
			}

			return newSession;
		},

		async renameSession(sessionToRename: Session, newSessionName: string) {
			const existingSession = this.sessions.find(
				(session: Session) => session.id === sessionToRename.id,
			);

			// Preemptively rename the session for responsiveness, and revert the name if the request fails.
			const previousName = existingSession.display_name;
			existingSession.display_name = newSessionName;

			try {
				await api.renameSession(sessionToRename.id, newSessionName);
				const existingRenamedSession = this.renamedSessions.find(
					(session: Session) => session.id === sessionToRename.id,
				);
				if (existingRenamedSession) {
					existingRenamedSession.display_name = newSessionName;
				} else {
					this.renamedSessions = [
						{ ...sessionToRename, display_name: newSessionName },
						...this.renamedSessions,
					];
				}
			} catch (error) {
				existingSession.display_name = previousName;
			}
		},

		async deleteSession(sessionToDelete: Session) {
			await api.deleteSession(sessionToDelete!.id);
			await this.getSessions();

			this.removeSession(sessionToDelete!.id);

			// Add the deleted session to the list of deleted sessions to handle inconsistencies in the backend updating the session list.
			this.deletedSessions = [sessionToDelete, ...this.deletedSessions];

			// Ensure there is at least always 1 session
			if (this.sessions.length === 0) {
				const newSession = await this.addSession(this.getDefaultChatSessionProperties());
				this.removeSession(sessionToDelete!.id);
				await this.changeSession(newSession);
			}

			const firstSession = this.sessions[0];
			if (firstSession) {
				await this.changeSession(firstSession);
			}
		},

		removeSession(sessionId: string) {
			this.sessions = this.sessions.filter((session: Session) => session.id !== sessionId);
		},

		initializeMessageContent(content: MessageContent) {
			return reactive({
				...content,
				blobUrl: '',
				loading: true,
				error: false,
			});
		},

		async getMessages() {
			const data = await api.getMessages(this.currentSession.id);
			this.currentMessages = data.map((message) => ({
				...message,
				content: message.content ? message.content.map(this.initializeMessageContent) : [],
			}));
			await nextTick();
		},

		updateSessionAgentFromMessages(session: Session) {
			const lastAssistantMessage = this.currentMessages
				.filter((message) => message.sender.toLowerCase() === 'assistant')
				.pop();
			if (lastAssistantMessage) {
				const agent = this.agents.find(
					(agent) => agent.resource.name === lastAssistantMessage.senderDisplayName,
				);
				if (agent) {
					this.setSessionAgent(session, agent);
				}
			}
		},

		getSessionAgent(session: Session) {
			if (!session) return null;
			let selectedAgent = this.selectedAgents.get(session.id);
			if (!selectedAgent) {
				if (this.lastSelectedAgent) {
					// Default to the last selected agent to make the selection "sticky" across sessions.
					selectedAgent = this.lastSelectedAgent;
				} else {
					// Default to the first agent in the list.
					selectedAgent = this.agents[0];
				}
			}
			return selectedAgent;
		},

		setSessionAgent(session: Session, agent: ResourceProviderGetResult<Agent>) {
			this.lastSelectedAgent = agent;
			return this.selectedAgents.set(session.id, agent);
		},

		/**
		 * Sends a message to the Core API.
		 *
		 * @param text - The text of the message to send.
		 * @returns A Promise that resolves when the message is sent.
		 */
		async sendMessage(text: string) {
			if (!text) return;

			const agent = this.getSessionAgent(this.currentSession!).resource;
			const sessionId = this.currentSession!.id;
			const relevantAttachments = this.attachments.filter(
				(attachment) => attachment.sessionId === sessionId,
			);

			const attachmentDetails = relevantAttachments.map((attachment) => ({
				objectId: attachment.id,
				displayName: attachment.fileName,
				contentType: attachment.contentType,
			}));

			const authStore = useAuthStore();
			const tempUserMessage: Message = {
				completionPromptId: null,
				id: '',
				rating: null,
				sender: 'User',
				senderDisplayName: authStore.currentAccount?.name ?? 'You',
				sessionId: this.currentSession!.id,
				text,
				timeStamp: new Date().toISOString(),
				tokens: 0,
				type: 'Message',
				vector: [],
				attachmentDetails,
			};
			this.currentMessages.push(tempUserMessage);

			const tempAssistantMessage: Message = {
				completionPromptId: null,
				id: '',
				rating: null,
				sender: 'Assistant',
				senderDisplayName: agent.name,
				sessionId: this.currentSession!.id,
				text: '',
				timeStamp: new Date().toISOString(),
				tokens: 0,
				type: 'LoadingMessage',
				vector: [],
			};
			this.currentMessages.push(tempAssistantMessage);

			if (agent.long_running) {
				// Handle long-running operations
				const operationId = await api.startLongRunningProcess('/completions', {
					session_id: this.currentSession!.id,
					user_prompt: text,
					agent_name: agent.name,
					settings: null,
					attachments: relevantAttachments.map((attachment) => String(attachment.id)),
				});

				this.longRunningOperations.set(this.currentSession!.id, operationId);
				this.pollForCompletion(this.currentSession!.id, operationId);
			} else {
				await api.sendMessage(
					this.currentSession!.id,
					text,
					agent,
					relevantAttachments.map((attachment) => String(attachment.id)),
				);
				await this.getMessages();
				// Get rid of the attachments that were just sent.
				this.attachments = this.attachments.filter((attachment) => {
					return !relevantAttachments.includes(attachment);
				});
			}
		},

		/**
		 * Polls for the completion of a long-running operation.
		 *
		 * @param sessionId - The session ID associated with the operation.
		 * @param operationId - The ID of the operation to check for completion.
		 */
		async pollForCompletion(sessionId: string, operationId: string) {
			while (true) {
				const status = await api.checkProcessStatus(operationId);
				if (status.isCompleted) {
					this.longRunningOperations.delete(sessionId);
					eventBus.emit('operation-completed', { sessionId, operationId });
					await this.getMessages();
					break;
				}
				await new Promise((resolve) => setTimeout(resolve, 2000)); // Poll every 2 seconds
			}
		},

		async rateMessage(messageToRate: Message, isLiked: Message['rating']) {
			const existingMessage = this.currentMessages.find(
				(message) => message.id === messageToRate.id,
			);

			// Preemptively rate the message for responsiveness, and revert the rating if the request fails.
			const previousRating = existingMessage.rating;
			existingMessage.rating = isLiked;

			try {
				await api.rateMessage(messageToRate, isLiked);
			} catch (error) {
				existingMessage.rating = previousRating;
			}
		},

		async changeSession(newSession: Session) {
			const nuxtApp = useNuxtApp();
			const appConfigStore = useAppConfigStore();

			if (appConfigStore.isKioskMode) {
				nuxtApp.$router.push({ query: {} });
			} else {
				const query = { chat: newSession.id };
				nuxtApp.$router.push({ query });
			}

			this.currentSession = newSession;
			await this.getMessages();
			this.updateSessionAgentFromMessages(newSession);
		},

		toggleSidebar() {
			this.isSidebarClosed = !this.isSidebarClosed;
		},

		async getAgents() {
			this.agents = await api.getAllowedAgents();
			return this.agents;
		},

		async getFileStoreConfiguration() {
			this.fileStoreConfiguration = await api.getFileStoreConfiguration();
			return this.fileStoreConfiguration;
		},

		async oneDriveWorkSchoolConnect() {
			await api.oneDriveWorkSchoolConnect().then(() => {
				this.oneDriveWorkSchool = true;
			});
		},

		async oneDriveWorkSchoolDisconnect() {
			await api.oneDriveWorkSchoolDisconnect().then(() => {
				this.oneDriveWorkSchool = false;
			});
		},

		async getUserProfiles() {
			this.userProfiles = await api.getUserProfile();
			this.oneDriveWorkSchool = this.userProfiles?.flags.oneDriveWorkSchoolEnabled;
			return this.userProfiles;
		},

		async oneDriveWorkSchoolDownload(sessionId: string, oneDriveWorkSchool: OneDriveWorkSchool) {
			const agent = this.getSessionAgent(this.currentSession!).resource;
			// If the agent is not found, do not upload the attachment and display an error message.
			if (!agent) {
				throw new Error('No agent selected.');
			}

			const item = (await api.oneDriveWorkSchoolDownload(
				sessionId,
				agent.name,
				oneDriveWorkSchool,
			)) as OneDriveWorkSchool;
			const newAttachment: Attachment = {
				id: item.objectId!,
				fileName: item.name!,
				sessionId,
				contentType: item.mimeType!,
				source: 'OneDrive Work/School',
			};

			this.attachments.push(newAttachment);

			return item.objectId;
		},

		async uploadAttachment(file: FormData, sessionId: string, progressCallback: Function) {
			const agent = this.getSessionAgent(this.currentSession!).resource;
			// If the agent is not found, do not upload the attachment and display an error message.
			if (!agent) {
				throw new Error('No agent selected.');
			}

			const upsertResult = (await api.uploadAttachment(
				file,
				sessionId,
				agent.name,
				progressCallback,
			)) as ResourceProviderUpsertResult;
			const fileName = file.get('file')?.name;
			const contentType = file.get('file')?.type;
			const newAttachment: Attachment = {
				id: upsertResult.objectId,
				fileName,
				sessionId,
				contentType,
				source: 'Local Computer',
			};

			this.attachments.push(newAttachment);

			return upsertResult.objectId;
		},

		async deleteAttachment(attachment: Attachment) {
			const deleteResults: ResourceProviderDeleteResults = await api.deleteAttachments([
				attachment.id,
			]);
			Object.entries(deleteResults).forEach(([key, value]) => {
				if (key === attachment.id) {
					if (value.deleted) {
						this.attachments = this.attachments.filter((a) => a.id !== attachment.id);
					} else {
						throw new Error(`Could not delete the attachment: ${value.reason}`);
					}
				}
			});
		},

		async getVirtualUser() {
			return await api.getVirtualUser();
		},
	},
});
