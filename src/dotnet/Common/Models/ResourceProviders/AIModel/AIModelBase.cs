using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using Microsoft.Graph.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Models.ResourceProviders.AIModel
{
    public class AIModelBase : ResourceBase
    {
        /// <summary>
        /// The endpoint metadata needed to call the AI model endpoint
        /// </summary>
        public AIEndpoint? Endpoint { get; set; }
        /// <summary>
        /// The version for the AI model
        /// </summary>
        public string? Version { get; set; }

        /// <summary>
        /// Deployment name for the AI model
        /// </summary>
        public string? DeploymentName { get; set; }
        /// <summary>
        /// Key value parameters configured for the model
        /// </summary>
        public Dictionary<string, object> ModelParameters { get; set; } = new Dictionary<string, object>();

    }
}
