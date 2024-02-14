<template>
	<div class="navbar">
		<!-- Sidebar header -->
		<div class="navbar__header">
			<img v-if="$appConfigStore.logoUrl !== ''" :src="$appConfigStore.logoUrl" />
			<span v-else>{{ $appConfigStore.logoText }}</span>

			<template v-if="!$appConfigStore.isKioskMode">
				<Button
					:icon="$appStore.isSidebarClosed ? 'pi pi-arrow-right' : 'pi pi-arrow-left'"
					size="small"
					severity="secondary"
					class="secondary-button"
					@click="$appStore.toggleSidebar"
				/>
			</template>
		</div>

		<!-- Navbar content -->
		<div class="navbar__content">
			<div class="navbar__content__left">
				<div class="navbar__content__left__item">
					<template v-if="currentSession">
						<span>{{ currentSession.name }}</span>
						<Button
							v-if="!$appConfigStore.isKioskMode"
							v-tooltip.bottom="'Copy link to chat session'"
							class="button--share"
							icon="pi pi-copy"
							text
							severity="secondary"
							@click="handleCopySession"
						/>
						<Toast position="top-center" />
					</template>
					<template v-else>
						<span>Please select a session</span>
					</template>
				</div>
			</div>

			<!-- Right side content -->
			<div class="navbar__content__right">
				<template v-if="currentSession && $appConfigStore.allowAgentHint">
					<span class="header__dropdown">
						<img alt="Select an agent" class="avatar" v-tooltip.bottom="'Select an agent'" src="~/assets/FLLM-Agent-Light.svg">
						<Dropdown
							v-model="agentSelection"
							class="dropdown--agent"
							:options="agentOptionsGroup"
							option-group-label="label"
							option-group-children="items"
							optionDisabled="disabled"
							option-label="label"
							placeholder="--Select--"
							@change="handleAgentChange"
						/>
					</span>
				</template>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import type { Session } from '@/js/types';

interface AgentDropdownOption {
	label: string;
	value: any;
	disabled?: boolean;
	private?: boolean;
}

interface AgentDropdownOptionsGroup {
	label: string;
	items: AgentDropdownOption[];
}

export default {
	name: 'NavBar',

	data() {
		return {
			agentSelection: null as AgentDropdownOption | null,
			agentOptions: [] as AgentDropdownOption[],
			agentOptionsGroup: [] as AgentDropdownOptionsGroup[],
		};
	},

	computed: {
		currentSession() {
			return this.$appStore.currentSession;
		},
	},

	watch: {
		currentSession(newSession: Session, oldSession: Session) {
			if (newSession.id === oldSession?.id) return;

			this.agentSelection =
				this.agentOptions.find(
					(agent) => agent.value === this.$appStore.getSessionAgent(newSession),
				) || null;
		},
	},

	async created() {
		await this.$appStore.getAgents();

		this.agentOptions = this.$appStore.agents.map((agent) => ({
			label: agent.name,
			private: agent.private,
			value: agent,
		}));

		const publicAgentOptions = this.agentOptions.filter((agent) => !agent.private);
		const privateAgentOptions = this.agentOptions.filter((agent) => agent.private);
		const noAgentOptions = [{ label: 'None', value: null, disabled: true }];

		this.agentOptionsGroup.push({
			label: '',
			items: [{ label: '--select--', value: null }],
		});

		this.agentOptionsGroup.push({
			label: 'Public',
			items: publicAgentOptions.length > 0 ? publicAgentOptions : noAgentOptions,
		});

		this.agentOptionsGroup.push({
			label: 'Private',
			items: privateAgentOptions.length > 0 ? privateAgentOptions : noAgentOptions,
		});
	},

	methods: {
		handleCopySession() {
			const chatLink = `${window.location.origin}?chat=${this.currentSession!.id}`;
			navigator.clipboard.writeText(chatLink);

			this.$toast.add({
				severity: 'success',
				detail: 'Chat link copied!',
				life: 2000,
			});
		},

		handleAgentChange() {
			this.$appStore.setSessionAgent(this.currentSession, this.agentSelection!.value);
			const message = this.agentSelection!.value
				? `Agent changed to ${this.agentSelection!.label}`
				: `Cleared agent hint selection`;

			this.$toast.add({
				severity: 'success',
				detail: message,
				life: 2000,
			});
		},

		async handleLogout() {
			await this.$authStore.logout();
		},
	},
};
</script>

<style lang="scss" scoped>
.navbar {
	height: 70px;
	width: 100%;
	overflow: hidden;
	display: flex;
	flex-direction: row;
	box-shadow: 0 5px 10px 0 rgba(27, 29, 33, 0.1);
}

.navbar--collapsed {
	.navbar__content {
		background-color: var(--primary-color);
		justify-content: flex-end;
		border-bottom: none;
	}
}

.navbar__header {
	width: 300px;
	padding-right: 24px;
	padding-left: 24px;
	padding-top: 12px;
	padding-bottom: 12px;
	display: flex;
	align-items: center;
	justify-content: space-between;
	color: var(--primary-text);
	background-color: var(--primary-color);

	img {
		max-height: 100%;
		width: auto;
		max-width: 148px;
		margin-right: 12px;
	}
}

.navbar__content {
	flex: 1;
	display: flex;
	justify-content: space-between;
	align-items: center;
	padding: 24px;
	border-bottom: 1px solid #EAEAEA;
	background-color: var(--accent-color);
}

.navbar__content__left {
	display: flex;
	align-items: center;
}

.navbar__content__left__item {
	display: flex;
	align-items: center;
}

.navbar__content__right__item {
	display: flex;
	align-items: center;
}

.button--share {
	margin-left: 8px;
}

.button--auth {
	margin-left: 24px;
}

.secondary-button {
	background-color: var(--secondary-button-bg)!important;
	border-color: var(--secondary-button-bg)!important;
	color: var(--secondary-button-text)!important;
}

.header__dropdown {
	display: flex;
	align-items: center;
}

.avatar {
	width: 32px;
	height: 32px;
	border-radius: 50%;
	margin-right: 12px;
}

@media only screen and (max-width: 620px) {
	.navbar__header {
		width: 95px;
		justify-content: center;

		img {
			display: none;
		}
	}
}
</style>

<style>
@media only screen and (max-width: 545px) {
	.dropdown--agent .p-dropdown-label {
		display: none;
	}
	.dropdown--agent .p-dropdown-trigger {
		height: 40px;
	}
}
</style>
