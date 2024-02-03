namespace FoundationaLLM.Vectorization.Models.Resources
{
    /// <summary>
    /// Models the content of a profile store managed by the FoundationaLLM.Vectorization resource provider.
    /// </summary>
    public class ProfileStore<T> where T : VectorizationProfileBase
    {
        /// <summary>
        /// The list of all profiles that are registered in the profile store.
        /// </summary>
        public required List<T> Profiles { get; set; }

        /// <summary>
        /// Creates a new profile store from a dictionary.
        /// </summary>
        /// <param name="dictionary">The dictionary containing the profiles.</param>
        /// <returns>The newly created profile store.</returns>
        public static ProfileStore<T> FromDictionary(Dictionary<string, T> dictionary) =>
            new ProfileStore<T>
            {
                Profiles = [.. dictionary.Values]
            };

        /// <summary>
        /// Creates a dictionary of profiles from the profile store.
        /// </summary>
        /// <returns>The newly created dictionary.</returns>
        public Dictionary<string, T> ToDictionary() =>
            Profiles.ToDictionary<T, string>(p => p.Name);
    }
}
