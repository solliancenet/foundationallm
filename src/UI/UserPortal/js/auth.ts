import { LogLevel, PublicClientApplication } from '@azure/msal-browser';
declare const AUTH_CLIENT_ID: string;
declare const AUTH_AUTHORITY: string;

// Config object to be passed to Msal on creation
export const msalConfig = {
	auth: {
		clientId: `${AUTH_CLIENT_ID}`,
		authority: `${AUTH_AUTHORITY}`,
		redirectUri: '/', // Must be registered as a SPA redirectURI on your app registration
		postLogoutRedirectUri: '/', // Must be registered as a SPA redirectURI on your app registration
	},
	cache: {
		cacheLocation: 'sessionStorage',
	},
	system: {
		loggerOptions: {
			loggerCallback: (level: LogLevel, message: string, containsPii: boolean) => {
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
					default:
						return;
				}
			},
			logLevel: LogLevel.Verbose,
		},
	},
};

export const msalInstance = new PublicClientApplication(msalConfig);

// Add here scopes for id token to be used at MS Identity Platform endpoints.
export const loginRequest = {
	scopes: ['User.Read'],
};

// Add here the endpoints for MS Graph API services you would like to use.
export const graphConfig = {
	graphMeEndpoint: 'https://graph.microsoft.com/v1.0/me',
	graphMailEndpoint: 'https://graph.microsoft.com/v1.0/me/messages',
};
