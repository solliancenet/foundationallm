<template>
	<div class="login-page">
		<div class="login-container">
			<img :src="$appConfigStore.logoUrl" class="login__logo" />
			<Button class="primary-button" icon="pi pi-microsoft" label="Sign in" size="large" @click="signIn"></Button>
			<div v-if="$route.query.message" class="login__message">{{ $route.query.message }}</div>
		</div>
	</div>
</template>

<script setup lang="ts">
definePageMeta({
	name: 'auth/login',
	path: '/signin-oidc',
});
</script>

<script lang="ts">
export default {
	name: 'Login',

	methods: {
		async signIn() {
			await this.$authStore.login();
			if (this.$authStore.isAuthenticated) {
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

.login__logo {
	width: 300px;
	height: auto;
	margin-bottom: 48px;
}

.login__message {
	margin-top: 16px;
	padding: 16px;
	color: var(--primary-text);
	font-size: 1rem;
}

.primary-button {
	background-color: var(--primary-button-bg) !important;
	border-color: var(--primary-button-bg) !important;
	color: var(--primary-button-text) !important;
}
</style>
