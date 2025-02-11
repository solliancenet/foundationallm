export type ResourceProviderGetResult<T> = {
	/**
	 * Represents the result of a fetch operation.
	 */
	resource: T;

	/**
	 * List of authorized actions on the resource.
	 */
	actions: string[];

	/**
	 * List of roles on the resource.
	 */
	roles: string[];
};

export interface ContentArtifact {
	id: string;
	title: string;
	filepath: string;
}

export interface ResourceProviderUpsertResult {
	objectId: string;
	resourceExists: boolean;
}

export interface MessageContent {
	type: string;
	fileName: string;
	value: string;
	blobUrl?: string;
	loading?: boolean;
	error?: boolean;
}

export interface AttachmentDetail {
	objectId: string;
	displayName: string;
	contentType: string;
}

export interface AnalysisResult {
	tool_input: string;
	tool_output: string;
	agent_capability_category: string;
	tool_name: string;
}

export interface Message {
	id: string;
	type: string;
	sessionId: string;
	timeStamp: string;
	sender: 'User' | 'Agent';
	senderName: string | null;
	senderDisplayName: string | null;
	tokens: number;
	text: string;
	rating: boolean | null;
	ratingComments: string | null;
	vector: Array<Number>;
	completionPromptId: string | null;
	contentArtifacts: Array<ContentArtifact>;
	content: Array<MessageContent>;
	attachments: Array<string>;
	attachmentDetails: Array<AttachmentDetail>;
	analysisResults: Array<AnalysisResult>;
	processingTime: number; // Calculated in milliseconds - not from the API
}

export interface MessageRatingRequest
{
    rating: boolean | null;
	comments: string | null;
}

export interface Session {
	id: string;
	type: string;
	sessionId: string;
	tokensUsed: Number;
	name: string;
	display_name: string;
	messages: Array<Message>;
}

export interface ChatSessionProperties {
	name: string;
}

export interface LongRunningOperation {
	id?: string;
	type: string;
	operation_id?: string;
	status: string;
	status_message?: string;
	last_updated?: Date;
	ttl: number;
	prompt_tokens: number;
	result?: Message;
}

export interface CompletionPrompt {
	id: string;
	type: string;
	sessionId: string;
	messageId: string;
	prompt: string;
}

export interface OrchestrationSettings {
	orchestrator?: string;
	endpoint_configuration?: { [key: string]: any } | null;
	model_parameters?: { [key: string]: any } | null;
}
export interface Agent {
	type: string;
	name: string;
	object_id: string;
	description: string;
	properties?: { [key: string]: string | null };
	long_running: boolean;
	orchestration_settings?: OrchestrationSettings;
	show_message_tokens?: boolean;
	show_message_rating?: boolean;
	show_view_prompt?: boolean;
	show_file_upload?: boolean;
}

export interface CompletionRequest {
	session_id?: string;
	user_prompt: string;
	agent_name?: string;
	settings?: OrchestrationSettings;
	attachments?: string[];
}

export interface Attachment {
	id: string;
	fileName: string;
	sessionId: string;
	contentType: string;
	source: string;
}

export interface ResourceProviderDeleteResult {
	deleted: boolean;
	reason?: string;
}

export interface ResourceProviderDeleteResults {
	[key: string]: ResourceProviderDeleteResult;
}

export interface UserProfile {
	id: string;
	type: string;
	upn: string;
	flags: Record<string, boolean>;
}

export interface FileStoreConnector {
	name: string;
	category: string;
	subcategory: string;
	url: string;
}

export interface CoreConfiguration {
	maxUploadsPerMessage: number;
	fileStoreConnectors?: FileStoreConnector[];
	completionResponsePollingIntervalSeconds: number;
}

export interface OneDriveWorkSchool {
	id: string;
	driveId?: string;
	objectId?: string;
	name?: string;
	mimeType?: string;
	access_token?: string;
}
