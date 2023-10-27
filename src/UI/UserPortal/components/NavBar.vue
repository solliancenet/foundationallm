<template>
	<div class="navbar" :class="{ 'navbar-collapsed': collapseSidebar }">
		<!-- Sidebar header -->
		<div class="navbar__header">
			<img :src="logoURL" v-if="logoURL !== ''"/>
			<span v-else>{{ logoText }}</span>
			<Button v-if="collapsedSidebar" icon="pi pi-arrow-right" size="small" severity="secondary" @click="collapseSidebar(false)" />
			<Button v-else icon="pi pi-arrow-left" size="small" severity="secondary" @click="collapseSidebar(true)" />
		</div>

		<!-- Sidebar -->
		<div class="navbar__content">
			<div class="navbar__content__left">
				<div class="navbar__content__left__item">
					<template v-if="currentSession">
						<span>{{ currentSession.name }}</span>
						<Button
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
				<!-- Auth button -->
				<div v-if="!signedIn" class="navbar__content__right__item">
					<Button class="button--auth" icon="pi pi-sign-in" label="Sign In" @click="signIn()"></Button>
				</div>
				<!-- Logged in user name -->
				<div v-else class="navbar__content__right__item">
					<span>Welcome {{ accountName }}</span>
					<Button class="button--auth" icon="pi pi-sign-out" label="Sign Out" @click="signOut()"></Button>
				</div>
			</div>
		</div>
	</div>
</template>

<script lang="ts">
import type { PropType } from 'vue';
import type { Session } from '@/js/types';
import { msalInstance, loginRequest } from '@/js/auth';
import getAppConfigSetting from '@/js/config';

export default {
	name: 'NavBar',

	props: {
		currentSession: {
			type: [Object, null] as PropType<Session | null>,
			required: true,
		},
	},

	emits: ['collapse-sidebar'],

	data() {
		return {
			logoText: '',
			logoURL: '',
			collapsedSidebar: false,
			signedIn: false,
			accountName: '',
			userName: '',
		};
	},

	async created() {
		if (process.client) {
			await msalInstance.initialize();
			const accounts = await msalInstance.getAllAccounts();
			if (accounts.length > 0) {
				this.signedIn = true;
				this.accountName = accounts[0].name;
				this.userName = accounts[0].username;
			}
		}
		this.logoText = await getAppConfigSetting("FoundationaLLM:Branding:LogoText");
		this.logoURL = await getAppConfigSetting("FoundationaLLM:Branding:LogoUrl");
	},

	methods: {
		collapseSidebar(collapsed: boolean) {
			this.collapsedSidebar = collapsed;
			this.$emit('collapse-sidebar', collapsed);
		},

		handleCopySession() {
			const chatLink = `${window.location.origin}?chat=${this.currentSession!.id}`;
			navigator.clipboard.writeText(chatLink);

			this.$toast.add({
				severity: 'success',
				detail: 'Chat link copied!',
				life: 2000,
			});
		},

		async signIn() {
			const response = await msalInstance.loginPopup(loginRequest);
			if (response.account) {
				this.signedIn = true;
				this.accountName = response.account.name;
				this.userName = response.account.username;
			}
		},

		async signOut() {
			const logoutRequest = {
				account: msalInstance.getAccountByUsername(this.userName),
			};

			await msalInstance.logoutPopup(logoutRequest);
			this.signedIn = false;
			this.accountName = '';
			this.userName = '';
			this.$router.push({ path: '/login' });
			// await msalInstance.logout();
		}
	},
};
</script>

<style lang="scss" scoped>
.navbar {
	height: 70px;
	width: 100%;
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
</style>
