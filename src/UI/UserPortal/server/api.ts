/* eslint-disable prettier/prettier */
import { Message, Session, CompletionPrompt } from '@/js/types';
declare const API_URL: string;

export default {
  async getSessions() {
    return await $fetch(`${API_URL}/sessions`) as Array<Session>;
  },

  async addSession() {
    return await $fetch(`${API_URL}/sessions`, { method: 'POST' }) as Session;
  },

  async renameSession(sessionId: string, newChatSessionName: string) {
    return await $fetch(`${API_URL}/sessions/${sessionId}/rename`, {
      method: 'POST',
      params: {
        newChatSessionName
      }
    }) as Session;
  },

  async summarizeSessionName(sessionId: string, text: string) {
    return await $fetch(`${API_URL}/sessions/${sessionId}/summarize-name`, {
      method: 'POST',
      body: JSON.stringify(text),
    }) as { text: string };
  },

  async deleteSession(sessionId: string) {
    return await $fetch(`${API_URL}/sessions/${sessionId}`, { method: 'DELETE' }) as Session;
  },

  async getMessages(sessionId: string) {
    return await $fetch(`${API_URL}/sessions/${sessionId}/messages`) as Array<Message>;
  },

  async getPrompt(sessionId: string, promptId: string) {
    return await $fetch(`${API_URL}/sessions/${sessionId}/completionprompts/${promptId}`) as CompletionPrompt;
  },

  async rateMessage(message: Message, rating: Message['rating']) {
    const params: {
      rating?: Message['rating']
    } = {};
    if (rating !== null) params.rating = rating;
    return await $fetch(
      `${API_URL}/sessions/${message.sessionId}/message/${message.id}/rate`, {
        method: 'POST',
        params
      }
    ) as Message;
  },

  async sendMessage(sessionId: string, text: string) {
    return (await $fetch(`${API_URL}/sessions/${sessionId}/completion`, {
      method: 'POST',
      body: JSON.stringify(text),
    })) as string;
  },
};
