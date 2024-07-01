import { describe, test, expect } from 'vitest';
import api from './api';
require('dotenv').config();

describe('API Account Tests', async () => {
	api.setApiUrl(process.env.VITEST_API_URL);
	api.setInstanceId(process.env.VITEST_API_INSTANCE_ID);
	api.bearerToken = process.env.VITEST_API_AUTH_TOKEN;

	test.concurrent('should fetch role assignments', async () => {
		const roleAssignments = await api.getRoleAssignments();
		expect(roleAssignments).toBeInstanceOf(Array);

		const roleAssignment = roleAssignments[0];
		expect(roleAssignment).toBeDefined();
	});

	test.concurrent('should fetch role assignment details', async () => {
		const roleAssignments = await api.getRoleAssignments();
		expect(roleAssignments).toBeInstanceOf(Array);
		const roleAssignment = roleAssignments[0];
		expect(roleAssignment).toBeDefined();

		const roleAssignmentDetails = await api.getRoleAssignment(roleAssignment.id);
		expect(roleAssignmentDetails).toBeDefined();
		expect(roleAssignmentDetails).toBeInstanceOf(Object);
		expect(roleAssignmentDetails).not.toBeInstanceOf(Array);
	});

	test.concurrent('should fetch users', async () => {
		const users = await api.getUsers();
		const user = users.items[0];
		expect(user).toBeDefined();
	});

	test.concurrent('should fetch user details', async () => {
		const users = await api.getUsers();
		const user = users.items[0];
		expect(user).toBeDefined();
		
		const userDetails = await api.getUser(user.id);
		expect(userDetails).toBeDefined();
	});

	test.concurrent('should fetch groups', async () => {
		const groups = await api.getGroups();
		const group = groups.items[0];
		expect(group).toBeDefined();
	});

	test.concurrent('should fetch group details', async () => {
		const groups = await api.getGroups();
		const group = groups.items[0];
		expect(group).toBeDefined();
		
		const groupDetails = await api.getGroup(group.id);
		expect(groupDetails).toBeDefined();
	});

	test.concurrent('should fetch objects for user and group', async () => {
		const users = await api.getUsers();
		const user = users.items[0];
		expect(user).toBeDefined();

		const groups = await api.getGroups();
		const group = groups.items[0];
		expect(group).toBeDefined();
		
		const objects = await api.getObjects({
			ids: [user.id, group.id],
		});
		expect(objects).toBeDefined();
		expect(objects).toBeInstanceOf(Array);
	});
});
