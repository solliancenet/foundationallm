<template>
	<div>
		<template v-if="loading">
			<div class="grid__loading-overlay">
				<LoadingGrid />
				<div>{{ loadingStatusText }}</div>
			</div>
		</template>
		<h2 class="page-header">Branding</h2>
		<div class="page-subheader">
			<p>Customize the look and feel of your UI.</p>
			<div style="display: flex; flex-direction: row; align-items: center; gap: 0.5rem">
				<p>Show contrast information</p>
				<InputSwitch v-model="showContrastInfo" />
			</div>
		</div>
		<div class="steps">
			<div v-for="key in orderedKeys" :key="key" class="step span-2">
				<div :id="key.split(':').pop()" class="step-header mb-2">{{ getFriendlyName(key) }}</div>
				<div class="mb-2">{{ getBrandingDescription(key) }}</div>
				<InputSwitch
					v-if="key === 'FoundationaLLM:Branding:KioskMode'"
					v-model:modelValue="kioskMode"
					@change="updateBrandingValue(key, JSON.stringify(kioskMode))"
				/>
				<CustomQuillEditor
					v-if="
						key === 'FoundationaLLM:Branding:FooterText' ||
						key === 'FoundationaLLM:Branding:NoAgentsMessage' ||
						key === 'FoundationaLLM:Branding:DefaultAgentWelcomeMessage'
					"
					:id="key.split(':').pop()"
					:initial-content="JSON.parse(JSON.stringify(getBrandingValue(key)))"
					@content-update="updateHtmlText(key, $event)"
				/>
				<InputText
					v-if="
						key !== 'FoundationaLLM:Branding:FooterText' &&
						key !== 'FoundationaLLM:Branding:KioskMode' &&
						key !== 'FoundationaLLM:Branding:NoAgentsMessage' &&
						key !== 'FoundationaLLM:Branding:DefaultAgentWelcomeMessage'
					"
					:value="getBrandingValue(key)"
					:aria-labelledby="key.split(':').pop()"
					class="branding-input"
					@input="updateBrandingValue(key, $event.target.value)"
				/>
				<div
					v-if="key === 'FoundationaLLM:Branding:LogoUrl'"
					:style="{ backgroundColor: getBrandingValue('FoundationaLLM:Branding:PrimaryColor') }"
					class="logo-preview"
				>
					<img :src="$filters.publicDirectory(getBrandingValue(key))" class="logo-image" />
				</div>
			</div>
			<div class="divider" />
			<div
				v-for="group in orderedKeyColorsGrouped"
				:key="group.label"
				class="step span-2 color-group-container"
			>
				<div class="color-group">
					<div v-for="key in group.keys" :key="key.key" class="step span-2">
						<div :id="key.key.split(':').pop()" class="step-header mb-2">
							{{ getFriendlyName(key.key) }}
						</div>
						<div class="mb-2">{{ getBrandingDescription(key.key) }}</div>
						<ColorInput
							:value="getBrandingValue(key.key)"
							:color="getColorBrandingValue(key.key)"
							:format="getColorBrandingFormat(key.key)"
							:original-value="getOriginalBrandingValue(key.key)"
							:aria-label="key.key.split(':').pop()"
							@update-value="(newValue) => updateBrandingValue(key.key, newValue)"
							@reset="resetBrandingValue(key.key, $event)"
						/>
					</div>
				</div>
				<div class="color-preview-container">
					<div v-if="showContrastInfo" class="color-ratio">
						{{
							contrastRatioText(
								group.keys.find((k) => k.type === 'background').key,
								group.keys.find((k) => k.type === 'foreground')?.key,
							)
						}}
					</div>
					<div
						class="color-preview-background"
						:style="{
							backgroundColor: getBrandingValue(
								group.keys.find((k) => k.type === 'background').key,
							),
						}"
					>
						<div
							class="color-preview-foreground"
							:style="{
								color:
									getBrandingValue(group.keys.find((k) => k.type === 'foreground')?.key) ||
									'transparent',
							}"
						>
							TEST
						</div>
					</div>
					<div
						v-if="
							group.keys.find((k) => k.type === 'background').key &&
							group.keys.find((k) => k.type === 'foreground')?.key &&
							showContrastInfo
						"
						class="color-wcag-results-container"
					>
						<div class="color-wcag-results">
							<div
								class="color-wcag-result"
								:style="{
									color: contrastColorAA(
										group.keys.find((k) => k.type === 'background').key,
										group.keys.find((k) => k.type === 'foreground')?.key,
									),
								}"
							>
								<div class="color-wcag-result-label">AA</div>
								<div class="color-wcag-result-value">
									{{
										contrastCompliance(
											group.keys.find((k) => k.type === 'background').key,
											group.keys.find((k) => k.type === 'foreground')?.key,
											4.5,
										)
									}}
								</div>
							</div>
							<div
								class="color-wcag-result"
								:style="{
									color: contrastColorAAA(
										group.keys.find((k) => k.type === 'background').key,
										group.keys.find((k) => k.type === 'foreground')?.key,
									),
								}"
							>
								<div class="color-wcag-result-label">AAA</div>
								<div class="color-wcag-result-value">
									{{
										contrastCompliance(
											group.keys.find((k) => k.type === 'background').key,
											group.keys.find((k) => k.type === 'foreground')?.key,
											7,
										)
									}}
								</div>
							</div>
						</div>
					</div>
				</div>
			</div>
			<div v-if="unorderedKeys.length > 0" class="divider" />
			<div v-for="key in unorderedKeys" :key="key" class="step span-2">
				<div :id="key.split(':').pop()" class="step-header mb-2">{{ getFriendlyName(key) }}</div>
				<div class="mb-2">{{ getBrandingDescription(key) }}</div>
				<InputText
					:value="getBrandingValue(key)"
					class="branding-input"
					:aria-labelledby="key.split(':').pop()"
					@input="updateBrandingValue(key, $event.target.value)"
				/>
			</div>
			<div class="button-container column-2 justify-self-end">
				<Button
					:disabled="JSON.stringify(branding) === JSON.stringify(brandingOriginal)"
					label="Reset"
					severity="secondary"
					@click="cancelBrandingChanges"
				/>
				<Button label="Set Default" @click="setDefaultBranding" />
				<Button label="Save" severity="primary" @click="saveBranding" />
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import api from '@/js/api';

function getLuminance(color: string) {
	let r, g, b;
	if (/^#[0-9A-F]{3}$/i.test(color)) {
		const rgb = color.match(/^#([0-9A-F]{3})$/i);
		r = parseInt(rgb[1][0] + rgb[1][0], 16);
		g = parseInt(rgb[1][1] + rgb[1][1], 16);
		b = parseInt(rgb[1][2] + rgb[1][2], 16);
	} else if (/^rgb\((\d{1,3}),\s*(\d{1,3}),\s*(\d{1,3})\)$/i.test(color)) {
		const rgb = color.match(/^rgb\((\d{1,3}),\s*(\d{1,3}),\s*(\d{1,3})\)$/i);
		r = parseInt(rgb[1]);
		g = parseInt(rgb[2]);
		b = parseInt(rgb[3]);
	} else if (/^#[0-9A-F]{6}$/i.test(color)) {
		r = parseInt(color.substr(1, 2), 16);
		g = parseInt(color.substr(3, 2), 16);
		b = parseInt(color.substr(5, 2), 16);
	}

	const srgbToLinear = (channel) => {
		const c = channel / 255;
		return c <= 0.04045 ? c / 12.92 : Math.pow((c + 0.055) / 1.055, 2.4);
	};

	const linearR = srgbToLinear(r);
	const linearG = srgbToLinear(g);
	const linearB = srgbToLinear(b);

	return 0.2126 * linearR + 0.7152 * linearG + 0.0722 * linearB;
}

export default {
	name: 'Branding',

	data() {
		return {
			branding: null as any,
			brandingOriginal: null as any,
			brandingDefault: [
				{
					key: 'FoundationaLLM:Branding:AccentColor',
					value: '#fff',
				},
				{
					key: 'FoundationaLLM:Branding:AccentTextColor',
					value: '#131833',
				},
				{
					key: 'FoundationaLLM:Branding:BackgroundColor',
					value: '#fff',
				},
				{
					key: 'FoundationaLLM:Branding:CompanyName',
					value: 'FoundationaLLM',
				},
				{
					key: 'FoundationaLLM:Branding:FavIconUrl',
					value: 'favicon.ico',
				},
				{
					key: 'FoundationaLLM:Branding:AgentIconUrl',
					value: '',
				},
				{
					key: 'FoundationaLLM:Branding:KioskMode',
					value: 'false',
				},
				{
					key: 'FoundationaLLM:Branding:LogoText',
					value: 'FoundationaLLM',
				},
				{
					key: 'FoundationaLLM:Branding:LogoUrl',
					value: 'foundationallm-logo-white.svg',
				},
				{
					key: 'FoundationaLLM:Branding:PageTitle',
					value: 'FoundationaLLM User Portal',
				},
				{
					key: 'FoundationaLLM:Branding:PrimaryColor',
					value: '#131833',
				},
				{
					key: 'FoundationaLLM:Branding:PrimaryTextColor',
					value: '#fff',
				},
				{
					key: 'FoundationaLLM:Branding:SecondaryColor',
					value: '#334581',
				},
				{
					key: 'FoundationaLLM:Branding:SecondaryTextColor',
					value: '#fff',
				},
				{
					key: 'FoundationaLLM:Branding:PrimaryButtonBackgroundColor',
					value: '#5472d4',
				},
				{
					key: 'FoundationaLLM:Branding:PrimaryButtonTextColor',
					value: '#fff',
				},
				{
					key: 'FoundationaLLM:Branding:SecondaryButtonBackgroundColor',
					value: '#70829a',
				},
				{
					key: 'FoundationaLLM:Branding:SecondaryButtonTextColor',
					value: '#fff',
				},
				{
					key: 'FoundationaLLM:Branding:FooterText',
					value: 'FoundationaLLM &copy; All rights reserved.',
				},
				{
					key: 'FoundationaLLM:Branding:NoAgentsMessage',
					value: 'No agents available. Please check with your system administrator for assistance.',
				},
				{
					key: 'FoundationaLLM:Branding:DefaultAgentWelcomeMessage',
					value: 'Start the conversation using the text box below.',
				},
			],
			orderedKeys: [
				'FoundationaLLM:Branding:CompanyName',
				'FoundationaLLM:Branding:FavIconUrl',
				'FoundationaLLM:Branding:FooterText',
				'FoundationaLLM:Branding:KioskMode',
				'FoundationaLLM:Branding:LogoText',
				'FoundationaLLM:Branding:LogoUrl',
				'FoundationaLLM:Branding:PageTitle',
				'FoundationaLLM:Branding:AgentIconUrl',
				'FoundationaLLM:Branding:NoAgentsMessage',
				'FoundationaLLM:Branding:DefaultAgentWelcomeMessage',
			],
			orderedKeyColorsGrouped: [
				{
					label: 'Accent',
					keys: [
						{
							key: 'FoundationaLLM:Branding:AccentColor',
							type: 'background',
						},
						{
							key: 'FoundationaLLM:Branding:AccentTextColor',
							type: 'foreground',
						},
					],
				},
				{
					label: 'Background',
					keys: [
						{
							key: 'FoundationaLLM:Branding:BackgroundColor',
							type: 'background',
						},
					],
				},
				{
					label: 'Primary Button',
					keys: [
						{
							key: 'FoundationaLLM:Branding:PrimaryButtonBackgroundColor',
							type: 'background',
						},
						{
							key: 'FoundationaLLM:Branding:PrimaryButtonTextColor',
							type: 'foreground',
						},
					],
				},
				{
					label: 'Primary',
					keys: [
						{
							key: 'FoundationaLLM:Branding:PrimaryColor',
							type: 'background',
						},
						{
							key: 'FoundationaLLM:Branding:PrimaryTextColor',
							type: 'foreground',
						},
					],
				},
				{
					label: 'Secondary Button',
					keys: [
						{
							key: 'FoundationaLLM:Branding:SecondaryButtonBackgroundColor',
							type: 'background',
						},
						{
							key: 'FoundationaLLM:Branding:SecondaryButtonTextColor',
							type: 'foreground',
						},
					],
				},
				{
					label: 'Secondary',
					keys: [
						{
							key: 'FoundationaLLM:Branding:SecondaryColor',
							type: 'background',
						},
						{
							key: 'FoundationaLLM:Branding:SecondaryTextColor',
							type: 'foreground',
						},
					],
				},
			],
			unorderedKeys: [] as string[],
			footerText: '',
			rawFooterTextHTML: '',
			showContrastInfo: false,
			kioskMode: false,
			showQuillrawHTMLDialog: false,
			loading: true,
			loadingStatusText: 'Retrieving data...' as string,
		};
	},

	async created() {
		await this.getBranding();
		this.footerText = JSON.parse(
			JSON.stringify(this.getBrandingValue('FoundationaLLM:Branding:FooterText')),
		);
		this.kioskMode = this.getBrandingValue('FoundationaLLM:Branding:KioskMode') === 'true';
	},

	methods: {
		async getBranding() {
			try {
				this.loading = true;
				this.branding = await api.getBranding();
				this.loading = false;
				this.brandingOriginal = JSON.parse(JSON.stringify(this.branding));
				this.getUnorderedKeys();
			} catch (error) {
				this.$toast.add({
					severity: 'error',
					detail: error?.response?._data || error,
					life: 5000,
				});
			}
		},

		getUnorderedKeys() {
			const keys = this.branding.map((brand: any) => brand.resource.key);
			const groupedKeys = this.orderedKeyColorsGrouped.flatMap((group) =>
				group.keys.map((k) => k.key),
			);
			this.unorderedKeys = keys.filter(
				(key: string) => !this.orderedKeys.includes(key) && !groupedKeys.includes(key),
			);
		},

		getBrandingValue(key: string) {
			const brand = this.branding?.find((item: any) => item.resource.key === key);
			return brand ? brand.resource.value : '';
		},

		getColorBrandingValue(key: string) {
			let color = null;
			const brand = this.branding?.find((item: any) => item.resource.key === key);
			if (brand && brand.resource.value) {
				let hex = brand.resource.value;
				// Check if the hex color is 3 digits
				if (/^#[0-9A-F]{3}$/i.test(hex)) {
					// Convert 3-digit hex to 6-digit hex
					hex = `#${hex[1]}${hex[1]}${hex[2]}${hex[2]}${hex[3]}${hex[3]}`;

					color = hex;
				} else if (/^#[0-9A-F]{6}$/i.test(hex)) {
					color = hex;
				} else if (/^rgb\((\d{1,3}),\s*(\d{1,3}),\s*(\d{1,3})\)$/i.test(hex)) {
					// Convert rgb to object with r, g, b properties
					const rgb = hex.match(/^rgb\((\d{1,3}),\s*(\d{1,3}),\s*(\d{1,3})\)$/i);
					color = {
						r: parseInt(rgb[1]),
						g: parseInt(rgb[2]),
						b: parseInt(rgb[3]),
					};
				}
			} else {
				color = brand ? brand.resource.value : '';
			}
			return color ?? '';
		},

		getColorBrandingFormat(key: string) {
			const brand = this.branding?.find((item: any) => item.resource.key === key);
			if (brand && brand.resource.value) {
				const hex = brand.resource.value;
				if (/^#[0-9A-F]{3}$/i.test(hex) || /^#[0-9A-F]{6}$/i.test(hex)) {
					return 'hex';
				} else if (/^rgb\((\d{1,3}),\s*(\d{1,3}),\s*(\d{1,3})\)$/i.test(hex)) {
					return 'rgb';
				}
			}
			return 'hex';
		},

		resetBrandingValue(key: string, value: string) {
			this.updateBrandingValue(key, value);
		},

		getOriginalBrandingValue(key: string) {
			const brand = this.brandingOriginal?.find((item: any) => item.resource.key === key);
			return brand ? brand.resource.value : '';
		},

		getContrastRatio(color1: string, color2: string) {
			const lum1 = getLuminance(color1);
			const lum2 = getLuminance(color2);
			const ratio = lum1 > lum2 ? (lum1 + 0.05) / (lum2 + 0.05) : (lum2 + 0.05) / (lum1 + 0.05);

			return Math.round(ratio * 100) / 100;
		},

		getBrandingDescription(key: string) {
			const brand = this.branding?.find((item: any) => item.resource.key === key);
			return brand ? brand.resource.description : '';
		},

		getFriendlyName(key: string) {
			return (
				key
					.split(':')
					.pop()
					?.replace(/([A-Z])/g, ' $1')
					.trim() || ''
			);
		},

		updateHtmlText(key: string, newContent: string) {
			if (newContent === this.getBrandingValue(key)) {
				return;
			}
			this.updateBrandingValue(key, newContent);
		},

		updateBrandingValue(key: string, newValue: string) {
			const isColorKey = this.orderedKeyColorsGrouped.some((group) =>
				group.keys.some((k) => k.key === key),
			);
			if (isColorKey) {
				if (/^[0-9a-fA-F]{6}$/.test(newValue)) {
					if (!newValue.startsWith('#')) {
						newValue = '#' + newValue;
					}
				} else if (
					typeof newValue === 'object' &&
					newValue !== null &&
					'r' in newValue &&
					'g' in newValue &&
					'b' in newValue
				) {
					newValue = `rgb(${newValue.r}, ${newValue.g}, ${newValue.b})`;
				}
			}

			const brand = this.branding?.find((item: any) => item.resource.key === key);
			if (brand) {
				brand.resource.value = newValue;
			}
		},

		setDefaultBranding() {
			this.branding.forEach((brand: any) => {
				const defaultBrand = this.brandingDefault.find(
					(defaultBrand: any) => defaultBrand.key === brand.resource.key,
				);
				if (defaultBrand) {
					brand.resource.value = defaultBrand.value;
				}
			});
		},

		cancelBrandingChanges() {
			this.branding = JSON.parse(JSON.stringify(this.brandingOriginal));
		},

		saveBranding() {
			const changedBranding = this.branding.filter((brand: any) => {
				const originalBrand = this.brandingOriginal.find(
					(original: any) => original.resource.key === brand.resource.key,
				);
				return originalBrand.resource.value !== brand.resource.value;
			});

			changedBranding.forEach(async (brand: any) => {
				const params = {
					type: brand.resource.type,
					name: brand.resource.name,
					display_name: brand.resource.display_name,
					description: brand.resource.description,
					key: brand.resource.key,
					value: brand.resource.value,
					content_type: brand.resource.content_type,
				};
				return await api.saveBranding(brand.resource.key, params);
			});

			try {
				this.loading = true;
				this.loadingStatusText = 'Applying branding changes...';
				this.brandingOriginal = JSON.parse(JSON.stringify(this.branding));
				this.$toast.add({
					severity: 'success',
					detail: 'Branding has been successfully updated.',
					life: 5000,
				});
			} catch (error) {
				this.$toast.add({
					severity: 'error',
					detail: error?.response?._data || error,
					life: 5000,
				});
			} finally {
				this.loading = false;
			}
		},

		contrastColorAA(backgroundKey, foregroundKey) {
			return this.getContrastRatio(
				this.getBrandingValue(backgroundKey),
				this.getBrandingValue(foregroundKey) || 'transparent',
			) >= 4.5
				? '#1a784c'
				: '#9e054a';
		},

		contrastColorAAA(backgroundKey, foregroundKey) {
			return this.getContrastRatio(
				this.getBrandingValue(backgroundKey),
				this.getBrandingValue(foregroundKey) || 'transparent',
			) >= 7
				? '#1a784c'
				: '#9e054a';
		},

		contrastRatioText(backgroundKey, foregroundKey) {
			if (!backgroundKey || !foregroundKey) {
				return;
			}
			const ratio = this.getContrastRatio(
				this.getBrandingValue(backgroundKey),
				this.getBrandingValue(foregroundKey) || 'transparent',
			);
			return `${ratio} : 1`;
		},

		contrastCompliance(backgroundKey, foregroundKey, requiredRatio) {
			const ratio = this.getContrastRatio(
				this.getBrandingValue(backgroundKey),
				this.getBrandingValue(foregroundKey) || 'transparent',
			);
			return ratio >= requiredRatio ? 'Pass' : 'Fail';
		},
	},
};
</script>

<style lang="scss" scoped>
.steps {
	display: grid;
	grid-template-columns: minmax(auto, 50%) minmax(auto, 50%);
	gap: 24px;
	position: relative;
}

.step-header {
	font-weight: bold;
	margin-bottom: -10px;
}

.quill-container {
	max-width: 80ch;
}

.divider {
	border-top: 3px solid #bbb;
	grid-column: span 2;
}

.button-container {
	display: grid;
	grid-template-columns: repeat(3, 1fr);
	gap: 14px;
}

.color-group-container {
	flex-direction: row !important;
	align-items: center;
	gap: 24px;
	border: 2px solid var(--primary-color);
	padding: 10px;
}

.color-group {
	// flex: 1;
}

.color-preview-container {
	flex: 1;
	display: flex;
	flex-direction: column;
	align-items: center;
}

.color-preview-background {
	height: 50px;
	display: flex;
	justify-content: center;
	align-items: center;
	border: 2px solid #000;
	min-width: 100px;
}

.color-preview-foreground {
	padding: 10px;
	font-weight: bold;
}

.branding-input {
	max-width: 80ch;
}

.branding-color-input {
	width: 30ch;
}

.logo-preview {
	display: flex;
	justify-content: center;
	align-items: center;
	height: 100px;
	border: 2px solid #000;
	margin-top: 10px;
	max-width: 300px;
	padding: 10px;
}

.logo-image {
	max-width: 148px;
	max-height: 100%;
}

.color-wcag-results-container {
	display: flex;
	justify-content: space-between;
	margin-top: 10px;
}

.color-wcag-results {
	display: flex;
	justify-content: space-between;
	min-width: 100px;
}

.color-wcag-result {
	display: flex;
	flex-direction: column;
	align-items: center;
	flex: 1;
}

.color-wcag-result-label {
	font-weight: bold;
}

.color-wcag-result-value {
	color: #000;
}

.color-input-container {
	display: flex;
	align-items: center;
}

.color-picker {
	width: 50px;
}

.color-undo-button {
	border: 2px solid #e1e1e1;
	border-width: 2px 2px 2px 0;
	width: 50px;
}

.p-colorpicker-preview {
	border-radius: 0px;
	height: 100%;
	border-left: 0px;
}

.color-ratio {
	text-align: center;
	font-weight: bold;
}

.quill-view-html {
	font-size: 1rem;
	color: #4b5563;
	font-weight: 550;
	border-radius: 4px;
	padding: 0.5rem;
	cursor: pointer;
	width: auto !important;
}

.grid__loading-overlay {
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

<style lang="scss">
.ql-container {
	height: auto;
}

.ql-editor {
	height: auto;
	max-height: 200px;
	font-family: 'Poppins', sans-serif;
}
</style>
