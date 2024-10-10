namespace FoundationaLLM.Common.Models.Conversation
{
    /// <summary>
    /// Gatekeeper override options expressed by an agent.
    /// </summary>
    public enum AgentGatekeeperOverrideOption
    {
        /// <summary>
        /// The agent deferrs to the system option.
        /// </summary>
        UseSystemOption,

        /// <summary>
        /// The agent requires an explicit bypass of the Gatekeeper, regardless of the system option.
        /// </summary>
        MustBypass,

        /// <summary>
        /// The agent requires an explicit call to the Gatekeeper, regardless of the system option.
        /// </summary>
        MustCall
    }
}
