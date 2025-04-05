using System.Collections.Immutable;

namespace FoundationaLLM.Common.Constants.Context
{
    /// <summary>
    /// Provides constants for the code session programming languages.
    /// </summary>
    public static class CodeSessionLanguages
    {
        /// <summary>
        /// The Python programming language.
        /// </summary>
        public const string Python = "Python";

        /// <summary>
        /// The C# programming language.
        /// </summary>
        public const string CSharp = "CSharp";

        /// <summary>
        /// All code session programming languages.
        /// </summary>
        public static readonly ImmutableArray<string> All = [
            Python,
            CSharp
        ];
    }
}
