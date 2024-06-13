<template>
	<div class="wrapper">
		<Sidebar />
		<div class="page">
			<!-- Page to render -->
			<div class="page-content">
				<slot />
			</div>

			<!-- Session expired dialog -->
			<Dialog
				modal
				:visible="$authStore.isExpired"
				:closable="false"
				header="Your session has expired."
			>
				Please log in again to continue using the app.
				<template #footer>
					<Button label="Log in" primary @click="handleRefreshLogin" />
				</template>
			</Dialog>

			<Footer v-if="$appConfigStore.footerText">
				<FooterItem v-html="$appConfigStore.footerText"></FooterItem>
			</Footer>
		</div>
	</div>
</template>

<script>
export default {
	methods: {
		async handleRefreshLogin() {
			await this.$authStore.logoutSilent();
			this.$router.push({ name: 'auth/login' });
		},
	},
};
</script>

<style scoped>
.page {
	display: flex;
	flex-direction: column;
	min-height: 100vh;
}

.page-content {
	flex: 1;
}

footer {
	text-align: right;
	font-size: 0.85rem;
    margin-top: 24px;
	padding-right: 24px;
}
</style>
