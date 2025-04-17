namespace FoundationaLLM.Common.Interfaces
{
    /// <summary>
    /// Defines the properties and methods for a runnable FoundationaLLM resource.
    /// </summary>
    public interface IRunnableResource
    {
        /// <summary>
        /// Gets or sets the flag that indicates whether the resource run is completed.
        /// </summary>
        public bool Completed { get; set; }

        /// <summary>
        /// Gets or sets the flag that indicates whether the resource run was successful.
        /// </summary>
        public bool Successful { get; set; }
    }
}
