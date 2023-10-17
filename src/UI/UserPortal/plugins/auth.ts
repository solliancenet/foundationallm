// plugins/auth.js
import { PublicClientApplication, LogLevel } from '@azure/msal-browser';
import { defineNuxtPlugin } from '#app';

const msalConfig = {
  auth: {
    clientId: '0b08d115-b517-4d7f-b883-a1665191d14d',
    authority: 'https://login.microsoftonline.com/common',
    redirectUri: '/',
  },
  cache: {
    cacheLocation: 'sessionStorage',
    storeAuthStateInCookie: false,
  },
  system: {
    loggerOptions: {
      loggerCallback: (level, message, containsPii) => {
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
    },
  },
};

let msalInstance; // Define the MSAL instance

// Asynchronous function to initialize MSAL
async function initializeMsal() {
  msalInstance = new PublicClientApplication(msalConfig); // Create the MSAL instance
  await msalInstance.handleRedirectPromise(); // Handle any redirect responses if necessary
}

export default defineNuxtPlugin(async (nuxtApp) => {
  // Initialize the MSAL instance
  await initializeMsal();

  // Inject the MSAL instance into global properties
  nuxtApp.vueApp.config.globalProperties.$msalInstance = msalInstance;
});
