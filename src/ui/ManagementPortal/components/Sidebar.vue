<template>
	<div class="sidebar">
		<!-- Sidebar section header -->
		<div class="sidebar__header">
			<template v-if="$appConfigStore.logoUrl">
				<NuxtLink to="/">
					<img :src="$filters.publicDirectory($appConfigStore.logoUrl)"
					/>
				</NuxtLink>
			</template>
			<template v-else>
				<NuxtLink to="/">{{ $appConfigStore.logoText }}</NuxtLink>
			</template>
		</div>

		<!-- Agents -->
		<div class="sidebar__section-header">
			<span class="pi pi-users"></span>
			<span>Agents</span>
		</div>

		<NuxtLink to="/agents/create" class="sidebar__item">Create New Agent</NuxtLink>
		<NuxtLink to="/agents/public" class="sidebar__item">All Agents</NuxtLink>
		<NuxtLink to="/agents/private" class="sidebar__item">My Agents</NuxtLink>
		<div class="sidebar__item">Performance</div>

		<!-- Data Catalog -->
		<div class="sidebar__section-header">
			<span class="pi pi-database"></span>
			<span>Data Catalog</span>
		</div>

		<NuxtLink to="/data-sources" class="sidebar__item">Data Sources</NuxtLink>
		<div class="sidebar__item">Vector Stores</div>

		<!-- Quotas -->
		<div class="sidebar__section-header">
			<span class="pi pi-calculator"></span>
			<span>Quotas</span>
		</div>

		<div class="sidebar__item">Policies</div>

		<!-- LLM's -->
		<div class="sidebar__section-header">
			<span class="pi pi-sitemap"></span>
			<span>LLM's</span>
		</div>

		<div class="sidebar__item">Language Models & Endpoints</div>

		<!-- Security -->
		<div class="sidebar__section-header">
			<span class="pi pi-shield"></span>
			<span>Security</span>
		</div>

		<NuxtLink to="/security/role-assignments" class="sidebar__item">Instance Access Control</NuxtLink>

		<!-- FLLM Deployment -->
		<div class="sidebar__section-header">
			<span class="pi pi-cloud"></span>
			<span>FLLM Platform</span>
		</div>

		<NuxtLink to="/info" class="sidebar__item">Deployment Information</NuxtLink>

		<!-- Logged in user -->
		<div v-if="$authStore.currentAccount?.name" class="sidebar__account">
			<Avatar icon="pi pi-user" class="sidebar__avatar" size="large" />
			<div>
				<span class="sidebar__username">{{ $authStore.currentAccount?.name }}</span>
				<Button
					class="sidebar__sign-out-button secondary-button"
					icon="pi pi-sign-out"
					label="Sign Out"
					severity="secondary"
					size="small"
					@click="$authStore.logout()"
				/>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
export default {
	name: 'Sidebar',
};
</script>

<style lang="scss" scoped>
a {
	text-decoration: none;
}

.sidebar {
	width: 300px;
	max-width: 100%;
	height: 100%;
	display: flex;
	flex-direction: column;
	background-color: var(--primary-color);
	z-index: 3;
	flex-shrink: 0;
	overflow-y: scroll;
}

.sidebar__header {
	height: 70px;
	width: 100%;
	padding-right: 24px;
	padding-left: 24px;
	padding-top: 12px;
	/*display: flex;*/
	align-items: center;
	color: var(--primary-text);

	img {
		max-height: 100%;
		width: auto;
		max-width: 180px;
		margin-right: 12px;
	}
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
</style>
