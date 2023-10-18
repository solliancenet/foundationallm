<template>
	<div class="navbar">
		<!-- Sidebar header -->
		<div class="navbar__header">
			<img :src="logoURL" />
			<span>{{ logoText }}</span>
		</div>
        <div class="navbar__content">
            <div class="navbar__content__left">
                <div class="navbar__content__left__item">
                    <template v-if="currentSession">
                        <span>{{ currentSession.name }}</span>
                    </template>
                    <template v-else>
                        <span>Please select a session</span>
                    </template>
                </div>
            </div>
            <div class="navbar__content__right">
                <div class="navbar__content__right__item" v-if="!signedIn">
                    <Button icon="pi pi-sign-in" label="Sign In" @click="signIn()"></Button>
                </div>
                <div class="navbar__content__right__item" v-else>
                    <span>Welcome {{ accountName }}</span>
                    <Button class="sign-out-button" icon="pi pi-sign-out" label="Sign Out" @click="signOut()"></Button>
                </div>
            </div>
        </div>
	</div>
</template>

<script lang="ts">
import type { PropType } from 'vue';
import { Session } from '@/js/types';
import { msalInstance, loginRequest } from '@/js/auth'

export default {
	name: 'Navbar',

	props: {
		currentSession: {
			type: Object as PropType<Session> | null,
			required: false,
		}
	},

	emits: ['change-session'],

	data() {
		return {
			logoText: this.$config.public.LOGO_TEXT,
			logoURL: this.$config.public.LOGO_URL,
            signedIn: false,
            accountName: '',
            userName: '',
		};
	},

	methods: {
        async signIn() {
			await msalInstance.loginPopup(loginRequest).then((response) => {
				if(response.account) {
					console.log(response.account);
                    this.signedIn = true;
                    this.accountName = response.account.name;
                    this.userName = response.account.username;
				}
			}).catch((error) => {
				console.log(error);
			});
		},

        async signOut() {
            const logoutRequest = {
                account: msalInstance.getAccountByUsername(this.userName)
            };
            await msalInstance.logoutPopup(logoutRequest).then((response) => {
                console.log(response);
                this.signedIn = false;
                this.accountName = '';
                this.userName = '';
            }).catch((error) => {
                console.log(error);
            });
            // await msalInstance.logout();
        }
	},

    async created() {
        await msalInstance.initialize().then(async () => {
            const accounts = await msalInstance.getAllAccounts();
            console.log(accounts);
            if(accounts.length > 0) {
                this.signedIn = true;
                this.accountName = accounts[0].name;
                this.userName = accounts[0].username;
            }
        }).catch((error) => {
            console.log(error);
        })
    }
};
</script>

<style lang="scss" scoped>
.navbar {
	height: 70px;
    width: 100%;
	display: flex;
	flex-direction: row;
}

.navbar__header {
	width: 300px;
	padding-right: 24px;
	padding-left: 24px;
	padding-top: 12px;
	display: flex;
	align-items: center;
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

.navbar__content__right__item {
    display: flex;
    align-items: center;
}

.sign-out-button {
    margin-left: 12px;
}
</style>
