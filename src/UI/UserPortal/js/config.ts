import { AppConfigurationClient } from '@azure/app-configuration';
declare const APP_CONFIG_ENDPOINT: string;

let appConfigClient = new AppConfigurationClient(APP_CONFIG_ENDPOINT);

function getConfigClient() {
	if (!APP_CONFIG_ENDPOINT) {
		// throw new Error('APP_CONFIG_ENDPOINT environment variable is not defined');
	}

	if (!appConfigClient) {
		appConfigClient = new AppConfigurationClient(APP_CONFIG_ENDPOINT);
	}

	return appConfigClient;
}

export default async function getAppConfigSetting(key: string) {
	const setting = await getConfigClient().getConfigurationSetting({ key });
	return setting.value ? setting.value : null;
}
