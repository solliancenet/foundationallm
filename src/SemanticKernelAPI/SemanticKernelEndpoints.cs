using FoundationaLLM.SemanticKernelAPI.Interfaces;

namespace SemanticKernelAPI
{
    public class SemanticKernelEndpoints
    {
        private readonly ISemanticKernelService _semanticKernelService;

        public SemanticKernelEndpoints(ISemanticKernelService semanticKernelService)
        {
            _semanticKernelService = semanticKernelService;
        }

        public void Map(WebApplication app)
        {
            app.MapGet("/test/", async () => await _semanticKernelService.Test())
                .WithName("Test");
        }
    }
}