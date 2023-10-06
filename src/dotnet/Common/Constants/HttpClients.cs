namespace FoundationaLLM.Common.Constants;

/// <summary>
/// Name constants used to configure and retrieve an <see cref="T:System.Net.Http.HttpClient" />,
/// using <see cref="T:System.Net.Http.IHttpClientFactory" />.
/// </summary>
public static class HttpClients
{
    public const string DefaultHttpClient = "DefaultHttpClient";
    public const string LangChainAPIClient = "LangChainAPIClient";
    public const string SemanticKernelAPIClient = "SemanticKernelAPIClient";
    public const string GatekeeperAPIClient = "GatekeeperAPIClient";
    public const string AgentFactoryAPIClient = "AgentFactoryAPIClient";
    public const string AgentHubAPIClient = "AgentHubAPIClient";
}
