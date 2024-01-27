using FoundationaLLM.Common.Models.TextEmbedding;
using System.Security.Cryptography;
using System.Text;

namespace FoundationaLLM.Vectorization.Services.VectorizationStates
{
    /// <summary>
    /// Provides base services for the vectorization state services.
    /// </summary>
    public abstract class VectorizationStateServiceBase
    {
        /// <summary>
        /// Gets the location of the vectorization state based on the content identifier.
        /// </summary>
        /// <param name="contentIdentifier">The <see cref="ContentIdentifier"/> holding the content identification information.</param>
        /// <returns></returns>
        protected string GetPersistenceIdentifier(ContentIdentifier contentIdentifier) =>
            $"{contentIdentifier.CanonicalId}_state_{HashContentIdentifier(contentIdentifier)}";

        /// <summary>
        /// Computes the MD5 hash of the content identifier.
        /// </summary>
        /// <param name="contentIdentifier">The <see cref="ContentIdentifier"/> holding the content identification information.</param>
        /// <returns></returns>
        protected static string HashContentIdentifier(ContentIdentifier contentIdentifier)
        {
            var byteHash = MD5.HashData(
                Encoding.UTF8.GetBytes(
                    contentIdentifier.CanonicalId + "|" + contentIdentifier.UniqueId));

            return BitConverter.ToString(byteHash).Replace("-", string.Empty);
        }
    }
}
