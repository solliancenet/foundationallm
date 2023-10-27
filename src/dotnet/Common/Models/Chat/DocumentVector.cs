namespace FoundationaLLM.Common.Models.Chat
{
    /// <summary>
    /// The document vector object.
    /// </summary>
    public record DocumentVector
    {
        /// <summary>
        /// The unique identifier.
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// The identifier of the item associated with the document vector.
        /// </summary>
        public string itemId { get; set; }
        /// <summary>
        /// The partition key associated with the document vector.
        /// </summary>
        public string partitionKey { get; set; }
        /// <summary>
        /// The name of the container associated with the document vector.
        /// </summary>
        public string containerName { get; set; }
        /// <summary>
        /// The vector associated with the document.
        /// </summary>
        public float[]? vector { get; set; }

        /// <summary>
        /// Constructor for Document Vector.
        /// </summary>
        public DocumentVector(string itemId, string partitionKey, string containerName, float[]? vector = null)
        {
            id = Guid.NewGuid().ToString();
            this.itemId = itemId;
            this.partitionKey = partitionKey;
            this.containerName = containerName;
            this.vector = vector;
        }
    }
}
