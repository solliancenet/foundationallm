# Core API

The Core API serves as the entry point for user requests to FLLM's underlying engine. While clients primarily interact with the Core API through the Chat UI, the Core API exposes some convenient interfaces for developers.

## Session-less Completion

The session-less completion endpoint enables users to query agents without first creating a chat session.

**Endpoint:** `[DEPLOYMENT URL]/core/orchestration/completion?api-version=1.0`

>**Note:** For AKS deployments, `[DEPLOYMENT URL]` is the same as the cluster FQDN, while for ACA deployments, the Core API endpoint can be found by navigating to the `[DEPLOYMENT PREFIX]coreca` Container App in the Azure Portal.

**Sample Request:**

```json
{"user_prompt": "What are your capabilities?"}
```

**Payload Headers:**

| Header | Value | Details |
| ------ | ----- | ------- |
| `Authorization` | `Bearer [ENTRA ID BEARER TOKEN]` | Valid token from Entra ID |
| `X-AGENT-HINT` | `{"name": "[AGENT NAME]", "private": [true/false]}` | JSON document specifying the desired agent to handle the completion request |
| `Content-Type` | `application/json` | |

**Sample Response:**

```json
{
    "text": "FoundationaLLM is a copilot platform that simplifies and streamlines building knowledge management and analytic agents over the data sources present across your enterprise. It provides integration with enterprise data sources used by agents for in-context learning, fine-grain security controls over data used by agents, and pre/post completion filters that guard against attack. The solution is scalable and load balances across multiple endpoints. It is also extensible to new data sources, new LLM orchestrators, and LLMs. You can learn more about FoundationaLLM at https://foundationallm.ai."
}
```

**Sample Postman Request:** `/orchestration/completion/Requests a completion from the downstream APIs.`