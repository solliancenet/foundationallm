namespace FoundationaLLM.SemanticKernel.Text
{
    /// <summary>
    /// String extensions class.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Normalizes line endings for an input text.
        /// </summary>
        /// <param name="src">The input text.</param>
        /// <returns>The input text with normalized line endlings.</returns>
        public static string NormalizeLineEndings(this string src)
        {
            return src.ReplaceLineEndings("\n");
        }
    }
}
