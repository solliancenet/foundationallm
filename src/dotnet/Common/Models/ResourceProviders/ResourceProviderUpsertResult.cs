﻿namespace FoundationaLLM.Common.Models.ResourceProviders
{
    /// <summary>
    /// Represents the result of an upsert operation.
    /// </summary>
    public class ResourceProviderUpsertResult
    {
        /// <summary>
        /// The id of the object that was created or updated.
        /// </summary>
        public string? ObjectId { get; set; }

        /// <summary>
        /// A flag denoting whether the upserted resource already exists.
        /// </summary>
        public bool? ResourceExists { get; set; }
    }
}
