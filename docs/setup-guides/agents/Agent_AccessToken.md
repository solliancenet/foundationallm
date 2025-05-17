# Agent Access Token

To allow the flexibility of using an agent without requiring the user to be authenticated using Entra ID credentials, you can create an **Agent Access Token**. This is particularly useful for public applications that want to provide access to the agent without requiring users to log in with their Entra ID credentials.

## Quick Start to creating and using an Agent Access Token

1. In the **Security** section of the agent configuration, click on the **Create Access Token** button to create a new access token.
   
   ![Create Access Token](./media/agent_Workflow_6.png)

2. In the **Create Access Token** dialog, enter a description for the access token and an expiration date then click on the **Create Access Token** button.
   
   ![Create Access Token Dialog](./media/agent_Workflow_7.png)

3. The access token will be created and displayed in a dialog, make sure to save it or copy it for future use.
   
   ![Access Token Created](./media/agent_Workflow_8.png)

4. Download the free version of POSTMAN from https://getpostman.com  
5. Clone the POSTMAN collection for FoundationaLLM CoreAPI from https://docs.foundationallm.ai/development/calling-apis/directly-calling-core-api.html#postman-collection and import it into POSTMAN.
   
   ![POSTMAN](./media/agent_Workflow_12.png)
6. Under `sessions`, pick the POST `Creates a new Chat session with Agent Access Token`
7. For this POST you will create a new chat session as the request for completion later requires a `sessionId`
8. Start by setting the Authorization type to No Auth
   
    ![POSTMAN Authorization](./media/agent_Workflow_13.png)
    
9. On the Headers tab, set a new key X-AGENT-ACCESS-TOKEN with the value of the Access Token that you were asked to save or copy previously

    ![POSTMAN Headers](./media/agent_Workflow_14.png)

10. On the Body tab, enter a name for your chat conversation session in the name key in raw JSON format

    ![POSTMAN Body](./media/agent_Workflow_15.png)

11. Click on the `Send` button to send the request and save the `sessionId` from the response as it will be needed in the next step.
12.	Now, let’s POST to the completion endpoint to get a response from the agent utilizing the sessionId received in the previous step.
13.	Under `completions`, expand `completion` and pick POST `Requests a completion with Agent Access Token`
14. Set the Authorization type to No Auth

    ![POSTMAN second Authorization](./media/agent_Workflow_16.png)

15. On the Headers tab, set a new key X-AGENT-ACCESS-TOKEN with the value of the Access Token that you were asked to save or copy previously
    
    ![POSTMAN second Headers](./media/agent_Workflow_17.png)

16. On the Body tab, enter the sessionId received in the previous POST in the sessionId key in raw JSON format
    
    ![POSTMAN second Body](./media/agent_Workflow_18.png)

17. Click on the `Send` button to send the request and receive a response from the agent explaining why the sky is blue.