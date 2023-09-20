using FoundationaLLM.GatekeeperAPI.Interfaces;
using FoundationaLLM.GatekeeperAPI.Models.ConfigurationOptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM.GatekeeperAPI.Services
{
    public class GatekeeperService : IGatekeeperService
    {
        private readonly GatekeeperServiceSettings _settings;
        private readonly ILogger _logger;

        public GatekeeperService(
            IOptions<GatekeeperServiceSettings> options,
            ILogger<GatekeeperService> logger)
        {
            _settings = options.Value;
            _logger = logger;
        }

        public async Task Test()
        {
            await Task.Run(() => 
            {
                throw new NotImplementedException(); 
            });
        }
    }
}
