import TimeAgo from 'javascript-time-ago';
import en from 'javascript-time-ago/locale/en';
TimeAgo.addDefaultLocale(en);
const timeAgo = new TimeAgo('en-US');

const filters = {
	timeAgo(date: Date): string {
		return timeAgo.format(date);
	},

	enforceLeadingSlash(path: string) {
		if (!path.startsWith('/')) {
			return '/' + path;
		} else {
			return path;
		}
	}
};

export default defineNuxtPlugin((nuxtApp) => {
	nuxtApp.vueApp.config.globalProperties.$filters = filters;
});
