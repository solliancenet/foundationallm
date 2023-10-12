namespace FoundationaLLM.Common.Constants;

/// <summary>
/// Name constants used to configure and retrieve an <see cref="T:System.Net.Http.HttpClient" />,
/// using <see cref="T:System.Net.Http.IHttpClientFactory" />.
/// </summary>
public static class HttpClients
{
    public const string DefaultHttpClient = "DefaultHttpClient";
    public const string LangChainAPIClient = "LangChainOrchestration";
    public const string SemanticKernelAPIClient = "SemanticKernelOrchestration";
    public const string GatekeeperAPIClient = "GatekeeperAPI";
    public const string AgentFactoryAPIClient = "AgentFactoryAPI";
    public const string AgentHubAPIClient = "AgentHub";
}
