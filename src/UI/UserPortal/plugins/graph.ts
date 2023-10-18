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

      try {
        const response = await app.$fetch(endpoint, options);
        return response;
      } catch (error) {
        console.error(error);
      }
    },
  });
});