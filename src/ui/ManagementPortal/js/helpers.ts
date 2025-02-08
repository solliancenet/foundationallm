/**
 * Renames a key in an object while preserving the key order.
 * (Normally renaming a key in an object involves deleting the old key and
 * creating a new key with the same value, which will append the new key to the
 * end of the object rather than preserving the position of the original key)
 *
 * @param {Record<string, any>} object - The object whose key is to be renamed.
 * @param {string} oldKeyName - The name of the key to be renamed.
 * @param {string} newKeyName - The new name for the key.
 * @returns {Record<string, any>} A new object with the renamed key in the same position of the original key.
 *
 * @example
 * const object = { oldKey: 'value', anotherKey: 'anotherValue' };
 * const updatedObject = renameObjectKey(object, 'oldKey', 'newKey');
 * console.log(updatedObject); // { newKey: 'value', anotherKey: 'anotherValue' }
 */
export function renameObjectKey(
	object: Record<string, any>,
	oldKeyName: string,
	newKeyName: string,
): Record<string, any> {
	// Convert the object to an array of key value pairs
	const objectKeyValuePairs = Object.entries(object);

	// Find and rename the key
	const updatedKeyValuePairs = objectKeyValuePairs.map(([key, value]) => {
		if (key === oldKeyName) {
			return [newKeyName, value];
		}

		return [key, value];
	});

	// Convert the array back to an object
	return Object.fromEntries(updatedKeyValuePairs);
}
