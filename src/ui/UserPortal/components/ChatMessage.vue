<template>
	<div>
		<div class="message-row" :class="message.sender === 'User' ? 'message--out' : 'message--in'">
			<div class="message" tabindex="0">
				<div class="message__header">
					<!-- Sender -->
					<span class="header__sender">
						<AgentIcon
							v-if="message.sender !== 'User'"
							:src="$appConfigStore.agentIconUrl || '~/assets/FLLM-Agent-Light.svg'"
							alt="Agent avatar"
						/>
						<span>{{ getDisplayName() }}</span>
					</span>

					<!-- Tokens & Timestamp -->
					<span class="message__header--right">
						<Chip
							:label="`Tokens: ${message.tokens}`"
							class="token-chip"
							:class="message.sender === 'User' ? 'token-chip--out' : 'token-chip--in'"
							:pt="{
								label: {
									style: {
										color: message.sender === 'User' ? 'var(--accent-text)' : 'var(--primary-text)',
									},
								},
							}"
						/>
						<VTooltip :auto-hide="isMobile" :popper-triggers="isMobile ? [] : ['hover']">
							<span class="time-stamp" tabindex="0" @keydown.esc="hideAllPoppers">{{
								$filters.timeAgo(new Date(message.timeStamp))
							}}</span>
							<template #popper>
								<div role="tooltip">
									{{ formatTimeStamp(message.timeStamp) }}
								</div>
							</template>
						</VTooltip>

						<!-- Copy user message button -->
						<VTooltip :auto-hide="isMobile" :popper-triggers="isMobile ? [] : ['hover']">
							<Button
								v-if="message.sender === 'User'"
								class="message__copy"
								size="small"
								text
								icon="pi pi-copy"
								aria-label="Copy Message"
								@click.stop="handleCopyMessageContent"
								@keydown.esc="hideAllPoppers"
							/>
							<template #popper><div role="tooltip">Copy Message</div></template>
						</VTooltip>
					</span>
				</div>

				<!-- Message text -->
				<div class="message__body" @click="handleFileLinkInText">
					<!-- Attachments -->
					<AttachmentList
						v-if="message.sender === 'User'"
						:attachments="message.attachmentDetails ?? []"
						:attachment-ids="message.attachments"
					/>

					<!-- Message loading -->
					<template v-if="message.sender === 'Assistant' && message.type === 'LoadingMessage'">
						<div role="status">
							<i class="pi pi-spin pi-spinner" role="img" aria-label="Loading message"></i>
						</div>
					</template>

					<!-- Render the html content and any vue components within -->
					<!-- <component :is="compiledMarkdownComponent" v-else /> -->

					<div v-for="(content, index) in processedContent" v-else :key="index">
						<ChatMessageTextBlock v-if="content.type === 'text'" :value="content.value" />
						<ChatMessageContentBlock v-else :value="content" />
					</div>

					<!-- Analysis button -->
					<Button
						v-if="message.analysisResults && message.analysisResults.length > 0"
						class="message__button"
						:disabled="message.type === 'LoadingMessage'"
						size="small"
						text
						icon="pi pi-window-maximize"
						label="Analysis"
						@click.stop="isAnalysisModalVisible = true"
					/>
				</div>

				<!-- Assistant message footer -->
				<div v-if="message.sender !== 'User'" class="message__footer">
					<!-- Citations -->
					<div v-if="message.citations?.length" class="citations">
						<span><b>Citations: </b></span>
						<span
							v-for="citation in message.citations"
							:key="citation.id"
							v-tooltip.top="{ content: citation.filepath, showDelay: 500, hideDelay: 300 }"
							class="citation"
						>
							<i class="pi pi-file"></i>
							{{ citation.title.split('/').pop() }}
						</span>
					</div>

					<!-- Rating -->
					<span class="ratings">
						<!-- Like -->
						<span>
							<Button
								class="message__button"
								:disabled="message.type === 'LoadingMessage'"
								size="small"
								text
								:icon="message.rating ? 'pi pi-thumbs-up-fill' : 'pi pi-thumbs-up'"
								:label="message.rating ? 'Message Liked!' : 'Like'"
								@click.stop="handleRate(message, true)"
							/>
						</span>

						<!-- Dislike -->
						<span>
							<Button
								class="message__button"
								:disabled="message.type === 'LoadingMessage'"
								size="small"
								text
								:icon="message.rating === false ? 'pi pi-thumbs-down-fill' : 'pi pi-thumbs-down'"
								:label="message.rating === false ? 'Message Disliked.' : 'Dislike'"
								@click.stop="handleRate(message, false)"
							/>
						</span>
					</span>

					<!-- Avg MS Per Word: {{ averageTimePerWordMS }} -->
					<div v-if="messageDisplayStatus" class="loading-shimmer" style="font-weight: 600">
						{{ messageDisplayStatus }}
					</div>

					<!-- Right side buttons -->
					<span>
						<!-- Copy message button -->
						<Button
							:disabled="message.type === 'LoadingMessage'"
							class="message__button"
							size="small"
							text
							icon="pi pi-copy"
							label="Copy"
							@click.stop="handleCopyMessageContent"
						/>

						<!-- View prompt buttom -->
						<Button
							class="message__button"
							:disabled="message.type === 'LoadingMessage'"
							size="small"
							text
							icon="pi pi-book"
							label="View Prompt"
							@click.stop="handleViewPrompt"
						/>

						<!-- Prompt dialog -->
						<Dialog
							v-model:visible="viewPrompt"
							class="prompt-dialog"
							modal
							header="Completion Prompt"
						>
							<p class="prompt-text" tabindex="0">
								{{ prompt.prompt }}
							</p>
							<template #footer>
								<Button
									:style="{
										backgroundColor: $appConfigStore.primaryButtonBg,
										borderColor: $appConfigStore.primaryButtonBg,
										color: $appConfigStore.primaryButtonText,
									}"
									label="Close"
									@click="viewPrompt = false"
								/>
							</template>
						</Dialog>
					</span>
				</div>
			</div>
		</div>

		<!-- Date Divider -->
		<Divider v-if="message.sender == 'User'" align="center" type="solid" class="date-separator">
			{{ $filters.timeAgo(new Date(message.timeStamp)) }}
		</Divider>

		<!-- Analysis Modal -->
		<AnalysisModal
			:visible="isAnalysisModalVisible"
			:analysis-results="message.analysisResults ?? []"
			@update:visible="isAnalysisModalVisible = $event"
		/>
	</div>
</template>

<script lang="ts">
import hljs from 'highlight.js';
import 'highlight.js/styles/github-dark-dimmed.css';
import { marked } from 'marked';
import katex from 'katex';
import 'katex/dist/katex.min.css';
// import truncate from 'truncate-html';
import DOMPurify from 'dompurify';
import type { PropType } from 'vue';
import { hideAllPoppers } from 'floating-vue';

import type { Message, MessageContent, CompletionPrompt } from '@/js/types';
import api from '@/js/api';
import { fetchBlobUrl } from '@/js/fileService';

function processLatex(content) {
	const blockLatexPattern = /\\\[\s*([\s\S]+?)\s*\\\]/g;
	const inlineLatexPattern = /\\\(([\s\S]+?)\\\)/g;

	// Process block LaTeX: \[ ... \]
	content = content.replace(blockLatexPattern, (_, math) => {
		return katex.renderToString(math, { displayMode: true, throwOnError: false });
	});

	// Process inline LaTeX: \( ... \)
	content = content.replace(inlineLatexPattern, (_, math) => {
		return katex.renderToString(math, { throwOnError: false });
	});

	return content;
}

function trimToWordCount(str, count) {
	let wordCount = 0;
	let index = 0;

	while (wordCount < count && index < str.length) {
		if (str[index] === ' ' && str[index - 1] !== ' ' && index > 0) {
			wordCount++;
		}
		index++;
	}

	return str.substring(0, index).trim();
}

function getWordCount(str) {
	let wordCount = 0;
	let index = 0;

	str = str.trim();

	while (index < str.length) {
		if (str[index] === ' ' && str[index - 1] !== ' ' && index > 0) {
			wordCount++;
		}
		index++;
	}

	if (str.length > 0) {
		wordCount++;
	}

	return wordCount;
}

const MAX_WORD_SPEED_MS = 15;

export default {
	name: 'ChatMessage',

	props: {
		message: {
			type: Object as PropType<Message>,
			required: true,
		},
		showWordAnimation: {
			type: Boolean,
			required: false,
			default: false,
		},
	},

	emits: ['rate'],

	data() {
		return {
			prompt: {} as CompletionPrompt,
			viewPrompt: false,
			currentWordIndex: 0,
			isAnalysisModalVisible: false,
			isMobile: window.screen.width < 950,
			markedRenderer: null,
			pollingInterval: null,
			pollingIteration: 0,
			totalTimeElapsed: 0,
			totalWordsGenerated: 0,
			averageTimePerWordMS: 50,
			processedContent: [],
			completed: false,
			isRenderingMessage: false,
		};
	},

	computed: {
		messageContent() {
			if (this.message.status === 'Failed') {
				const failedMessage = this.message.text ?? 'Failed to generate a response.';
				return [
					{
						type: 'text',
						content: failedMessage,
						value: failedMessage,
						origValue: failedMessage,
					},
				];
			}

			return this.message.content ?? [];
		},

		messageDisplayStatus() {
			if (
				this.message.status === 'Failed' ||
				(this.message.status === 'Completed' && !this.isRenderingMessage)
			)
				return null;

			if (this.isRenderingMessage && this.messageContent.length > 0) return 'Responding';

			// Account for old messages that are complete (status of "Pending" with null operation_id)
			const isPending = this.message.status === 'Pending' && this.message.operation_id;
			if (
				this.showWordAnimation &&
				(isPending || this.message.status === 'InProgress' || this.message.status === 'Loading')
			)
				return 'Thinking';

			return null;
		},
	},

	watch: {
		message: {
			immediate: true,
			deep: true,
			handler(newMessage, oldMessage) {
				// There is an issue here if a message that is not the latest has an incomplete status
				if (newMessage.status === 'Completed') {
					this.computedAverageTimePerWord({ ...newMessage }, oldMessage ?? {});
					this.handleMessageCompleted(newMessage);
					return;
				}

				this.computedAverageTimePerWord({ ...newMessage }, oldMessage ?? {});
				if (
					!this.isRenderingMessage &&
					this.showWordAnimation &&
					newMessage.type !== 'LoadingMessage' &&
					newMessage.operation_id
				)
					this.startRenderingMessage();
			},
		},

		processedContent: {
			deep: true,
			handler() {
				this.markSkippableContent();
			},
		},
	},

	created() {
		this.createMarkedRenderer();

		if (this.message.text && this.message.sender === 'User') {
			this.processedContent = [
				{
					type: 'text',
					value: this.processContentBlock(this.message.text),
					origValue: this.message.text,
				},
			];
		} else if (this.message.content) {
			this.processedContent = this.message.content.map((content) => {
				this.currentWordIndex = getWordCount(content.value);
				return {
					type: content.type,
					content,
					value: this.processContentBlock(content.value),
					origValue: content.value,
				};
			});
		}
	},

	methods: {
		processContentBlock(contentToProcess) {
			let htmlContent = processLatex(contentToProcess ?? '');
			htmlContent = marked(htmlContent, { renderer: this.markedRenderer });
			return DOMPurify.sanitize(htmlContent);
		},

		computedAverageTimePerWord(newMessage, oldMessage) {
			const newContent = newMessage.content ?? [];
			const oldContent = oldMessage.content ?? [];

			this.pollingIteration += 1;

			// Calculate the number of words in the previous message content
			let amountOfOldWords = 0;
			if (oldContent) {
				amountOfOldWords = oldContent.reduce((acc, content) => {
					return acc + (content.value?.match(/[\w'-]+/g) || []).length;
				}, 0);
			}

			// Calculate the number of words in the new message content
			const amountOfNewWords = newContent.reduce((acc, content) => {
				return acc + (content.value?.match(/[\w'-]+/g) || []).length;
			}, 0);

			// Calculate the number of new words generated since the last request
			const newWordsGenerated = amountOfNewWords - amountOfOldWords;

			if (newWordsGenerated > 0) {
				this.totalWordsGenerated += newWordsGenerated;
				this.totalTimeElapsed += this.$appStore.getPollingRateMS();
			}

			// Calculate the average time per word
			if (this.pollingIteration === 1) {
				this.averageTimePerWordMS = MAX_WORD_SPEED_MS;
			} else {
				this.averageTimePerWordMS =
					this.totalWordsGenerated > 0
						? Number((this.totalTimeElapsed / this.totalWordsGenerated).toFixed(2))
						: 0;
			}
		},

		handleMessageCompleted() {
			this.averageTimePerWordMS = MAX_WORD_SPEED_MS;
			this.completed = true;
		},

		startRenderingMessage() {
			if (this.isRenderingMessage) return;

			this.displayWordByWord();
		},

		displayWordByWord() {
			this.isRenderingMessage = true;

			let currentContentIndex = Math.max(this.processedContent.length - 1, 0);
			let content = this.messageContent[currentContentIndex]?.value;
			let processedContent = this.processedContent[currentContentIndex]?.content;

			// If the processed content block is the same as the current content block,
			// and there is a new one, then we know to move to the next block
			// check content !== for files blocks that are still null, but the next block is already generated
			if (
				this.messageContent[currentContentIndex + 1] &&
				content === processedContent &&
				!!content
			) {
				currentContentIndex += 1;
				this.currentWordIndex = 0;

				content = this.messageContent[currentContentIndex]?.value;
				processedContent = this.processedContent[currentContentIndex]?.content;
			}

			if (content !== processedContent) {
				this.currentWordIndex += 1;

				if (this.messageContent[currentContentIndex]?.type === 'text') {
					const truncatedContent = trimToWordCount(content, this.currentWordIndex);

					this.processedContent[currentContentIndex] = {
						type: this.messageContent[currentContentIndex]?.type,
						content: truncatedContent,
						value: this.processContentBlock(truncatedContent),
						origValue: this.messageContent[currentContentIndex]?.value,
					};
				} else {
					this.processedContent[currentContentIndex] = {
						type: this.messageContent[currentContentIndex]?.type,
						content,
						value: content,
						origValue: this.messageContent[currentContentIndex]?.value,
					};
				}
			}

			// If the current block is still rendering, or there is another block, or the message is still processing
			if (
				(content !== processedContent && !!content) ||
				this.messageContent[currentContentIndex + 1] ||
				!this.completed
			) {
				return setTimeout(() => this.displayWordByWord(), this.averageTimePerWordMS);
			}

			// Trigger after polling rate from message completion to ensure message is always rendered on completion:
			// Just to make sure the message renders fully in the case the animation fails
			// this.processedContent = this.messageContent.map((content) => {
			// 	return {
			// 		type: content.type,
			// 		content,
			// 		value: content.type === 'text' ? this.processContentBlock(content.value) : content.value,
			// 		origValue: content.value,
			// 	};
			// });

			this.isRenderingMessage = false;
		},

		createMarkedRenderer() {
			this.markedRenderer = new marked.Renderer();

			// Code blocks
			this.markedRenderer.code = (code) => {
				const language = code.lang;
				const sourceCode = code.text || code;
				const validLanguage = !!(language && hljs.getLanguage(language));
				const highlighted = validLanguage
					? hljs.highlight(sourceCode, { language })
					: hljs.highlightAuto(sourceCode);
				const languageClass = validLanguage ? `hljs language-${language}` : 'hljs';
				const encodedCode = encodeURIComponent(sourceCode);
				return `<pre><code class="${languageClass}" data-code="${encodedCode}" data-language="${highlighted.language}">${highlighted.value}</code></pre>`;
			};

			// Links
			this.markedRenderer.link = ({ href, title, text }) => {
				// Check if the link is a file download type.
				const isFileDownload = href.includes('/files/FoundationaLLM');

				if (isFileDownload) {
					const matchingFileBlock = this.messageContent.find(
						(block) => block.type === 'file_path' && block.value === href,
					);

					// Append file icon if there's a matching file_path
					const fileName = matchingFileBlock?.fileName.split('/').pop() ?? '';
					const fileIcon = matchingFileBlock
						? `<i class="${this.$getFileIconClass(fileName, true)}" class="attachment-icon"></i>`
						: `<i class="pi pi-file" class="attachment-icon"></i>`;
					return `${fileIcon} &nbsp;<a href="#" data-href="${href}" data-filename="${fileName}" title="${title || ''}" class="file-download-link">${text}</a>`;
				} else {
					return `<a href="${href}" title="${title || ''}" target="_blank">${text}</a>`;
				}
			};
		},

		markSkippableContent() {
			if (!this.processedContent) return;

			this.processedContent.forEach((contentBlock) => {
				if (contentBlock.type === 'file_path') {
					// Check for a matching text content that shares the same URL
					const matchingTextContent = this.processedContent.find(
						(block) => block.type === 'text' && block.origValue.includes(contentBlock.origValue),
					);

					if (matchingTextContent) {
						// Set the fileName in the matching text content
						matchingTextContent.fileName = contentBlock.fileName;
						// Skip rendering this file_path block
						contentBlock.skip = true;
					}
				}
			});
		},

		formatTimeStamp(timeStamp: string) {
			const date = new Date(timeStamp);
			const options = {
				year: 'numeric',
				month: 'long',
				day: 'numeric',
				hour: 'numeric',
				minute: 'numeric',
				second: 'numeric',
				timeZoneName: 'short',
			};
			return date.toLocaleString(undefined, options);
		},

		getDisplayName() {
			return this.message.sender === 'User'
				? this.message.senderDisplayName
				: this.message.senderDisplayName || 'Agent';
		},

		handleCopyMessageContent() {
			let contentToCopy = '';
			if (this.messageContent && this.messageContent?.length > 0) {
				this.messageContent.forEach((contentBlock) => {
					switch (contentBlock.type) {
						case 'text':
							contentToCopy += contentBlock.value;
							break;
						// default:
						// 	contentToCopy += `![${contentBlock.fileName || 'image'}](${contentBlock.value})`;
						// 	break;
					}
				});
			} else {
				contentToCopy = this.message.text;
			}

			const textarea = document.createElement('textarea');
			textarea.value = contentToCopy;
			document.body.appendChild(textarea);
			textarea.select();
			document.execCommand('copy');
			document.body.removeChild(textarea);

			this.$toast.add({
				severity: 'success',
				detail: 'Message copied to clipboard!',
				life: 5000,
			});
		},

		handleRate(message: Message, isLiked: boolean) {
			this.$emit('rate', { message, isLiked: message.rating === isLiked ? null : isLiked });
		},

		async handleViewPrompt() {
			const prompt = await api.getPrompt(this.message.sessionId, this.message.completionPromptId);
			this.prompt = prompt;
			this.viewPrompt = true;
		},

		hideAllPoppers() {
			hideAllPoppers();
		},

		handleFileLinkInText(event: MouseEvent) {
			const link = (event.target as HTMLElement).closest('a.file-download-link');
			if (link && link.dataset.href) {
				event.preventDefault();

				const content: MessageContent = {
					type: 'file_path',
					value: link.dataset.href,
					fileName: link.dataset.filename || link.textContent,
				};

				fetchBlobUrl(content, this.$toast);
			}
		},
	},
};
</script>

<style lang="scss" scoped>
@keyframes loading-shimmer {
	0% {
		background-position: -100% top;
	}

	to {
		background-position: 250% top;
	}
}

$shimmerColor: white;
// $textColor: var(--accent-text);
$textColor: #131833;
.loading-shimmer {
	text-fill-color: transparent;
	-webkit-text-fill-color: transparent;
	animation-delay: 0.3s;
	animation-duration: 3s;
	animation-iteration-count: infinite;
	animation-name: loading-shimmer;
	background: $textColor
		gradient(linear, 100% 0, 0 0, from($textColor), color-stop(0.5, $shimmerColor), to($textColor));
	background: $textColor -webkit-gradient(linear, 100% 0, 0 0, from($textColor), color-stop(0.5, $shimmerColor), to($textColor));
	background-clip: text;
	-webkit-background-clip: text;
	background-repeat: no-repeat;
	background-size: 50% 200%;
	display: flex;
	align-items: center;
}

[dir='ltr'] .loading-shimmer {
	background-position: -100% top;
}

[dir='rtl'] .loading-shimmer {
	background-position: 200% top;
}

.message-row {
	display: flex;
	align-items: flex-end;
	margin-top: 8px;
	margin-bottom: 8px;
}

.message {
	padding: 12px;
	width: 80%;
	box-shadow: 0 5px 10px 0 rgba(27, 29, 33, 0.1);
}

.date-separator {
	display: none;
}

.message--in {
	.message {
		background-color: rgba(250, 250, 250, 1);
	}
}

.message--out {
	flex-direction: row-reverse;
	.message {
		background-color: var(--primary-color);
		color: var(--primary-text);
	}
}

.message__header {
	margin-bottom: 12px;
	display: flex;
	justify-content: space-between;
	padding-left: 12px;
	padding-right: 12px;
	padding-top: 8px;
}

.message__header--right {
	display: flex;
	align-items: center;
	flex-shrink: 0;
}

.message__body {
	// white-space: pre-wrap;
	overflow-wrap: break-word;
	overflow-x: auto;
	padding-left: 12px;
	padding-right: 12px;
	padding-top: 8px;
	padding-bottom: 8px;
}

.message__footer {
	margin-top: 8px;
	display: flex;
	justify-content: space-between;
	flex-wrap: wrap;
}

.message__copy {
	color: var(--primary-text) !important;
	margin-left: 4px;
}

.header__sender {
	display: flex;
	align-items: center;
}

.avatar {
	width: 32px;
	height: 32px;
	border-radius: 50%;
	margin-right: 12px;
}

.token-chip {
	border-radius: 24px;
	margin-right: 12px;
}

.token-chip--out {
	background-color: var(--accent-color);
}

.token-chip--in {
	background-color: var(--primary-color);
}

.citations {
	flex-basis: 100%;
	padding: 8px 12px;
	display: flex;
	flex-wrap: wrap;
	align-items: center;
}

.citation {
	background-color: var(--primary-color);
	color: var(--primary-text);
	margin: 4px;
	padding: 4px 8px;
	cursor: pointer;
	white-space: nowrap;
}

.ratings {
	display: flex;
	// gap: 16px;
}

.icon {
	margin-right: 4px;
	cursor: pointer;
}

.prompt-text {
	white-space: pre-wrap;
	overflow-wrap: break-word;
}

@media only screen and (max-width: 950px) {
	.message {
		width: 95%;
	}
}

.message__button {
	color: #00356b;
}
</style>

<style lang="scss">
.p-chip .p-chip-text {
	line-height: 1.1;
	font-size: 0.75rem;
}
.prompt-dialog {
	width: 50vw;
}

.message__body img {
	max-width: 100% !important;
	height: auto !important;
	display: block !important;
}

@media only screen and (max-width: 950px) {
	.prompt-dialog {
		width: 90vw;
	}
}

@media only screen and (max-width: 545px) {
	.date-separator {
		display: flex !important;
	}
	.time-stamp {
		display: none;
	}
	.token-chip {
		margin-right: 0px !important;
	}
	.message__button .p-button-label {
		display: none;
	}
	.message__button .p-button-icon {
		margin-right: 0px;
	}
}
</style>
