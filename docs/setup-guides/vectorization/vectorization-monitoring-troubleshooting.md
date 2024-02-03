# Monitoring and troubleshooting vectorization

The typical steps you have to perform when monitoring and troubleshooting vectorization in FoundationaLLM (FLLM) are:

1. Check the configuration of the Vectorization API and Vectorization Worker. For more details, see [Configuring vectorization](vectorization-configuration.md).
2. Check the working condition of the Vectorization API and Vectorization Worker(s). Ensure the services have started and initialized successfully.
3. Check the status endpoints for the Core API, Vectoriation API and the Management API. You can do this by submitting a HTTP GET request to the `/status` endpoint of these APIs and validate that you get a HTTP 200 OK response with body like `<api_name> - ready`.
4. Check the logs of the Vectorization API and Vectorization Worker(s) for errors. By default, the logs are written to the Azure App Insights Log Analytics Workspace deployed by FLLM.
5. Check the definitions of the vectorization profiles used in the vectorization requests. For more details, see [Managing vectorization profiles](vectorization-profiles.md). Ensure all the required app configuration elements are present and have the correct values.
6. Check the state of the vectorization requests. By default, the vectorization requests are stored in the `vectorization-state` container of the Azure Storage account deployed by FLLM.