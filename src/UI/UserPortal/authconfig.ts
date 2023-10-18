import { LogLevel, PublicClientApplication } from '@azure/msal-browser';

export const msalConfig = {
    auth: {
        // 'Application (client) ID' of app registration in Azure portal - this value is a GUID
        clientId: "0b08d115-b517-4d7f-b883-a1665191d14d",
        // Full directory URL, in the form of https://login.microsoftonline.com/<tenant-id>
        authority: "https://login.microsoftonline.com/common",
        // Full redirect URL, in form of http://localhost:3000
        redirectUri: "/",
    },
    cache: {
        cacheLocation: "sessionStorage", // This configures where your cache will be stored
        storeAuthStateInCookie: false, // Set this to "true" if you are having issues on IE11 or Edge
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
        }	
    }
};

export const msalInstance = new PublicClientApplication(msalConfig);

export const loginRequest = {
    scopes: ['User.Read'],
};

export const tokenRequest = {
    scopes: ["User.Read"],
    forceRefresh: true // Set this to "true" to skip a cached token and go to the server to get a new token
};

export const graphConfig = {
    graphMeEndpoint: 'https://graph.microsoft.com/v1.0/me',
    graphMailEndpoint: 'https://graph.microsoft.com/v1.0/me/messages',
};
  