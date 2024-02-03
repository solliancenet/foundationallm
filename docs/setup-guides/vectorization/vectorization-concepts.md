# Vectorization Concepts

Foundationa**LLM** (FLLM) provides utilities and services to support vectorizing your data in batch and on-demand modalities. Vectorization is a multi-step process, starting with loading your data, splitting (or chunking) the data as required, performing vector embeddings, and storing the vectors into a vector index so [an agent](../agents/index.md) can later retrieve relevant information through a vector search. In FLLM, vectorization is an idempotent operation, meaning that vectorizing the same document multiple times will result in the same vector being stored in the vector index. This is useful for re-vectorizing documents that have been updated, or for cases where only parts of the vectorization process need to be run (e.g., only the vector embeddings need to be updated).

For each individual document, vectorization is performed by executing a vectorization pipeline instance. The execution is started by the Vectorization API upon receiving a valid vectorization request. Based on the details of the vectorizaiton request, a vectorization pipeline can be executed in one of the following modes:

- Synchonously - the pipeline steps start executing immediately and execute sequentially until the pipeline is completed or an error occurs. This type of execution is used for on-demand vectorization and is well suited for small to medium sized documents and relatively small numbers of documents at a time.
- Asynchronously - the pipeline steps are submitted to queues and executed by workers. This type of execution is used for batch vectorization and is well suited for large numbers of documents at a time. It is also well suited for vectorizing documents of any size.

The FLLM platform components involved in vectorization are:

- Vectorization API (processes vectorization requests and executes synchonous vectorization pipelines).
- Vectorization Worker(s) (execute asynchronous vectorization pipelines).

A FLLM instance deploys one instance of the Vectorization API and one or more instances of the Vectorization Worker. See [Configuring vectorization](vectorization-configuration.md) for more details on configuring these components.

> [!NOTE]
> The initialization of both the Vectorization API and the Vectorization Worker is a time consuming process, as it involves dowloading and initializing various elements (e.g., Byte-Pair encoding dictionaries). As a result, after restarting the API, it might take up to a minute until it becomes ready to accept vectorization requests. It is recommended to either use the status endpoint of the Vectorization API to determine when it is ready to accept requests, or to wait for a minute after restarting the API before sending vectorization requests.

Vectorization pipelines are started when the Vectorization API receives a vectorization request. The following types of triggers are supported:

- None (no triggering of vectorization pipelines).
- Manual (vectorization pipelines are triggered manually by calling the Vectorization API). The typical use cases for on-demand vectorization (either synchronous or asynchronous) are testing, manual vectorization (or re-vectorization), and application integration (where another platform component triggers vectorization).
- Content-based (vectorization pipelines are triggered automatically when either new content is added to a content source or existing content is updated).
- Schedule-based (vectorization pipelines are triggered automatically based on a schedule).

> [!NOTE]
> Content-based and schedule-based triggering are currently in pre-release and are not yet available in public releases of FLLM.

When working with vectorization in FLLM, the typical steps you have to perform are:

- Ensure that the Vectorization API and Vectorization Worker are configured and running. This is a one-time operation. For more details, see [Configuring vectorization](vectorization-configuration.md).
- Create vectorization profiles. You can either reuse existing profiles or create new ones. For more details, see [Managing vectorization profiles](vectorization-profiles.md).
- Submit vectorization requests to the Vectorization API. For more details, see [Triggering vectorization](vectorization-triggering.md).

