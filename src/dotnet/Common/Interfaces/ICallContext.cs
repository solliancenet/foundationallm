using FoundationaLLM.Common.Models.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoundationaLLM.Common.Models.Metadata;

namespace FoundationaLLM.Common.Interfaces
{
    /// <summary>
    /// Stores context information for the current HTTP request.
    ///
    /// The AgentHint property is extracted from the request header, if any.
    /// This is used in scenarios where the client wants to hint the API about
    /// a specific agent to resolve, and is passed through the API layers in
    /// the form of a header.
    ///
    /// The CurrentUserIdentity stores a <see cref="UnifiedUserIdentity"/> object
    /// resolved from one or more services.
    /// </summary>
    public interface ICallContext
    {
        /// <summary>
        /// The current agent hint. If empty, there is no associated header value.
        /// </summary>
        Agent? AgentHint { get; set; }

        /// <summary>
        /// The current <see cref="UnifiedUserIdentity"/> object resolved
        /// from one or more services.
        /// </summary>
        UnifiedUserIdentity? CurrentUserIdentity { get; set; }
        /// <summary>
        /// The unique identifier of the current FoundationaLLM deployment instance.
        /// </summary>
        string? InstanceId { get; set; }
    }
}
