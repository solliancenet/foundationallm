# Foundationa**LLM** Core Client

The Foundationa**LLM** Core Client is a .NET client library that simplifies the process of interacting with the Foundationa**LLM** Core API. The client library provides a set of classes and methods that allow you to interact with the Foundationa**LLM** Core API in a more intuitive way.

This library contains two primary classes:

- `CoreRESTClient`: A class that provides a set of methods for interacting with the Foundationa**LLM** Core API using REST. This is considered the low-level client and provides direct access to all Core API endpoints.
- `CoreClient`: A class that provides a set of methods for interacting with the Foundationa**LLM** Core API using a higher-level abstraction. This class is designed to simplify the process of interacting with the Core API by providing a more intuitive interface. It does not contain all the methods available in the `CoreRESTClient` class, but it provides a more user-friendly way to interact with the Core API.

> [!NOTE]
> These two classes are mutually exclusive, and you should choose one based on your requirements. If you need direct access to all Core API endpoints, use the `CoreRESTClient` class. If you need a more user-friendly interface, use the `CoreClient` class.

## Getting started

> [!TIP]
> If you do not have FoundationaLLM deployed, follow the [Quick Start Deployment instructions](https://docs.foundationallm.ai/deployment/deployment-quick-start.html) to get FoundationaLLM deployed in your Azure subscription.

Install the NuGet package:

```bash
dotnet add package FoundationaLLM.Client.Core
```

### Manual service instantiation

Complete the following steps if you do not want to use dependency injection:

1. Create a new instance of the `CoreRESTClient` and `CoreClient` classes:

    ```csharp
    var coreUri = "<YOUR_CORE_API_URL>"; // e.g., "https://myfoundationallmcoreapi.com"
    var credential = new AzureCliCredential(); // Can use any TokenCredential implementation, such as ManagedIdentityCredential or AzureCliCredential.
    var options = new APIClientSettings // Optional settings parameter. Default timeout is 900 seconds.
    {
        Timeout = TimeSpan.FromSeconds(600)
    };

    var coreRestClient = new CoreRESTClient(
        coreUri,
        credential,
        options);
    var coreClient = new CoreClient(
        coreUri,
        credential,
        options);
    ```

2. Make a request to the Core API with the `CoreRESTClient` class:

    ```csharp
    var status = await coreRestClient.Status.GetServiceStatusAsync();
    ```

3. Make a request to the Core API with the `CoreClient` class:

    ```csharp
    var results = await coreClient.GetAgentsAsync();
    ```

> [!TIP]
> You can use the `FoundationaLLM.Common.Authentication.DefaultAuthentication` class to generate the `TokenCredential`. This class sets the `AzureCredential` property using the `ManagedIdentityCredential` when running in a production environment (`production` parameter of the `Initialize` method) and the `AzureCliCredential` when running in a development environment.
>
> Example:
>
> `DefaultAuthentication.Initialize(false, "Test");`
> `var credentials = DefaultAuthentication.AzureCredential;`

### Use dependency injection with a configuration file

Rather than manually instantiating the `CoreRESTClient` and `CoreClient` classes, you can use dependency injection to manage the instances. This approach is more flexible and allows you to easily switch between different implementations of the `ICoreClient` and `ICoreRESTClient` interfaces.

1. Create a configuration file (e.g., `appsettings.json`) with the following content:

    ```json
    {
     "FoundationaLLM": {
      "APIs": {
       "CoreAPI": {
        "APIUrl": "https://localhost:63279/"
       }
      }
     }
    }
    ```

2. Read the configuration file:

    ```csharp
    var configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build();
    ```

3. Use the `CoreClient` extension method to add the `CoreClient` and `CoreRESTClient` to the service collection:

    ```csharp
    var services = new ServiceCollection();
    var credential = new AzureCliCredential(); // Can use any TokenCredential implementation, such as ManagedIdentityCredential or AzureCliCredential.
    services.AddCoreClient(configuration[AppConfigurationKeys.FoundationaLLM_APIs_CoreAPI_APIUrl]!, credential);

    var serviceProvider = services.BuildServiceProvider();
    ```

4. Retrieve the `CoreClient` and `CoreRESTClient` instances from the service provider:

    ```csharp
    var coreClient = serviceProvider.GetRequiredService<ICoreClient>();
    var coreRestClient = serviceProvider.GetRequiredService<ICoreRESTClient>();
    ```

Alternately, you can inject the `CoreClient` and `CoreRESTClient` instances directly into your classes using dependency injection.

```csharp
public class MyService
{
    private readonly ICoreClient _coreClient;
    private readonly ICoreRESTClient _coreRestClient;

    public MyService(ICoreClient coreClient, ICoreRESTClient coreRestClient)
    {
        _coreClient = coreClient;
        _coreRestClient = coreRestClient;
    }
}
```

### Use dependency injection with Azure App Configuration

If you prefer to retrieve the configuration settings from Azure App Configuration, you can use the `Microsoft.Azure.AppConfiguration.AspNetCore` or `Microsoft.Extensions.Configuration.AzureAppConfiguration` package to retrieve the configuration settings from Azure App Configuration.

1. Connect to Azure App Configuration:

    ```csharp
    var configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables()
        .AddAzureAppConfiguration(options =>
        {
            options.Connect("<connection-string>");
            options.ConfigureKeyVault(kv =>
            {
                kv.SetCredential(Credentials);
            });
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_Instance);
            options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIs);
        })
        .Build();
    ```

    > If you have configured your [local development environment](https://docs.foundationallm.ai/development/development-local.html), you can obtain the App Config connection string from an environment variable (`Environment.GetEnvironmentVariable(EnvironmentVariables.FoundationaLLM_AppConfig_ConnectionString)`) when developing locally.

2. Use the `CoreClient` extension method to add the `CoreClient` and `CoreRESTClient` to the service collection:

    ```csharp
    var services = new ServiceCollection();
    var credential = new AzureCliCredential(); // Can use any TokenCredential implementation, such as ManagedIdentityCredential or AzureCliCredential.
    services.AddCoreClient(configuration, scope);

    services.AddCoreClient(configuration[AppConfigurationKeys.FoundationaLLM_APIs_CoreAPI_APIUrl]!, credential);
    ```

3. Retrieve the `CoreClient` and `CoreRESTClient` instances from the service provider:

    ```csharp
    var coreClient = serviceProvider.GetRequiredService<ICoreClient>();
    var coreRestClient = serviceProvider.GetRequiredService<ICoreRESTClient>();
    ```

### Example projects

The `Core.Examples` test project contains several examples that demonstrate how to use the `CoreClient` and `CoreRESTClient` classes to interact with the Core API through a series of end-to-end tests.

## Foundationa**LLM**: The platform for deploying, scaling, securing and governing generative AI in the enterprises 🚀

[![License](https://img.shields.io/badge/license-evaluation%20and%20demo-green)](https://www.foundationallm.ai/license)

Foundationa**LLM** provides the platform for deploying, scaling, securing and governing generative AI in the enterprise. With Foundationa**LLM** you can:

- Create AI agents that are grounded in your enterprise data, be that text, semi-structured or structured data. 
- Make AI agents available to your users through a branded chat interface or integrate the REST API to the AI agent into your application for a copilot experience or integrate the Agent API in a machine-to-machine automated process.
- Experiment building agents that can use a variety of large language models including OpenAI GPT-4, Mistral and Llama 2 or any models pulled from the Hugging Face model catalog that provide a REST completions endpoint.
- Centrally manage, configure and secure your AI agents AND their underlying assets including prompts, data sources, vectorization data pipelines, vector databases and large language models using the management portal.
- Enable everyone in your enterprise to create their own AI agents. Your non-developer users can create and deploy their own agents in a self-service fashion from the management portal, but we don't get in the way of your advanced AI developers who can deploy their own orchestrations built in LangChain, Semantic Kernel, Prompt Flow or any orchestration that exposes a completions endpoint.
- Deploy and manage scalable vectorization data pipelines that can ingest millions of documents to provide knowledge to your model.
- Empower your users with as many task-focused AI agents as desired. 
- Control access to the AI agents and the resources they access using role-based access controls (RBAC).
- Harness the rapidly evolving capabilities from Azure AI and Azure OpenAI from one integrated stack. 

> [!NOTE] 
> Foundationa**LLM** is not a large language model. It enables you to use the large language models of your choice (e.g., OpenAI GPT-4, Mistral, LLama 2, etc.) 

Foundationa**LLM** deploys a secure, comprehensive and highly configurable copilot platform to your Azure cloud environment:

- Simplifies integration with enterprise data sources used by agent for in-context learning (e.g., enabling RAG, CoT, ReAct and inner monologue patterns).
- Provides defense in depth with fine-grain security controls over data used by agent and pre/post completion filters that guard against attack.
- Hardened solution attacked by an LLM red team from inception.
- Scalable solution load balances across multiple LLM endpoints.
- Extensible to new data sources, new LLM orchestrators and LLMs.

## Why is Foundationa**LLM** Needed?

Simply put we saw a lot of folks reinventing the wheel just to get a customized copilot or AI agent that was grounded and bases its responses in their own data as opposed to the trained parametric knowledge of the model. Many of the solutions we saw made for great demos, but were effectively toys wrapping calls to OpenAI endpoints- they were not something intended or ready to take into production at enterprise scale. We built Foundationa**LLM** to provide a continuous journey, one that was quick to get started with so folks could experiment quickly with LLM's but not fall off a cliff after that with a solution that would be insecure, unlicensed, inflexible and not fully featured enough to grow from the prototype into a production solution without having to start all over.  

The core problems to deliver enterprise copilots or AI agents are:

- Enterprise grade copilots or AI agents are complex and have lots of moving parts (not to mention infrastructure).
- The industry has a skills gap when it comes to filling the roles needed to deliver these complex copilot solutions.
- The top AI risks (inaccuracy, cybersecurity, compliance, explainability, privacy) are not being mitigated by individual tools.
- Delivery of a copilot or AI agent solution is time consuming, expensive and frustrating when starting from scratch.

## Documentation

Get up to speed with Foundationa**LLM** by reading the [documentation](https://docs.foundationallm.ai). This includes deployment instructions, quickstarts, architecture, and API references.

## Getting Started

Foundationa**LLM** provides a simple command line driven approach to getting your first deployment up and running. Basically, it's two commands. After that, you can customize the solution, run it locally on your machine and update the deployment with your customizations.

Follow the [Quick Start Deployment instructions](./docs/deployment/deployment-quick-start.md) to get Foundationa**LLM** deployed in your Azure subscription.

## Reporting Issues and Support

If you encounter any issues with Foundationa**LLM**, please [open an issue](https://github.com/solliancenet/foundationallm/issues) on GitHub. We will respond to your issue as soon as possible. Please use the Labels (`bug`, `documentation`, `general question`, `release x.x.x`) to categorize your issue and provide as much detail as possible to help us understand and resolve the issue.
