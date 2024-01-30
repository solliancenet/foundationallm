<template>
	<div class="sidebar">
		<!-- Sidebar section header -->
		<div class="sidebar__header">
			<img v-if="$appConfigStore.logoUrl" :src="$filters.enforceLeadingSlash($appConfigStore.logoUrl)" />
			<span v-else>{{ $appConfigStore.logoText }}</span>
		</div>

		<!-- Agents -->
		<div class="sidebar__section-header">
			<span class="pi pi-users"></span>
			<span>Agents</span>
		</div>

		<NuxtLink to="/agents/create" class="sidebar__item">Create New Agent</NuxtLink>
		<NuxtLink to="/agents/public" class="sidebar__item">Public Agents</NuxtLink>
		<NuxtLink to="/agents/private" class="sidebar__item">Private Agents</NuxtLink>
		<div class="sidebar__item">Performance</div>

		<!-- Data Catalog -->
		<div class="sidebar__section-header">
			<span class="pi pi-database"></span>
			<span>Data Catalog</span>
		</div>

		<div class="sidebar__item">Data Sources</div>
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

		<div class="sidebar__item">Identity & Access Management (IAM)</div>
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
}

.sidebar__header {
	height: 70px;
	width: 100%;
	padding-right: 24px;
	padding-left: 24px;
	padding-top: 12px;
	display: flex;
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

	&.router-link-active, &:hover {
		background-color: rgba(217, 217, 217, 0.05);
	}
}
</style>
