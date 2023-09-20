using FoundationaLLM.GatekeeperAPI.Interfaces;

namespace FoundationaLLM.GatekeeperAPI
{
    public class GatekeeperEndpoints
    {
        private readonly IGatekeeperService _gatekeeperService;

        public GatekeeperEndpoints(IGatekeeperService gatekeeperService)
        {
            _gatekeeperService = gatekeeperService;
        }

        public void Map(WebApplication app)
        {
            app.MapGet("/test/", async () => await _gatekeeperService.Test())
                .WithName("Test");
        }
    }
}