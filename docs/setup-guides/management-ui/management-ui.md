# Management UI

The Management UI enables FLLM administrators to configure agents without directly calling the Management API.

## Creating New Agent

1. Navigate to the **Create New Agent** page using the side navigation bar.
    
    ![FLLM Create New Agent tab.](../media/fllm-management-interface.png "Create New Agent")

2. Set the agent type: **Knowledge Management** or **Analytics**.

    ![Create New Agent select Agent Type.](../media/agent-type-selection.png "Agent Type")

3. Set the agent Knowledge Source:

    ![Agent Knowledge Source four-tile view.](../media/agent-knowledge-source.png "Agent Knowledge Source")

     - Expand the dropdown arrow next to the upper left box. Select the entry with the correct **Storage account name**, artifacts **Container name**, and file **Data Format(s)**. Then, select **Done**.

        ![Agent Blob Storage Data Sources.](../media/agent-data-source-dropdown.png "Blob Storage Data Sources")

     - Expand the dropdown arrow next to the upper right box to open the Azure AI Search index dropdown. The vectorized content will be populated in the selected index. Select **Done**.

        ![Agent Knowledge Source Index Selection.](../media/aisearch-index-dropdown.png "Index Selection")
     
     - Expand the dropdown arrow next to the lower left box. Set the **Chunk size** and **Overlap size** settings for vectorization. Select **Done**.

        ![Agent Splitting & Chunking Configuration.](../media/set-splitting-and-chunking.png "Splitting & Chunking")

     - Expand the dropdown arrow next to the lower right box. Set the trigger **Frequency** and select **Done**.

        ![Agent Vectorization Trigger Frequency.](../media/vectorization-trigger.png "Vectorization Trigger Frequency")

4. Configure user-agent interactions.

    ![User-Agent Interactions & Gatekeeper Configuration.](../media/user-agent-interactions-config.png "User-Agent Interactions")

    - Enable conversation history using the `Yes/No` Radio Button. Select **Done**.

        ![Agent Enable/Disable Conversation History.](../media/enable-disable-conversation-history.png "Enable/Disable Conversation History.")

    - Configure the Gatekeeper. Then, select **Done**.
        - `Enable/Disable` the Gatekeeper using the Radio Button
        - Set the **Content Safety** platform to either `None` or `Azure Content Safety` using the dropdown menu
        - Set the **Data Protection** platform to either `None` or `Microsoft Presidio` using the dropdown menu

        ![Agent Gatekeeper Status, Content Safety Configuration, and Data Protection Configuration.](../media/gatekeeper-config.png "Gatekeeper Configuration")

5. Lastly, set the **System Prompt**. The prompt prefixes users' requests to the agent, influencing the tone and functionality of the agent.

    ![Set Agent Prompt.](../media/set-system-prompt.png "Agent Prompt")

6. After setting the desired agent configuration, select **Create Agent** at the bottom right-hand corner of the page. You will be able to edit the agent configuration after creation.
