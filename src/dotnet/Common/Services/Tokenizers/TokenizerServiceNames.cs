using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Services.Tokenizers
{
    /// <summary>
    /// Names of tokenizer services supported by the platform.
    /// </summary>
    public static class TokenizerServiceNames
    {
        /// <summary>
        /// Tokenizer service implemented by Microsoft. For details, see https://github.com/microsoft/Tokenizer.
        /// </summary>
        public const string MICROSOFT_BPE_TOKENIZER = "MicrosoftBPETokenizer";
    }
}
