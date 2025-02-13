<template>
	<div
		:class="$appStore.sidebarCollapsed ? 'sidebar sidebar-collapsed' : 'sidebar'"
		role="navigation"
	>
		<h2 id="sidebar-title" class="visually-hidden">Main Navigation</h2>
		<!-- Sidebar section header -->
		<div class="sidebar__header">
			<template v-if="$appConfigStore.logoUrl">
				<NuxtLink to="/" aria-label="Navigate to homepage">
					<img
						:src="$filters.publicDirectory($appConfigStore.logoUrl)"
						aria-label="Logo as link to home"
						:alt="$appConfigStore.logoText"
					/>
				</NuxtLink>
			</template>
			<template v-else>
				<NuxtLink to="/">{{ $appConfigStore.logoText }}</NuxtLink>
			</template>
			<VTooltip :auto-hide="isMobile" :popper-triggers="isMobile ? [] : ['hover']">
				<Button
					:icon="$appStore.sidebarCollapsed ? 'pi pi-arrow-right' : 'pi pi-arrow-left'"
					:aria-label="$appStore.sidebarCollapsed ? 'Open Sidebar' : 'Close Sidebar'"
					@click="$appStore.sidebarCollapsed = !$appStore.sidebarCollapsed"
				/>
				<template #popper>
					<div role="tooltip">
						{{ $appStore.sidebarCollapsed ? 'Open Sidebar' : 'Close Sidebar' }}
					</div>
				</template>
			</VTooltip>
		</div>

		<div class="sidebar__content" v-if="!$appStore.sidebarCollapsed">
			<div class="sidebar__navigation">
				<!-- Agents -->
				<h3 class="sidebar__section-header">
					<span class="pi pi-users" aria-hidden="true"></span>
					<span>Agents</span>
				</h3>
				<ul>
					<li><NuxtLink to="/agents/create" class="sidebar__item">Create New Agent</NuxtLink></li>
					<li><NuxtLink to="/agents/public" class="sidebar__item">All Agents</NuxtLink></li>
					<li><NuxtLink to="/agents/private" class="sidebar__item">My Agents</NuxtLink></li>
					<li><NuxtLink to="/prompts" class="sidebar__item">Prompts</NuxtLink></li>
				</ul>
				<!-- <div class="sidebar__item">Performance</div> -->

				<!-- Data Catalog -->
				<h3 class="sidebar__section-header">
					<span class="pi pi-database" aria-hidden="true"></span>
					<span>Data Catalog</span>
				</h3>
				<ul>
					<li><NuxtLink to="/data-sources" class="sidebar__item">Data Sources</NuxtLink></li>
					<li><NuxtLink to="/vector-stores" class="sidebar__item">Vector Stores</NuxtLink></li>
				</ul>

				<!-- Models and Endpoints -->
				<h3 class="sidebar__section-header">
					<span class="pi pi-box" aria-hidden="true"></span>
					<span>Models and Endpoints</span>
				</h3>
				<ul>
					<li>
						<NuxtLink to="/models" class="sidebar__item">AI Models</NuxtLink>
					</li>
					<li>
						<NuxtLink to="/api-endpoints" class="sidebar__item">API Endpoints</NuxtLink>
					</li>
				</ul>

				<!-- Security -->
				<h3 class="sidebar__section-header">
					<span class="pi pi-shield" aria-hidden="true"></span>
					<span>Security</span>
				</h3>
				<ul>
					<li>
						<NuxtLink to="/security/role-assignments" class="sidebar__item">
							Instance Access Control
						</NuxtLink>
					</li>
				</ul>

				<!-- FLLM Deployment -->
				<h3 class="sidebar__section-header">
					<span class="pi pi-cloud" aria-hidden="true"></span>
					<span>FLLM Platform</span>
				</h3>
				<ul>
					<li><NuxtLink to="/branding" class="sidebar__item">Branding</NuxtLink></li>
					<li><NuxtLink to="/info" class="sidebar__item">Deployment Information</NuxtLink></li>
				</ul>
			</div>

			<!-- Logged in user -->
			<div v-if="$authStore.currentAccount?.name" class="sidebar__account">
				<UserAvatar
					class="sidebar__avatar"
					size="large"
					:aria-label="`User Avatar for ${$authStore.currentAccount?.name}`"
				/>

				<div>
					<VTooltip :auto-hide="isMobile" :popper-triggers="isMobile ? [] : ['hover']">
						<span
							class="sidebar__username"
							aria-label="Logged in as {{ $authStore.currentAccount?.username }}"
						>
							{{ $authStore.currentAccount?.name }}
						</span>
						<template #popper>
							<div role="tooltip">Logged in as {{ $authStore.currentAccount?.username }}</div>
						</template>
					</VTooltip>
					<Button
						class="sidebar__sign-out-button"
						icon="pi pi-sign-out"
						label="Sign Out"
						severity="secondary"
						size="small"
						@click="$authStore.logout()"
						aria-label="Sign out of the application"
					/>
				</div>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
export default {
	name: 'Sidebar',

	data() {
		return {
			isMobile: window.screen.width < 950,
		};
	},

	created() {
		this.$appStore.initializeSidebarState();
	},
};
</script>

<style lang="scss" scoped>
a {
	text-decoration: none;
}

.sidebar {
	width: 300px;
	max-width: 300px;
	height: 100%;
	display: flex;
	flex-direction: column;
	background-color: var(--primary-color);
	z-index: 3;
	flex-shrink: 1;
	flex-grow: 1;
	overflow-y: auto;
}

.sidebar-collapsed {
	position: absolute;
	background-color: transparent;
	width: 100%;
	max-width: 100%;
	height: max-content;
}

.sidebar ul {
	list-style-type: none;
	padding: 0;
	margin: 0;
}

.sidebar li {
	margin: 0;
	padding: 0;
}

.sidebar__header {
	display: flex;
	align-items: center;
	justify-content: space-between;
	background-color: var(--primary-color);
	height: 70px;
	width: 300px;
	padding-right: 24px;
	padding-left: 24px;
	padding-top: 12px;
	padding-bottom: 12px;
	color: var(--primary-text);

	img {
		max-height: 100%;
		width: 100%;
		max-width: 180px;
		margin-right: 12px;
	}
}

.sidebar__content {
	display: flex;
	height: 100%;
	flex-direction: column;
	flex-wrap: nowrap;
	align-items: stretch;
	overflow: hidden;
}

.sidebar__navigation {
	overflow-y: auto;
}

.sidebar__section-header {
	display: flex;
	align-items: center;
	gap: 10px;
	padding-bottom: 12px;
	padding-top: 12px;
	margin-left: 24px;
	margin-right: 24px;
	margin-bottom: 8px;
	margin-top: 24px;
	border-bottom: 1px solid rgba(255, 255, 255, 0.3);
	color: var(--primary-text);
	text-transform: uppercase;
	font-size: 0.95rem;
	font-weight: 600;
}

.sidebar__item {
	padding-bottom: 16px;
	padding-top: 16px;
	padding-left: 32px;
	padding-right: 32px;
	display: flex;
	justify-content: space-between;
	align-items: center;
	color: var(--primary-text);
	transition: all 0.1s ease-in-out;
	font-size: 0.8725rem;

	&.router-link-active,
	&:hover {
		background-color: rgba(217, 217, 217, 0.05);
	}
}

.sidebar__account {
	flex-grow: 1;
	color: var(--primary-text);
	display: grid;
	grid-template-columns: auto auto;
	align-items: end;
	justify-content: flex-start;
	padding: 12px 24px;
	text-transform: inherit;
}

.sidebar__avatar {
	margin-right: 12px;
	color: var(--primary-color);
	height: 61px;
	width: 61px;
}

.sidebar__username {
	color: var(--primary-text);
	font-size: 0.875rem;
	text-transform: capitalize;
	line-height: 0;
	vertical-align: super;
}

.sidebar__sign-out-button {
	width: 100%;
}

.visually-hidden {
	position: absolute;
	overflow: hidden;
	clip: rect(0 0 0 0);
	height: 1px;
	width: 1px;
	margin: -1px;
	padding: 0;
	border: 0;
}

@media only screen and (max-width: 960px) {
	.sidebar {
		position: absolute;
	}
}
</style>
