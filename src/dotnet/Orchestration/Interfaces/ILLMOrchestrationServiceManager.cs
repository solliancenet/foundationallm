﻿using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Infrastructure;

namespace FoundationaLLM.Orchestration.Core.Interfaces
{
    /// <summary>
    /// Defines the interface for the LLM Orchestration Service Manager.
    /// </summary>
    public interface ILLMOrchestrationServiceManager
    {
        /// <summary>
        /// Gets an aggregate initialization status based on the initialization status of each subordinate orchestration service.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance ID.</param>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> provding dependency injection services for the current scope.</param>
        Task<List<ServiceStatusInfo>> GetAggregateStatus(string instanceId, IServiceProvider serviceProvider);

        /// <summary>
        /// Gets an <see cref="ILLMOrchestrationService"/> instance based on the service name.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance ID.</param>
        /// <param name="serviceName">The name of the <see cref="ILLMOrchestrationService"/> to be retrieved.</param>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> provding dependency injection services for the current scope.</param>
        /// <param name="callContext">The <see cref="IOrchestrationContext"/> call context of the request being handled.</param>
        /// <returns></returns>
        ILLMOrchestrationService GetService(string instanceId, string serviceName, IServiceProvider serviceProvider, IOrchestrationContext callContext);
    }
}
