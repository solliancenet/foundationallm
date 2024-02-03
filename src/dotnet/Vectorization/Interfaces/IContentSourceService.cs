using FoundationaLLM.Common.Models.TextEmbedding;
using System;
using System.Threading.Tasks;

namespace FoundationaLLM.Vectorization.Interfaces
{
    /// <summary>
    /// Provides access to files from a content source.
    /// </summary>
    public interface IContentSourceService
    {
        /// <summary>
        /// Reads the binary content of a specified file from the storage.
        /// </summary>
        /// <param name="contentId">The <see cref="ContentIdentifier"/> providing the unique identifier of the file being read.</param>
        /// <param name="cancellationToken">The cancellation token that signals that operations should be cancelled.</param>
        /// <returns>The string content of the file.</returns>
        Task<string> ExtractTextFromFileAsync(ContentIdentifier contentId, CancellationToken cancellationToken);
    }
}
