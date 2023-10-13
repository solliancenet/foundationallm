namespace FoundationaLLM.Gatekeeper.Core.Models.ContentSafety
{
    public class AnalyzeTextFilterResult
    {
        public bool Safe { get; set; }
        public string Reason { get; set; }
    }
}
