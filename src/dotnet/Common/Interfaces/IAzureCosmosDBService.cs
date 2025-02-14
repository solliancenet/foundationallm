using FoundationaLLM.Common.Models.Azure.CosmosDB;
using FoundationaLLM.Common.Models.Configuration.Users;
using FoundationaLLM.Common.Models.Conversation;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.Orchestration.Response;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentFiles;
using FoundationaLLM.Common.Models.ResourceProviders.Attachment;

namespace FoundationaLLM.Common.Interfaces;

/// <summary>
/// Contains methods for accessing Azure Cosmos DB for NoSQL.
/// </summary>
public interface IAzureCosmosDBService
{
    /// <summary>
    /// Gets a single item by its identifier and partition key.
    /// </summary>
    /// <typeparam name="T">The type of the item to retrieve.</typeparam>
    /// <param name="containerName">The name of the container storing the item.</param>
    /// <param name="id">The identifier of the item.</param>
    /// <param name="partitionKey">The partition key of the item.</param>
    /// <param name="cancellationToken">Cancellation token for async calls.</param>
    /// <returns>The retrieved object of type <typeparamref name="T"/>.</returns>
    Task<T?> GetItemAsync<T>(string containerName, string id, string partitionKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates or updates an item in the specified container.
    /// </summary>
    /// <typeparam name="T">The type of the item to create or update.</typeparam>
    /// <param name="containerName">The name of the container storing the item.</param>
    /// <param name="item">The item to be created or updated.</param>
    /// <param name="partitionKey">The partition key of the item.</param>
    /// <param name="cancellationToken">Cancellation token for async calls.</param>
    /// <returns>The created or updated object of type <typeparamref name="T"/>.</returns>
    Task<T?> UpsertItemAsync<T>(string containerName, string partitionKey, T item, CancellationToken cancellationToken = default);

    /// <summary>
    /// Patches an item in the specified container.
    /// </summary>
    /// <typeparam name="T">The type of the item to patch.</typeparam>
    /// <param name="containerName">The name of the container storing the item.</param>
    /// <param name="partitionKey">The partition key of the item.</param>
    /// <param name="id">The item ID.</param>
    /// <param name="upn">The user principal name used for matching the UPN of the item.</param>
    /// <param name="propertyValues">Dictionary of the property names and values to patch.</param>
    /// <param name="cancellationToken">Cancellation token for async calls.</param>
    /// <returns>The patched object of type <typeparam name="T"></typeparam>.</returns>
    Task<T> PatchItemPropertiesAsync<T>(string containerName, string partitionKey, string id,
        string upn, Dictionary<string, object?> propertyValues, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a list of all current conversations.
    /// </summary>
    /// <param name="type">The conversation type to return.</param>
    /// <param name="upn">The user principal name used for retrieving
    /// conversations for the signed-in user.</param>
    /// <param name="cancellationToken">Cancellation token for async calls.</param>
    /// <returns>List of distinct conversation items.</returns>
    Task<List<Conversation>> GetConversationsAsync(string type, string upn, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a point read to retrieve a single conversation item.
    /// </summary>
    /// <returns>The conversation item. Returns null if the conversation does not exist.</returns>
    Task<Conversation?> GetConversationAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates or updates a conversation.
    /// </summary>
    /// <param name="session">Conversation item to create or update.</param>
    /// <param name="cancellationToken">Cancellation token for async calls.</param>
    /// <returns>Newly created or updated conversation item.</returns>
    Task<Conversation> CreateOrUpdateConversationAsync(Conversation session, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates conversation properties through a patch operation.
    /// </summary>
    /// <param name="id">The conversation id.</param>
    /// <param name="upn">The user principal name used for policy enforcement.</param>
    /// <param name="propertyValues">The dictionary containing property names and updated values.</param>
    /// <param name="cancellationToken">Cancellation token for async calls.</param>
    /// <returns>Updated conversation item.</returns>
    Task<Conversation> PatchConversationPropertiesAsync(string id, string upn, Dictionary<string, object?> propertyValues, CancellationToken cancellationToken = default);

    /// <summary>
    /// Batch deletes an existing chat session and all related messages.
    /// </summary>
    /// <param name="sessionId">Chat session identifier used to flag messages and sessions for deletion.</param>
    /// <param name="cancellationToken">Cancellation token for async calls.</param>
    Task DeleteConversationAsync(string sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a list of all current chat messages for a specified session identifier.
    /// Messages are always sorted by TimeStamp in ascending order.
    /// </summary>
    /// <param name="sessionId">Chat session identifier used to filter messages.</param>
    /// <param name="upn">The user principal name used for retrieving the messages for
    /// the signed in user.</param>
    /// <param name="max">If provided, limits the number of messages.</param>
    /// <param name="cancellationToken">Cancellation token for async calls.</param>
    /// <returns>List of chat message items for the specified session.</returns>
    Task<List<Message>> GetSessionMessagesAsync(string sessionId, string upn, int? max = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a single conversation message by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the message.</param>
    /// <param name="sessionId">The identifier of the conversation.</param>
    /// <param name="cancellationToken">Cancellation token for async calls.</param>
    /// <returns>A <see cref="Message"/> object containing the message.</returns>
    Task<Message> GetMessageAsync(string id, string sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new chat message.
    /// </summary>
    /// <param name="message">Chat message item to create.</param>
    /// <param name="cancellationToken">Cancellation token for async calls.</param>
    /// <returns>Newly created chat message item.</returns>
    Task<Message> InsertMessageAsync(Message message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing chat message.
    /// </summary>
    /// <param name="message">Chat message item to update.</param>
    /// <param name="cancellationToken">Cancellation token for async calls.</param>
    /// <returns>Revised chat message item.</returns>
    Task<Message> UpdateMessageAsync(Message message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Batch create or update chat messages and session.
    /// </summary>
    /// <param name="messages">Chat message and session items to create or replace.</param>
    Task UpsertSessionBatchAsync(params dynamic[] messages);

    /// <summary>
    /// Updates a subset of the properties of an item of a specified type from the Sessions collection.
    /// </summary>
    /// <typeparam name="T">The type of the item to update.</typeparam>
    /// <param name="itemId">The identifier of the item being updated.</param>
    /// <param name="partitionKey">The partition key of the item being updated.</param>
    /// <param name="propertyValues">The dictionary containing property names and updated values.</param>
    /// <param name="cancellationToken">Cancellation token for async calls.</param>
    /// <returns></returns>
    Task<T> PatchSessionsItemPropertiesAsync<T>(string itemId, string partitionKey, Dictionary<string, object?> propertyValues, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a subset of the properties of one or more items of a specified type from the Sessions collection.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="partitionKey">The partition key of the item(s) being updated. Batch operations
    /// must occur within the same partition key.</param>
    /// <param name="patchOperations">The patch operations to perform on each object, including the
    /// object's identifier, dictionary containing the property names and updated values, and the item type.</param>
    /// <param name="cancellationToken">Cancellation token for async calls.</param>
    /// <returns></returns>
    Task<Dictionary<string, object>> PatchMultipleSessionsItemsInTransactionAsync(
        string partitionKey,
        List<IPatchOperationItem> patchOperations,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create or update a user session from the passed in Session object.
    /// </summary>
    /// <param name="session">The chat session item to create or replace.</param>
    /// <param name="cancellationToken">Cancellation token for async calls.</param>
    /// <returns></returns>
    Task UpsertUserSessionAsync(Conversation session, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the completion prompt for a given session and completion prompt id.
    /// </summary>
    /// <param name="sessionId">The session id from which to retrieve the completion prompt.</param>
    /// <param name="completionPromptId">The id of the completion prompt to retrieve.</param>
    /// <returns></returns>
    Task<CompletionPrompt> GetCompletionPromptAsync(string sessionId, string completionPromptId);

    /// <summary>
    /// Returns the user profile for a given user via their UPN.
    /// </summary>
    /// <param name="upn">The user principal name used for retrieving the messages for
    /// the signed in user.</param>
    /// <param name="cancellationToken">Cancellation token for async calls.</param>
    /// <returns></returns>
    Task<UserProfile> GetUserProfileAsync(string upn, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts or updates a user profile.
    /// </summary>
    /// <param name="userProfile">The user profile to upsert.</param>
    /// <param name="cancellationToken">Cancellation token for async calls.</param>
    /// <returns></returns>
    Task UpsertUserProfileAsync(UserProfile userProfile, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the context for a long running operation.
    /// </summary>
    /// <param name="operationId">The identifier of the long running operation.</param>
    /// <param name="cancellationToken">Cancellation token for async calls.</param>
    /// <returns>A <see cref="LongRunningOperationContext"/> object providing the context for the long running operation.</returns>
    Task<LongRunningOperationContext> GetLongRunningOperationContextAsync(string operationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts or updates a long running operation context.
    /// </summary>
    /// <param name="longRunningOperationContext">The <see cref="LongRunningOperationContext"/> object providing the context for the long running operation.</param>
    /// <param name="cancellationToken">Cancellation token for async calls.</param>
    /// <returns></returns>
    Task UpsertLongRunningOperationContextAsync(LongRunningOperationContext longRunningOperationContext, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a subset of the properties of an item of a specified type from the Operations collection.
    /// </summary>
    /// <typeparam name="T">The type of the item to update.</typeparam>
    /// <param name="itemId">The identifier of the item being updated.</param>
    /// <param name="partitionKey">The partition key of the item being updated.</param>
    /// <param name="propertyValues">The dictionary containing property names and updated values.</param>
    /// <param name="cancellationToken">Cancellation token for async calls.</param>
    /// <returns></returns>
    Task<T> PatchOperationsItemPropertiesAsync<T>(string itemId, string partitionKey, Dictionary<string, object?> propertyValues, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an attachment.
    /// </summary>
    /// <param name="upn">The user's UPN.</param>
    /// <param name="id">The attachment id.</param>
    /// <param name="cancellationToken">Cancellation token for async calls.</param>
    /// <returns>An attachment.</returns>
    Task<AttachmentReference?> GetAttachment(string upn, string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets list of filtered attachments.
    /// </summary>
    /// <param name="upn">The user's UPN.</param>
    /// <param name="resourceFilter">The resource filter.</param>
    /// <param name="cancellationToken">Cancellation token for async calls.</param>
    /// <returns>A list of filtered attachments.</returns>
    Task<List<AttachmentReference>> FilterAttachments(string upn, ResourceFilter resourceFilter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a list of attachments for the signed in user.
    /// </summary>
    /// <param name="upn">The user principal name used for retrieving the attachments for
    /// the signed in user.</param>
    /// <param name="cancellationToken">Cancellation token for async calls.</param>
    /// <returns>A list of attachments for the signed in user.</returns>
    Task<List<AttachmentReference>> GetAttachments(string upn, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates an attachment.
    /// </summary>
    /// <param name="attachment">The attachment to be added.</param>
    /// <param name="cancellationToken">Cancellation token for async calls.</param>
    /// <returns></returns>
    Task CreateAttachment(AttachmentReference attachment, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an attachment.
    /// </summary>
    /// <param name="attachment">The attachment to be deleted.</param>
    /// <param name="cancellationToken">Cancellation token for async calls.</param>
    /// <returns></returns>
    Task DeleteAttachment(AttachmentReference attachment, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an agent file.
    /// </summary>
    /// <param name="instanceId">The instance unique identifier</param>
    /// <param name="agentName">The agent name.</param>
    /// <param name="id">The agent file id.</param>
    /// <param name="cancellationToken">Cancellation token for async calls.</param>
    /// <returns>An agent file.</returns>
    Task<AgentFileReference?> GetAgentFile(string instanceId, string agentName, string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a list of agent files.
    /// </summary>
    /// <param name="instanceId">The instance unique identifier</param>
    /// <param name="agentName">The agent name.</param>
    /// <param name="cancellationToken">Cancellation token for async calls.</param>
    /// <returns>A list of agent files.</returns>
    Task<List<AgentFileReference>> GetAgentFiles(string instanceId, string agentName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates an agent file.
    /// </summary>
    /// <param name="agentFile">The agent file to be added.</param>
    /// <param name="cancellationToken">Cancellation token for async calls.</param>
    /// <returns></returns>
    Task CreateAgentFile(AgentFileReference agentFile, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an agent file.
    /// </summary>
    /// <param name="agentFile">The agent file to be deleted.</param>
    /// <param name="cancellationToken">Cancellation token for async calls.</param>
    /// <returns></returns>
    Task DeleteAgentFile(AgentFileReference agentFile, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new container for vector search.
    /// </summary>
    /// <param name="containerName">The name of the container to create.</param>
    /// <param name="partitionKeyPath">The property path that contains the partition key.</param>
    /// <param name="vectorProperyPath">The property path that contains the vectors.</param>
    /// <param name="vectorDimensions">The length of each vector (the number of dimensions used for embedding).</param>
    /// <param name="cancellationToken">The cancellation token to signal the need to cancel the operation.</param>
    /// <returns></returns>
    Task CreateVectorSearchContainerAsync(
        string containerName,
        string partitionKeyPath,
        string vectorProperyPath,
        int vectorDimensions,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the completion response for a given user prompt embedding using vector search and a minimum threshold for similarity.
    /// </summary>
    /// <param name="containerName">The name of the container holding the vector index.</param>
    /// <param name="userPromptEmbedding">The reference embedding used for the vector search.</param>
    /// <param name="minimumSimilarityScore">The threshold used for the similarity score.</param>
    /// <returns>A <see cref="CompletionResponse"/> that matches the search criteria. If no item in the vector index matches the criteria, returns <see langref="null"/>.</returns>
    Task<CompletionResponse?> GetCompletionResponseAsync(
        string containerName,
        ReadOnlyMemory<float> userPromptEmbedding,
        decimal minimumSimilarityScore);
}
