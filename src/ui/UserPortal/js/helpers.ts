import type {
	ResourceProviderGetResult,
	Agent,
} from '@/js/types';

export const isAgentExpired = (agent: ResourceProviderGetResult<Agent>): boolean  => {
	return agent.resource.expiration_date !== null && new Date() > new Date(agent.resource.expiration_date);
}
