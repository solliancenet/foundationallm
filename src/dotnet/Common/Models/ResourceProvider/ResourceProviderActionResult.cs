using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Models.ResourceProvider
{
    /// <summary>
    /// The result of an action executed by a resource provider.
    /// </summary>
    /// <param name="IsSuccessResult">Indicates whether the action executed successfully or not.</param>
    public record ResourceProviderActionResult(
        bool IsSuccessResult)
    {
    }
}
