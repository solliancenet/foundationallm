using FoundationaLLM.Common.Constants;
using System.Diagnostics;

namespace FoundationaLLM.Common.Logging
{
    /// <summary>
    /// All ActivitySources for the FoundationaLLM services. This is a central place to define all ActivitySources.
    /// </summary>
    public class ActivitySources
    {
        /// <summary>
        /// AgentHubAPI ActivitySource.
        /// </summary>
        public static readonly ActivitySource AgentHubAPIActivitySource = new(ServiceNames.AgentHubAPI);

        /// <summary>
        /// CoreAPI ActivitySource.
        /// </summary>
        public static readonly ActivitySource CoreAPIActivitySource = new(ServiceNames.CoreAPI);

        /// <summary>
        /// GatekeeperAPI ActivitySource.
        /// </summary>
        public static readonly ActivitySource GatekeeperAPIActivitySource = new(ServiceNames.GatekeeperAPI);

        /// <summary>
        /// GatewayAdapterAPI ActivitySource.
        /// </summary>
        public static readonly ActivitySource GatewayAdapterAPIActivitySource = new(ServiceNames.GatewayAdapterAPI);

        /// <summary>
        /// ManagementAPI ActivitySource.
        /// </summary>
        public static readonly ActivitySource ManagementAPIActivitySource = new(ServiceNames.ManagementAPI);

        /// <summary>
        /// OrchestrationAPI ActivitySource.
        /// </summary>
        public static readonly ActivitySource OrchestrationAPIActivitySource = new(ServiceNames.OrchestrationAPI);

        /// <summary>
        /// SemanticKernelAPI ActivitySource.
        /// </summary>
        public static readonly ActivitySource SemanticKernelAPIActivitySource = new(ServiceNames.SemanticKernelAPI);

        /// <summary>
        /// StateAPI ActivitySource.
        /// </summary>
        public static readonly ActivitySource StateAPIActivitySource = new(ServiceNames.StateAPI);

        /// <summary>
        /// VectorizationAPI ActivitySource.
        /// </summary>
        public static readonly ActivitySource VectorizationAPIActivitySource = new(ServiceNames.VectorizationAPI);

        /// <summary>
        /// Start an activity with the given name and source. If addBaggage is true, the baggage from the parent activity will be added to the new activity.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="source"></param>
        /// <param name="kind"></param>
        /// <param name="addBaggage"></param>
        /// <returns></returns>
        public static Activity StartActivity(string name, ActivitySource source, ActivityKind kind = System.Diagnostics.ActivityKind.Consumer, bool addBaggage = true)
        {
            var activity = source.StartActivity(name, kind);

            if (addBaggage && activity != null)
            {
                foreach (var bag in activity?.Parent?.Baggage)
                {
                    activity?.AddTag(bag.Key, bag.Value);
                }
            }

            return activity;
        }
    }
}
