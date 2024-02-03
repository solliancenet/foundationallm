import { LogLevel, PublicClientApplication, EventType } from '@azure/msal-browser';

const ENABLE_LOGS = false;

export interface AuthConfigOptions {
	clientId: string;
	instance: string;
	tenantId: string;
	scopes: string;
	callbackPath: string;
}

let configOptions: AuthConfigOptions = {
	clientId: '',
	instance: '',
	tenantId: '',
	scopes: '',
	callbackPath: '',
};

export function setAuthConfig(config: AuthConfigOptions) {
	configOptions = config;
}

function getMsalConfig() {
	return {
		auth: {
			clientId: configOptions.clientId,
			authority: `${configOptions.instance}${configOptions.tenantId}`,
			redirectUri: configOptions.callbackPath,
			scopes: [configOptions.scopes],
			// Must be registered as a SPA redirectURI on your app registration.
			postLogoutRedirectUri: '/',
		},
		cache: {
			cacheLocation: 'sessionStorage',
		},
		system: {
			loggerOptions: {
				loggerCallback: (level: LogLevel, message: string, containsPii: boolean) => {
					if (!ENABLE_LOGS) return;

					if (containsPii) {
						return;
					}

					switch (level) {
						case LogLevel.Error:
							console.error(message);
							return;
						case LogLevel.Info:
							console.info(message);
							return;
						case LogLevel.Verbose:
							console.debug(message);
							return;
						case LogLevel.Warning:
							console.warn(message);
							return;
					}
				},
				logLevel: LogLevel.Verbose,
			},
		},
	};
}

// Create a timer to refresh the token on expiration.
let tokenExpirationTimer: any;
export async function createTokenRefreshTimer() {
	const accounts = await msalInstance.getAllAccounts();
	if (accounts.length > 0) {
		const account = accounts[0];
		if (account.idTokenClaims?.exp) {
			const tokenExpirationTime = account.idTokenClaims.exp * 1000;
			const currentTime = Date.now();
			const timeUntilExpiration = tokenExpirationTime - currentTime;

			if (timeUntilExpiration <= 0) {
				console.log(`Access token expired ${timeUntilExpiration / 1000} seconds ago.`);
				return;
			}

			clearTimeout(tokenExpirationTimer);

			tokenExpirationTimer = setTimeout(() => {
				refreshToken(account);
			}, timeUntilExpiration);

			console.log(`Set access token timer refresh in ${timeUntilExpiration / 1000} seconds.`);
		}
	}
}

export async function refreshToken(account: any) {
	try {
		await msalInstance.acquireTokenSilent({
			account,
			scopes: [configOptions.scopes],
		});
		console.log('Refreshed access token.');
		createTokenRefreshTimer();
	} catch (error) {
		console.error('Token refresh error:', error);
		sessionStorage.clear();
		window.location = configOptions.callbackPath;
	}
}

let msalInstance: any;
export async function getMsalInstance() {
	if (msalInstance) return msalInstance;

	msalInstance = new PublicClientApplication(getMsalConfig());

	msalInstance.addEventCallback((event) => {
		if (event.eventType === EventType.LOGIN_SUCCESS) {
			createTokenRefreshTimer();
		}
	});

	await msalInstance.initialize();
	return msalInstance;
}

// Add scopes here for the id token to be used at MS Identity Platform endpoints.
export function getLoginRequest() {
	const msalConfig = getMsalConfig();
	return {
		scopes: msalConfig.auth.scopes,
	};
}

// Add the endpoints here for MS Graph API services you would like to use.
export const graphConfig = {
	graphMeEndpoint: 'https://graph.microsoft.com/v1.0/me',
	graphMailEndpoint: 'https://graph.microsoft.com/v1.0/me/messages',
};
