# Deployment

## Deployment choices

The following table summarizes the deployment choices available for the solution:

 Deployment type | Description | When to use
--- | --- | ---
[Standard](./deployment-standard.md) | Use your local development environment to deploy the solution to your Azure subscription. | Best suited for situations where you need the flexibility of a full development environment (e.g. to customize the solution) and you have a local development environment available.
[CloudShell]() | *Coming Soon* - Use Azure CloudShell to deploy the solution using only a browser. | Best suited for situations where you want to deploy without needing to install anything in your local development environment. 

Select the links in the table above to learn more about each deployment choice.

## Deployment validation

Use the steps below to validate that the solution was deployed successfully.

Once the deployment script completes, the Application Insights `traces` query should display the following sequence of events:

![API initialization sequence of events](../media/initialization-trace.png)

Next, you should be able to see multiple entries referring to the vectorization of the data that was imported into Cosmos DB:

![API vectorization sequence of events](../media/initialization-embedding.png)

Finally, you should be able to see the Cognitive Search index being populated with the vectorized data:

![Cognitive Search index populated with vectorized data](../media/initialization-vector-index.png)

>**NOTE**:
>
>It takes several minutes until all imported data is vectorized and indexed.

## Monitoring with Application Insights

Use the steps below to monitor the solution with Application Insights:

1. Navigate to the `Application Insights` resource that was created as part of the deployment.

2. Select the `Logs` section and create a new query with the following statement. Select the `Run` button to execute the query:

    ```kql
    traces
    | order by desc timestamp
    ```

    ![Application Insights query](../media/monitoring-traces.png)

3. Select the `Export` button to explort the results the query.

4. In the query, replace `traces` with `requests` or `exceptions` to view the corresponding telemetry.