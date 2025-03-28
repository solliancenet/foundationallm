﻿using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Orchestration.Core.Models.ConfigurationOptions;
using FoundationaLLM.Orchestration.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace FoundationaLLM.Orchestration.Tests.Services
{
    public class LangChainServiceTests
    {
        private readonly IOptions<LangChainServiceSettings> _options = Substitute.For<IOptions<LangChainServiceSettings>>();
        private readonly ILogger<LangChainService> _logger = Substitute.For<ILogger<LangChainService>>();
        private readonly IOrchestrationContext _callContext = Substitute.For<IOrchestrationContext>();
        private readonly IHttpClientFactoryService _httpClientFactoryService = Substitute.For<IHttpClientFactoryService>();
        private readonly LangChainService _langChainService;

        public LangChainServiceTests()
        {
            _langChainService = new LangChainService(_options, _logger, _callContext, _httpClientFactoryService);
        }
    }
}
