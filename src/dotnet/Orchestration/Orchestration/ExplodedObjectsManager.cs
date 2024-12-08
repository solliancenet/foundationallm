using ZstdSharp.Unsafe;

namespace FoundationaLLM.Orchestration.Core.Orchestration
{
    /// <summary>
    /// Manages the exploded objects dictionary ensuring consistency and integrity.
    /// </summary>
    public class ExplodedObjectsManager
    {
        private readonly Dictionary<string, object> _explodedObjects = [];

        /// <summary>
        /// Adds a new key value pair to the exploded objects dictionary.
        /// </summary>
        /// <param name="key">The key of the object to add to the dictionary.</param>
        /// <param name="value">The object to add to the dictionary.</param>
        /// <returns><see langword="true"/> if the value was added successfully, <see langword="false"/> otherwise.</returns>
        /// <remarks>
        /// The first attempt to add an object always wins.
        /// This means that if the key already exists in the dictionary, the add operation will have no effect and it will not generate an exception either.
        /// </remarks>
        public bool TryAdd(string key, object value)
        {
            if (_explodedObjects.ContainsKey(key))
                return false;

            _explodedObjects.Add(key, value);
            return true;
        }

        /// <summary>
        /// Indicates whether the exploded objects dictionary contains the specified key.
        /// </summary>
        /// <param name="key">The key being searched for.</param>
        /// <returns><see langword="true"/> if the key is present in the dictionary (even if the associated value is null), <see langword="false"/> otherwise.</returns>
        public bool HasKey(string key) =>
            _explodedObjects.ContainsKey(key);

        /// <summary>
        /// Tries to get the value associated with the specified key.
        /// </summary>
        /// <typeparam name="T">The type of the value associated with the key.</typeparam>
        /// <param name="key">The key being searched for.</param>
        /// <param name="value">The value being searched for.</param>
        /// <returns>The typed object associated with the specified key.</returns>
        public bool TryGet<T>(string key, out T? value) where T : class
        {
            value = default(T);

            if (_explodedObjects.TryGetValue(key, out var obj))
            {
                value = obj as T;
                return value != null;
            }

            return false;
        }

        /// <summary>
        /// Gets the exploded objects dictionary.
        /// </summary>
        /// <returns>A shallow copy of the internal exploded objects dictionary. This only prevents unguarded changes to the key-value pairs but not to the values themselves.</returns>
        public Dictionary<string, object> GetExplodedObjects() =>
            new(_explodedObjects);
    }
}
