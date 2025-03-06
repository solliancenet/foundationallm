# Agents and Workflows

Foundationa**LLM** (FLLM) agents are the core of the platform solution. They are responsible for providing users with a customized experience based on its configuration.

## Creation of a new Agent

To create a new agent, you can use the **Create New Agent** hyperlink in the **Agents** section of the **Management Portal**. 

## What type of Agent are you interested in?

FoundationaLLM allows you to create several types of agents depending on your needs. The following types of agents are available:

- **LangChainExpressionLanguage**: This type of agent allows you to send your requests directly to an LLM to perform a wide range of tasks. These agents can be configured to use different models and prompts to provide customized responses to user requests.

- **OpenAIAssistants**: This type of agent is based on the OpenAI Assistants framework and allows you to create agents that can perform a wide range of tasks using the OpenAI API. These agents can be configured to use different models, prompts, and tools to provide customized responses to user requests. They give you access to the main three tools in OpenAI Assistants - **Code Interpreter**, **File Search** and **Function Calling**.

- **LangGraphReactAgent**: This type of agent is based on the LangGraph framework and allows you to create agents that can perform a wide range of tasks using the LangGraph API. These agents can be configured to use different models, prompts, and tools to provide customized responses to user requests. Currently, you have access to DALLE Image Generation tool to generate images and the FoundationaLLM Content Search Tool to search vectore stores predefined and vectorized through the FoundationaLLM vectorization services.

- **ExternalAgentWorkflow**: This type of agent allows you to create agents that can use external workflows developed in Python. These agents can be configured to use different models, prompts, and tools to provide customized responses to user requests.


### The creation of a new agent consists of 5 sections:

- General
- Agent Configuration
- User Portal Experience
- Knowledge Source
- Workflow
- Tools
- Security (available only after the initial creation of the agent)

### General Section

In this section, you can define the name, description and welcome message of the agent.  The Welcome message is what a user will see in the Chat portal as soon as they pick that agent from the dropdown to learn about the agent and its services that it provides before starting a chat conversation.

![General Agent information](./media/agent_Workflow_1.png)

### Agent Configuration Section

In this section, you can define the following configurations:

- **Chat History**: This setting allows you to enable or disable the chat history feature for the agent. When enabled, the agent will remember the context of previous conversations, allowing for more personalized and relevant responses. If disabled, the agent will not retain any memory of past interactions. It also allows you to define the maximum number of messages to be used in the conversation thread, the default is 5 messages.  

> [!NOTE]
> This maximum number is not related to the storing of conversation history in Cosmos DB as ALL of them are saved, it is only for the maximum number to be included in follow up request in the conversation.


- **Gatekeeper**: This setting allows you to enable or disable the gatekeeper feature for the agent. When enabled, the agent will have a gatekeeper that can filter and moderate the content of conversations, ensuring that inappropriate or harmful content is not generated. If disabled, the agent will not have any content moderation capabilities. 
You can choose from multiple options for the content safety:
  - **Azure Content Safety**
  - **Azure Content Safety Prompt Shield**
  - **Lakera Guard**
  - **Enkrypt Guardrails**
The Gatekeeper also allows you to enable the Data Protection aspect of the agent, which currently uses **Microsoft Presidio** to filter sensitive data in the conversations.

- **Prompt Rewrite**: This setting allows you to enable it and specify the rewrite LLM model desired, the pre-defined prompt that you have authored in your prompt hub and the window size of the rewrite.  This is a very useful feature when tools are involved to carry on the context of the question between responses.

- **Semantic Cache**: This is a very useful feature that you can enable and choose the model desired, the embedding dimension (like 1536 for ada-002 for example) and the minimum similarity threshold that you want to evaluate to determine if it is worth it to go back to the LLM (financial implication and speed) or if it is acceptable to return a cache response for a similar previous question as long as it meets the minimum similarity threshold.

- **Cost Center**: This setting allows you to define a cost center for the agent. A cost center is a department or unit within an organization that is responsible for its own expenses and budget. By assigning a cost center to the agent, you can track and manage the costs associated with its operations.

- **Expiration**: This setting allows you to define an expiration date for the agent. After this date, the agent will no longer be available for use. This is useful for managing the lifecycle of agents and ensuring that they are only active when needed.

> [!NOTE]
> Once an agent expires, it won’t respond to requests and is not visible in the agent selection dropdown in the chat portal.

![Agent Configuration Section](./media/agent_Workflow_2.png)

### User Portal Experience Section

![Agent Configuration Section](./media/agent_workflow_2b.png)

These setting allows you to turn on or off 4 valuable capabilities in the Chat Portal.
- The number of tokens used in the conversation. (Questions and Responses)
- The prompt used by the agent for a specific question including history and context.
- The option to rate the response of the agent.
-	The ability to allow the user to upload files to the agent in the conversation.

### Workflow Section

In this section, you can define the workflow of the agent. The workflow is a sequence of steps that the agent follows to process user requests and provide responses. You can define the following aspects of the workflow:

> [!NOTE]
> Please check each section below for each type of agent to understand the differences in the configuration of the workflow.

[LangchainExpressionLanguage](workflow_langchainexpressionlanguage.md)

[OpenAIAssistants](workflow_openaiassistants.md)

[LangGraphReactAgent](workflow_langgraphreact.md)

[ExternalAgentWorkflow](workflow_external.md)

### Security Section

In this section, you can define an Agent Access Token to be used by the agent. The Agent Access Token is a security token that is used to authenticate and authorize access to the agent's resources and services. It is a unique identifier that is generated for each agent and is used to ensure that only authorized users can access the agent's capabilities without requiring the user to be authenticated using Entra ID credentials. 
This is particularly useful for public applications that want to provide access to the agent without requiring users to log in with their Entra ID credentials.

![Agent Access Token configuration](./media/agent_Workflow_6.png)

[Access Token scenario](Agent_AccessToken.md)

