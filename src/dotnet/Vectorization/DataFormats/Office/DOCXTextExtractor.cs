using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using FoundationaLLM.Vectorization.Exceptions;
using System.Text;

namespace FoundationaLLM.Vectorization.DataFormats.Office
{
    /// <summary>
    /// Extracts text from DOCX files.
    /// </summary>
    public class DOCXTextExtractor
    {
        /// <summary>
        /// Extracts the text content from a DOCX document.
        /// </summary>
        /// <param name="binaryContent">The binary content of the DOCX document.</param>
        /// <returns>The text content of the DOCX document.</returns>
        public static string GetText(BinaryData binaryContent)
        {
            StringBuilder sb = new();

            using var stream = binaryContent.ToStream();
            var wordprocessingDocument = WordprocessingDocument.Open(stream, false);

            var mainPart = wordprocessingDocument.MainDocumentPart ?? throw new VectorizationException("The main document part is missing.");
            var body = mainPart.Document.Body ?? throw new VectorizationException("The document body is missing.");

            var paragraphs = body.Descendants<Paragraph>();
            if (paragraphs != null)
                foreach (Paragraph p in paragraphs)
                    sb.AppendLine(p.InnerText);

            return sb.ToString().Trim();
        }
    }
}
