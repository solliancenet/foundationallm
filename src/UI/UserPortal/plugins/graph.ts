import { defineNuxtPlugin } from '#app';

export default defineNuxtPlugin(({ app }, inject) => {
  inject('graph', {
    callMSGraph: async (endpoint, token) => {
      const headers = {
        Authorization: `Bearer ${token}`,
      };

      const options = {
        method: 'GET',
        headers,
      };

      console.log('Request made to Graph API at: ' + new Date().toString());

      try {
        const response = await app.$fetch(endpoint, options);
        return response;
      } catch (error) {
        console.error(error);
      }
    },
  });
});