<template>
	<div class="login-page">
		<div class="login-container">
			<img :src="$appConfigStore.logoUrl" class="logo" />
			<Button icon="pi pi-microsoft" label="Sign in" size="large" @click="signIn"></Button>
		</div>
	</div>
</template>

<script setup lang="ts">
definePageMeta({
	name: 'auth/login',
	path: '/signin-oidc',
	layout: false,
});
</script>

<script lang="ts">
export default {
	name: 'Login',

	methods: {
		async signIn() {
			const response = await this.$authStore.login();
			if (response.account) {
				this.$router.push({ path: '/', query: this.$nuxt._route.query });
			}
		},
	},
};
</script>

<style lang="scss" scoped>
.login-page {
	display: flex;
	align-items: center;
	justify-content: center;
	height: 100%;
	background-color: var(--primary-color);
	background: linear-gradient(45deg, var(--primary-color) 0%, var(--secondary-color) 50%);
}

.login-container {
	display: flex;
	flex-direction: column;
	align-items: center;
	justify-content: center;
	width: 500px;
	height: auto;
	padding: 48px;
	background-color: rgba(255, 255, 255, 0.05);
	backdrop-filter: blur(300px);
}

.logo {
	width: 300px;
	height: auto;
	margin-bottom: 48px;
}
</style>
