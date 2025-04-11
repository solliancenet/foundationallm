using System;

namespace FoundationaLLM.Common.Models.ResourceProviders
{
    /// <summary>
    /// Provides fields used for file mapping capabilities.
    /// </summary>
    public interface IFileMapping
    {
        /// <summary>
        /// The FoundationaLLM.Attachment resource object id.
        /// </summary>       
        public string FileObjectId { get; set; }

        /// <summary>
        /// The original file name of the file.
        /// </summary>        
        public string OriginalFileName { get; set; }

        /// <summary>
        /// The content type of the file.
        /// </summary>        
        public string FileContentType { get; set; }

        /// <summary>
        /// Indicates whether the file requires vectorization or not.
        /// </summary>        
        public bool FileRequiresVectorization { get; set; }
    }
}
