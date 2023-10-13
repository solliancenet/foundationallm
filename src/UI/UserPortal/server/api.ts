/* eslint-disable prettier/prettier */
import { Message, Session } from '@/js/types';

export default {
  async getSessions() {
    return await $fetch(`${API_URL}/sessions`) as Array<Session>;
  },

  async getMessages(sessionId: string) {
    return await $fetch(`${API_URL}/sessions/${sessionId}/messages`) as Array<Message>;
  },

  async addSession() {
    return await $fetch(`${API_URL}/sessions`, { method: 'POST' }) as Session;
  },

  async rateMessage(message: Message, rating: Message['rating']) {
    message.rating === rating
      ? (message.rating = null)
      : (message.rating = rating);

    const data = (await $fetch(
      `${API_URL}/sessions/${message.sessionId}/message/${message.id}/rate${message.rating !== null ? '?rating=' + message.rating : ''}`, {
        method: 'POST',
      },
    )) as Message;
  },

  async sendMessage(sessionId: string, text: string) {
    return (await $fetch(`${API_URL}/sessions/${sessionId}/completion`, {
      method: 'POST',
      body: JSON.stringify(text),
    })) as string;
  },
};
