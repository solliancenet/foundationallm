import { LogLevel, PublicClientApplication } from '@azure/msal-browser';
import getAppConfigSetting from './config';

const AUTH_CLIENT_ID = await getAppConfigSetting("FoundationaLLM:Chat:Entra:ClientId");
const AUTH_INSTANCE = await getAppConfigSetting("FoundationaLLM:Chat:Entra:Instance");
const AUTH_TENANT_ID = await getAppConfigSetting("FoundationaLLM:Chat:Entra:TenantId");
const AUTH_SCOPES = await getAppConfigSetting("FoundationaLLM:Chat:Entra:Scopes");
const AUTH_CLIENT_SECRET = await getAppConfigSetting("FoundationaLLM:Chat:Entra:ClientSecret");
const AUTH_CALLBACK_PATH = await getAppConfigSetting("FoundationaLLM:Chat:Entra:CallbackPath");

export const msalConfig = {
	auth: {
		clientId: `${AUTH_CLIENT_ID}`,
		authority: `${AUTH_INSTANCE}${AUTH_TENANT_ID}`,
		clientSecret: `${AUTH_CLIENT_SECRET}`,
		redirectUri: `${AUTH_CALLBACK_PATH}`, // Must be registered as a SPA redirectURI on your app registration
		scopes: [`${AUTH_SCOPES}`],
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
	scopes: msalConfig.auth.scopes,
};

// Add here the endpoints for MS Graph API services you would like to use.
export const graphConfig = {
	graphMeEndpoint: 'https://graph.microsoft.com/v1.0/me',
	graphMailEndpoint: 'https://graph.microsoft.com/v1.0/me/messages',
};
