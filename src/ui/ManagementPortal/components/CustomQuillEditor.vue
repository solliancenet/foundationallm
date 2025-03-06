<template>
	<div class="quill-container" ref="quillContainer">
		<QuillEditor
			ref="quillEditor"
			class="custom-quill-editor"
			:content="content"
			:toolbar="`#${toolbarId}`"
			content-type="html"
			@keydown.enter="handleEnterFromQuill"
			@update:content="handleContentUpdate"
		>
			<template #toolbar>
				<div :id="toolbarId">
					<select class="ql-size" aria-label="Text Size" title="Text Size">
						<option value="small"></option>
						<option selected></option>
						<option value="large"></option>
						<option value="huge"></option>
					</select>
					<button class="ql-bold" aria-label="Bold" title="Bold"></button>
					<button class="ql-italic" aria-label="Italic" title="Italic"></button>
					<button class="ql-underline" aria-label="Underline" title="Underline"></button>
					<button class="ql-strike" aria-label="Strike" title="Strike"></button>
					<button class="ql-link" aria-label="Link" title="Link"></button>
					<button class="ql-image" aria-label="Image" title="Image"></button>
					<button
						class="ql-list"
						value="ordered"
						aria-label="Ordered List"
						title="Ordered List"
					></button>
					<button
						class="ql-list"
						value="bullet"
						aria-label="Unordered List"
						title="Unordered List"
					></button>
					<button class="ql-clean" aria-label="Remove Styles" title="Remove Styles"></button>
					<button
						:class="`quill-view-html view-html-${randomNumber}`"
						aria-label="Edit HTML"
						style="width: 100px"
						@click="toggleHtmlDialog"
						@keydown.enter="toggleHtmlDialog"
					>
						Edit HTML
					</button>
				</div>
			</template>
		</QuillEditor>

		<!-- Raw HTML Dialog -->
		<Dialog
			v-model:visible="showHtmlDialog"
			:modal="true"
			:closable="false"
			:style="{ width: '90vw' }"
			header="Edit Footer HTML"
		>
			<CodeEditor
				ref="codeEditor"
				autofocus
				v-model="rawHtml"
				:languages="[['html', 'HTML']]"
				:wrap="true"
				theme="github-dark"
				style="width: 100%; height: 100%"
				@update:model-value="handleCodeEditorChange"
			/>

			<template #footer>
				<Button label="Save" @click="handleSaveRawHTML" />
				<Button label="Cancel" @click="toggleHtmlDialog" />
			</template>
		</Dialog>

		<!-- HTML Correction Dialog -->
		<Dialog
			v-model:visible="showHtmlCorrectionDialog"
			:modal="true"
			:closable="false"
			:style="{ width: '90vw' }"
			header="Footer HTML Correction Required"
		>
			<h4 style="margin-top: 0px; margin-bottom: 8px">
				The inputted html content must be corrected, please verify the corrected value:
			</h4>

			<Diff
				mode="split"
				theme="light"
				language="html"
				:prev="rawHtml"
				:current="processedContent"
			/>

			<template #footer>
				<Button label="Save" @click="handleSaveCorrectedHTML" />
				<Button label="Back" @click="handleHtmlCorrectionBack" />
			</template>
		</Dialog>
	</div>
</template>

<script lang="ts">
import { QuillEditor } from '@vueup/vue-quill';
import '@vueup/vue-quill/dist/vue-quill.snow.css';
import { Diff } from 'vue-diff';
import 'vue-diff/dist/index.css';
import 'highlight.js';
import CodeEditor from 'simple-code-editor';
import { v4 as uuidv4 } from 'uuid';

function getFocusableElements(container) {
	return Array.from(
		container.querySelectorAll(
			'a, button, input, textarea, select, details, [tabindex]:not([tabindex="-1"])',
		),
	).filter((el) => !el.hasAttribute('disabled') && el.offsetParent !== null);
}

export default {
	components: {
		QuillEditor,
		Diff,
		CodeEditor,
	},

	props: {
		initialContent: {
			type: String,
			required: true,
			default: '',
		},
	},

	emits: ['content-update'],

	data() {
		return {
			content: this.initialContent,
			oldContent: this.initialContent,
			rawHtml: '',
			processedContent: '',
			showHtmlDialog: false,
			showHtmlCorrectionDialog: false,
			toolbarId: `toolbar-${uuidv4()}`,
			randomNumber: Math.random(),
		};
	},

	watch: {
		initialContent(newContent) {
			if (newContent !== this.content) {
				this.content = newContent;
			}
		},
	},

	// mounted() {
	// 	this.initializeQuill();
	// },

	methods: {
		toggleHtmlDialog() {
			this.rawHtml = this.content;

			this.$refs.quillEditor.pasteHTML(this.rawHtml);
			this.processedContent = this.$refs.quillEditor.getHTML();

			this.showHtmlDialog = !this.showHtmlDialog;
		},

		handleContentUpdate(newContent) {
			this.oldContent = this.content;
			if (newContent !== this.content) {
				this.$emit('content-update', newContent);
			}
		},

		handleCodeEditorChange() {
			// Reset the highlighting state
			this.$refs.codeEditor.$refs.code.removeAttribute('data-highlighted');
		},

		handleSaveRawHTML() {
			this.$refs.quillEditor.pasteHTML(this.rawHtml);
			this.processedContent = this.$refs.quillEditor.getHTML();

			if (this.rawHtml !== this.processedContent) {
				this.showHtmlCorrectionDialog = true;
				return;
			}

			this.$emit('content-update', this.content);
			this.showHtmlDialog = false;
		},

		handleSaveCorrectedHTML() {
			this.$emit('content-update', this.content);
			this.showHtmlCorrectionDialog = false;
			this.showHtmlDialog = false;
		},

		handleHtmlCorrectionBack() {
			this.handleCodeEditorChange();
			this.showHtmlCorrectionDialog = false;
		},

		handleEnterFromQuill(event) {
			// console.log(event);
			if (event.key === 'Enter' && !event.shiftKey) {
				// event.preventDefault(); // Prevent Quill's default Tab handling
				const focusableElements = getFocusableElements(document);
				// console.log(focusableElements);
				const currentIndex = focusableElements.indexOf(
					focusableElements.find((el) => el.classList.contains(`view-html-${this.randomNumber}`)),
				);
				// console.log(currentIndex);
				const nextIndex = event.shiftKey ? currentIndex - 1 : currentIndex + 1;
				// console.log(nextIndex);
				this.$emit('content-update', this.oldContent);
				// focusableElements[nextIndex]?.focus();

				// this.$nextTick(() => {
				// 	this.content = this.oldContent;
				setTimeout(() => {
					// this.$emit('content-update', this.oldContent);
					focusableElements[nextIndex]?.focus();
				}, 50);
				// });

				// focusableElements[nextIndex]?.focus();
				// console.log(focusableElements[nextIndex]);
				// console.log(focusableElements[nextIndex].focus());
				// const nextElement = this.$refs.quillContainer;
				// console.log(nextElement);
				// nextElement?.focus(); // Focus the next element in the DOM
			}
		},

		// initializeQuill() {
		// 	console.log(this.$refs.quillEditor.getQuill());
		// 	const quill = this.$refs.quillEditor.getQuill();

		// 	quill.keyboard.addBinding(
		// 		{
		// 			key: 'Enter',
		// 		},
		// 		() => {
		// 			console.log('Enter key pressed');
		// 			const focusableElements = getFocusableElements(document);

		// 			const currentFocusedElement = document.activeElement;

		// 			const currentIndex = focusableElements.indexOf(currentFocusedElement);

		// 			const nextIndex = context.shiftKey ? currentIndex - 1 : currentIndex + 1;

		// 			if (focusableElements[nextIndex]) {
		//                 focusableElements[nextIndex].focus();
		//             }
		// 		}
		// 	);
		// },
	},
};
</script>

<style lang="scss" scoped>
.quill-container {
	max-width: 80ch;
}

.quill-view-html {
	font-size: 1rem;
	color: #4b5563;
	font-weight: 550;
	border-radius: 4px;
	padding: 0.5rem;
	cursor: pointer;
}
</style>

<style lang="scss">
.custom-quill-editor {
	.ql-container {
		height: auto;
	}

	.ql-editor {
		height: auto;
		min-height: 150px;
		max-height: 800px;
		resize: vertical;
		font-family: 'Poppins', sans-serif;
	}
}
</style>
