using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using FoundationaLLM.Vectorization.Exceptions;

namespace FoundationaLLM.Vectorization.DataFormats.Office
{
    /// <summary>
    /// Extracts text from PPTX files.
    /// </summary>
    public class PPTXTextExtractor
    {
        /// <summary>
        /// Extracts the text content from a PPTX document.
        /// </summary>
        /// <param name="binaryContent">The binary content of the PPTX document.</param>
        /// <returns>The text content of the PPTX document.</returns>
        public static string GetText(BinaryData binaryContent) => throw new VectorizationException($"The file type .pptx is not supported.");
    }
}
