<template>
	<div>
		<Toast position="top-center" />
		<div class="navbar">
			<!-- Sidebar header -->
			<div class="navbar__header">
				<img
					v-if="$appConfigStore.logoUrl !== ''"
					:src="$appConfigStore.logoUrl"
					:alt="$appConfigStore.logoText"
				/>
				<span v-else>{{ $appConfigStore.logoText }}</span>

				<template v-if="!$appConfigStore.isKioskMode">
					<VTooltip :auto-hide="isMobile" :popper-triggers="isMobile ? [] : ['hover']">
						<Button
							class="navbar__header__button"
							:icon="$appStore.isSidebarClosed ? 'pi pi-arrow-right' : 'pi pi-arrow-left'"
							size="small"
							severity="secondary"
							aria-label="Toggle sidebar"
							:aria-expanded="!$appStore.isSidebarClosed"
							@click="$appStore.toggleSidebar"
							@keydown.esc="hideAllPoppers"
						/>
						<template #popper><div role="tooltip">Toggle sidebar</div></template>
					</VTooltip>
				</template>
			</div>

			<!-- Navbar content -->
			<div class="navbar__content">
				<div class="navbar__content__left">
					<div class="navbar__content__left__item">
						<template v-if="currentSession">
							<span class="current_session_name">{{ currentSession.display_name }}</span>
							<!-- <VTooltip :auto-hide="false" :popper-triggers="['hover']">
								<Button
									v-if="!$appConfigStore.isKioskMode"
									class="button--share"
									icon="pi pi-copy"
									text
									severity="secondary"
									aria-label="Copy link to chat session"
									@click="handleCopySession"
								/>
								<template #popper>Copy link to chat session</template>
							</VTooltip> -->
						</template>
						<template v-else>
							<span>Please select a session</span>
						</template>
						<template v-if="virtualUser">
							<span style="margin-left: 10px">{{ virtualUser }}</span>
						</template>
					</div>
				</div>

				<!-- Right side content -->
				<div class="navbar__content__right">
					<template v-if="currentSession">
						<span class="header__dropdown">
							<VTooltip :auto-hide="isMobile" :popper-triggers="isMobile ? [] : ['hover']">
								<AgentIcon
									:src="$appConfigStore.agentIconUrl || '~/assets/FLLM-Agent-Light.svg'"
									alt="Select an agent"
									tabindex="0"
									@keydown.esc="hideAllPoppers"
								/>
								<template #popper><div role="tooltip">Select an agent</div></template>
							</VTooltip>
							<Dropdown
								v-model="agentSelection"
								:options="agentOptionsGroup"
								:style="{ maxHeight: '300px' }"
								class="dropdown--agent"
								option-group-label="label"
								option-group-children="items"
								option-disabled="disabled"
								option-label="label"
								placeholder="--Select--"
								aria-label="Select an agent"
								aria-activedescendant="selected-agent-{{ agentSelection?.label }}"
								@change="handleAgentChange"
							/>
							<Button
								class="print-button"
								@click="handlePrint"
								aria-label="Print"
								icon="pi pi-print"
							/>
						</span>
					</template>
				</div>
			</div>
		</div>

		<!-- No agents message -->
		<template v-if="showNoAgentsMessage">
			<div class="no-agents">
				<!-- eslint-disable-next-line vue/no-v-html -->
				<div class="body" v-html="emptyAgentsMessage"></div>
			</div>
		</template>
	</div>
</template>

<script lang="ts">
import { hideAllPoppers } from 'floating-vue';
import eventBus from '@/js/eventBus';
import type { Session } from '@/js/types';

interface AgentDropdownOption {
	label: string;
	value: any;
	disabled?: boolean;
	my_agent?: boolean;
	type: string;
	object_id: string;
	description: string;
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
			virtualUser: null as string | null,
			isMobile: window.screen.width < 950,
			emptyAgentsMessage: null as string | null,
		};
	},

	computed: {
		currentSession() {
			return this.$appStore.currentSession;
		},

		showNoAgentsMessage() {
			return this.agentOptions.length === 0 && this.emptyAgentsMessage !== null;
		},
	},

	watch: {
		currentSession(newSession: Session, oldSession: Session) {
			if (newSession.id !== oldSession?.id) {
				this.updateAgentSelection();
			}
		},
		'$appStore.selectedAgents': {
			handler() {
				this.updateAgentSelection();
			},
			deep: true,
		},
		'$appStore.agents': {
			handler() {
				this.setAgentOptions();
			},
			deep: true,
		},
	},

	async created() {},

	mounted() {
		this.updateAgentSelection();
	},

	methods: {
		handleAgentChange() {
			this.$appStore.setSessionAgent(this.currentSession, this.agentSelection!.value);
			const message = this.agentSelection!.value
				? `Agent changed to ${this.agentSelection!.label}`
				: `Cleared agent hint selection`;

			this.$appStore.addToast({
				severity: 'success',
				detail: message,
			});

			if (this.$appStore.currentMessages?.length > 0) {
				// Emit the event to create a new session.
				// TODO: Add flag on the agent to determine whether to create a new session.
				eventBus.emit('agentChanged');
			}
		},

		async handleLogout() {
			await this.$authStore.logout();
		},

		handlePrint() {
			window.print();
    	},
    
		async setAgentOptions() {
			this.agentOptions = this.$appStore.agents.map((agent) => ({
				label: agent.resource.display_name ? agent.resource.display_name : agent.resource.name,
				type: agent.resource.type,
				object_id: agent.resource.object_id,
				description: agent.resource.description,
				my_agent: agent.roles.includes('Owner'),
				value: agent,
			}));

			if (this.agentOptions.length === 0) {
				this.emptyAgentsMessage = this.$appConfigStore.noAgentsMessage ?? null;
			}

			const publicAgentOptions = this.agentOptions.filter((agent) => !agent.my_agent);
			const privateAgentOptions = this.agentOptions.filter((agent) => agent.my_agent);
			const noAgentOptions = [
				{
					label: 'None',
					value: null,
					disabled: true,
					type: '',
					object_id: '',
					description: '',
				},
			];
			this.virtualUser = await this.$appStore.getVirtualUser();

			this.agentOptionsGroup.push({
				label: '',
				items: [
					{
						label: '--select--',
						value: null,
						type: '',
						object_id: '',
						description: '',
					},
				],
			});

			if (this.agentOptions.length === 0) {
				// Append noAgentOptions to the last entry in the agentOptionsGroup
				this.agentOptionsGroup[this.agentOptionsGroup.length - 1].items.push(...noAgentOptions);
				return;
			}

			if (privateAgentOptions.length > 0) {
				this.agentOptionsGroup.push({
					label: 'My Agents',
					items: privateAgentOptions,
				});
				this.agentOptionsGroup.push({
					label: 'Other Agents',
					items: publicAgentOptions.length > 0 ? publicAgentOptions : noAgentOptions,
				});
			} else {
				this.agentOptionsGroup[this.agentOptionsGroup.length - 1].items.push(
					...(publicAgentOptions.length > 0 ? publicAgentOptions : noAgentOptions),
				);
			}
		},

		// handleCopySession() {
		// 	const chatLink = `${window.location.origin}?chat=${this.currentSession!.id}`;
		// 	navigator.clipboard.writeText(chatLink);

		// 	this.$appStore.addToast({
		// 		severity: 'success',
		// 		detail: 'Chat link copied!',
		// 	});
		// },

		updateAgentSelection() {
			const agent = this.$appStore.getSessionAgent(this.currentSession);

			this.agentSelection =
				this.agentOptions.find(
					(option) => option.value.resource.object_id === agent.resource.object_id,
				) || null;

			if (this.agentSelection) {
				this.$appStore.setSessionAgent(this.currentSession, this.agentSelection.value);
			}
		},

		hideAllPoppers() {
			hideAllPoppers();
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
	border-bottom: 1px solid #eaeaea;
	color: var(--accent-text);
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
	color: var(--accent-text);
}

.button--auth {
	margin-left: 24px;
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

.navbar__header__button:focus {
	box-shadow: 0 0 0 0.1rem #fff;
}

.print-button {
	margin-left: 8px;
}

.no-agents {
	position: fixed;
	top: 60px;
	left: 50%;
	transform: translateX(-50%);
	background-color: #fafafa;
	box-shadow: 0 5px 10px 0 rgba(27, 29, 33, 0.1);
	border-radius: 6px;
	width: 55%;
	text-align: center;
	z-index: 1000;

	.body {
		color: #5f0000;
		padding: 10px 14px;
	}
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
.dropdown--agent:focus {
	box-shadow: 0 0 0 0.1rem #000;
}

@media only screen and (max-width: 545px) {
	.dropdown--agent .p-dropdown-label {
		/* display: none; */
	}
	.dropdown--agent .p-dropdown-trigger {
		height: 40px;
	}
	.current_session_name {
		display: none;
	}
}

@media only screen and (max-width: 500px) {
	.dropdown--agent {
		max-width: 200px;
	}
}

@media only screen and (max-width: 450px) {
	.dropdown--agent {
		max-width: 160px;
	}
}

.p-dropdown-items-wrapper {
	max-height: 300px !important;
}
</style>
