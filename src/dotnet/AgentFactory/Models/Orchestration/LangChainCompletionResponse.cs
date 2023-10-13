using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.AgentFactory.Core.Models.Orchestration
{
    internal class LangChainCompletionResponse
    {
        public string user_prompt;
        public int prompt_tokens;
        public int completion_tokens;       
        public string completion;
    }
}
