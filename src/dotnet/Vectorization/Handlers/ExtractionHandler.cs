﻿using FoundationaLLM.Common.Constants;
using FoundationaLLM.Vectorization.Models;

namespace FoundationaLLM.Vectorization.Handlers
{
    public class ExtractionHandler : VectorizationStepHandlerBase
    {
        public ExtractionHandler(
            Dictionary<string, string> parameters) : base(VectorizationSteps.Extract, parameters)
        {
        }

        protected override async Task ProcessRequest(VectorizationRequest request, VectorizationState state, CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(10));
        }
    }
}
