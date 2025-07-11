<template>
  <main id="main-content">
    <div style="display: flex">
        <div style="flex: 1">
            <h2 class="page-header">Knowledge Sources</h2>
            <div class="page-subheader"></div>
        </div>

        <AccessControl
            v-if="selectedKnowledgeSource"
            :scopes="[
                {
                    label: 'Knowledge Source',
                    value: `providers/FoundationaLLM.Context/knowledgeSources/${selectedKnowledgeSource.resource.name}`,
                },
            ]"
        />
    </div>

    <div :class="{ 'grid--loading': loading }">
        <!-- Loading overlay -->
        <template v-if="loading">
        <div class="grid__loading-overlay" role="status" aria-live="polite">
            <LoadingGrid />
            <div>{{ loadingStatusText }}</div>
        </div>
        </template>

        <div class="w-full flex gap-4">
            <Dropdown
                v-model="selectedKnowledgeSource"
                :options="knowledgeSources"
                option-label="resource.name"
                placeholder="-- Select Knowledge Source --"
            />
            <Button
                type="button"
                icon="pi pi-refresh"
                @click="getKnowledgeSources"
                v-tooltip="'Refresh knowledge sources'"
            />
            <Button
                v-if="selectedKnowledgeSource && hasKnowledgeGraph"
                type="button"
                icon="pi pi-share-alt"
                @click="renderKnowledgeGraph"
                v-tooltip="'Render knowledge graph'"
            />
        </div>

        <div
            v-if="selectedKnowledgeSource"
            class="query-grid">

            <div style="grid-row: span 5;">
                <div style="display: flex; flex-direction: column;">
                    <label class="query-grid-label" for="input-name">User prompt to test:</label>
                    <InputTextArea
                        id="input-name"
                        v-model="queryRequest.user_prompt"
                        class="w-full"
                        placeholder="Enter the user prompt..."
                        style="width: 90%; height: 100%; resize: none;"
                    />
                    <Button
                        type="button"
                        style="margin-top: 10px; width: 250px; display: flex; align-items: center; justify-content: center;"
                        @click="queryKnowledgeSource"
                    >Query Knowledge Source</Button>
                </div>
            </div>

            <div>
                <div><h4 class="page-header">Text Content</h4></div>
                <div>
                    <label class="query-grid-label">Text chunks maximum count: {{ queryRequest.text_chunks_max_count }}</label>
                    <Slider
                        v-model="queryRequest.text_chunks_max_count"
                        :min="0"
                        :max="30"
                        :step="1"
                        style="width: 200px;"
                    />
                </div>
                <div>
                    <div class="flex items-center gap-2">
                        <label>Use semantic ranking: </label>
                        <Checkbox
                            v-model="queryRequest.use_semantic_ranking"
                            :binary="true"
                        />
                    </div>
                </div>
                <div
                    v-if="hasKnowledgeGraph">
                    <h4 class="page-header">Knowledge Graph</h4>
                </div>
                <div
                    v-if="hasKnowledgeGraph">
                    <label class="query-grid-label">Mapped entities max count: {{ knowledge_graph_query.mapped_entities_max_count }}</label>
                    <Slider
                        v-model="knowledge_graph_query.mapped_entities_max_count"
                        :min="0"
                        :max="100"
                        :step="1"
                        style="width: 200px;"
                    />
                </div>
                <div
                    v-if="hasKnowledgeGraph">
                    <label class="query-grid-label">Relationships max depth: {{ knowledge_graph_query.relationships_max_depth }}</label>
                    <Slider
                        v-model="knowledge_graph_query.relationships_max_depth"
                        :min="0"
                        :max="5"
                        :step="1"
                        style="width: 200px;"
                    />
                </div>
            </div>
            <div>
                <div></div>
                <div>
                    <label class="query-grid-label">Text chunks similarity threshold: {{ queryRequest.text_chunks_similarity_threshold }}</label>
                    <Slider
                        v-model="queryRequest.text_chunks_similarity_threshold"
                        :min="0"
                        :max="1"
                        :step="0.01"
                        style="width: 200px;"
                    />
                </div>
                <div></div>
                <div></div>
                <div
                    v-if="hasKnowledgeGraph">
                    <label class="query-grid-label">Mapped entities similarity threshold: {{ knowledge_graph_query.mapped_entities_similarity_threshold }}</label>
                    <Slider
                        v-model="knowledge_graph_query.mapped_entities_similarity_threshold"
                        :min="0"
                        :max="1"
                        :step="0.01"
                        style="width: 200px;"
                    />
                </div>
                <div
                    v-if="hasKnowledgeGraph">
                    <label class="query-grid-label">All entities max count: {{ knowledge_graph_query.all_entities_max_count }}</label>
                    <Slider
                        v-model="knowledge_graph_query.all_entities_max_count"
                        :min="0"
                        :max="200"
                        :step="1"
                        style="width: 200px;"
                    />
                </div>
            </div>
        </div>

        <div
            v-if="showQueryResult || showKnowledgeGraph"
            style="display: flex; flex-direction: row; gap: 2rem; align-items: flex-start; margin-top: 2rem; height: 50vh;"
            >
            <div v-if="showQueryResult" style="flex: 1; display: flex; flex-direction: column; height: 100%;">
                <div style="display: flex; align-items: center; gap: 0.5rem; margin-bottom: 0.5rem; justify-content: space-between;">
                    <label class="query-grid-label" style="margin-bottom: 0;">Query Result</label>
                    <Button
                        type="button"
                        icon="pi pi-share-alt"
                        @click="renderQueryResult"
                        v-tooltip="'Render query result'"
                        size="small"
                        style="padding: 0; font-size: 0.7rem; min-width: 1rem; width: 1.5rem; height: 1.5rem; opacity: 0.75;"
                    />
                </div>
                <JsonEditorVue
                v-model="queryResult"
                :read-only="true"
                style="flex: 1 1 0; min-height: 0px; overflow-y: auto;"
                />
            </div>
            <div
                v-if="showKnowledgeGraph"
                class="sigma-graph-container"
                style="flex: 1; display: flex; flex-direction: column; height: 100%;"
            >
                <label class="query-grid-label">Knowledge Graph</label>
                <div
                ref="sigmaContainer"
                style="flex: 1 1 0; min-height: 0px; overflow-y: auto; border: 1px solid #ccc; border-radius: 4px; background: #fafbfc;"
                ></div>
            </div>
        </div>
    </div>
  </main>
</template>

<script lang="ts">
import api from '@/js/api';
import type { ResourceProviderGetResult } from '@/js/types';
import JsonEditorVue from 'json-editor-vue';
import Sigma from "sigma";
import Graph from "graphology";
import forceAtlas2 from "graphology-layout-forceatlas2";

export default {
  name: 'KnowledgeSources',

  components: {
		JsonEditorVue,
	},
  
  data() {
    return {
      knowledgeSources: [] as any[],
      selectedKnowledgeSource: null,
      hasKnowledgeGraph: false as boolean,
      showKnowledgeGraph: true as boolean,
      showQueryResult: true as boolean,
      queryRequest: {
        user_prompt : null as string | null,
        text_chunks_max_count: 20 as number,
        text_chunks_similarity_threshold: 0.30 as number,
        use_semantic_ranking: true as boolean,
        knowledge_graph_query: null,
      },
      knowledge_graph_query: {
            mapped_entities_max_count: 10 as number,
            mapped_entities_similarity_threshold: 0.35 as number,
            relationships_max_depth: 2 as number,
            all_entities_max_count: 20 as number
        } as any,
      queryResult: null,
      sigmaInstance: null as Sigma | null,
      loading: false as boolean,
      loadingStatusText: 'Retrieving knowledge sources...' as string,
    };
  },

  async created() {
    await this.getKnowledgeSources();
  },
  
  watch: {
    selectedKnowledgeSource(newVal) {
        this.hasKnowledgeGraph = !!(newVal && newVal.resource && newVal.resource.has_knowledge_graph);
        if (this.hasKnowledgeGraph) {
            this.queryRequest.knowledge_graph_query = this.knowledge_graph_query;
        } else {
            this.queryRequest.knowledge_graph_query = null;
        }
    }
    },

  methods: {
    async getKnowledgeSources() {
        this.loadingStatusText = 'Retrieving knowledge sources...';
        this.loading = true;
        try {
            this.knowledgeSources = await api.getKnowledgeSources();
        } catch (error) {
            this.$toast?.add?.({
            severity: 'error',
            detail: error?.response?._data || error,
            life: 5000,
            });
        }
        this.loading = false;
    },

    async queryKnowledgeSource() {
        // Validation: ensure a knowledge source is selected and user prompt is not empty
        if (!this.selectedKnowledgeSource) {
            this.$toast?.add?.({
            severity: 'warn',
            detail: 'Please select a knowledge source.',
            life: 3000,
            });
            return;
        }
        if (!this.queryRequest.user_prompt || !this.queryRequest.user_prompt.trim()) {
            this.$toast?.add?.({
            severity: 'warn',
            detail: 'Please enter a user prompt.',
            life: 3000,
            });
            return;
        }
        // ...existing logic to query the knowledge source...
        this.loadingStatusText = `Querying knowledge source: ${this.selectedKnowledgeSource.resource.name}...`;
        this.loading = true;
        try {
            this.queryResult = await api.queryKnowledgeSource(
                this.selectedKnowledgeSource.resource.name,
                this.queryRequest
            );
        } catch (error) {
            this.$toast?.add?.({
            severity: 'error',
            detail: error?.response?._data || error,
            life: 5000,
            });
        }
        this.loading = false;
    },

    async renderKnowledgeGraph() {
        // Defensive: ensure we have a selected knowledge source
        if (!this.selectedKnowledgeSource) {
            this.$toast?.add?.({
                severity: 'warn',
                detail: 'Please select a knowledge source to render the graph.',
                life: 3000,
            });
            return;
        }
        if (!this.hasKnowledgeGraph) {
            this.$toast?.add?.({
                severity: 'warn',
                detail: 'The selected knowledge source does not have a knowledge graph.',
                life: 3000,
            });
            return;
        }
        // Example: fetch graph data from API or use existing data
        this.loadingStatusText = `Rendering knowledge graph for ${this.selectedKnowledgeSource.resource.name}...`;
        this.loading = true;
        api.renderKnowledgeSourceGraph(this.selectedKnowledgeSource.resource.name, {})
            .then(data => {
                this.renderSigmaGraph(data);
            })
            .catch(error => {
                this.$toast?.add?.({
                    severity: 'error',
                    detail: error?.response?._data || error,
                    life: 5000,
                });
            })
            .finally(() => {
                this.loading = false;
            });
    },

    renderQueryResult() {
        // Defensive: ensure we have a query response to render
        if (!this.queryResult) {
            this.$toast?.add?.({
                severity: 'warn',
                detail: 'No query result to render.',
                life: 3000,
            });
            return;
        }

        let data = {
            nodes: [],
            edges: []
        }
        let graphResponse = this.queryResult.knowledge_graph_response;

        // Main entities
        if (graphResponse.entities && Array.isArray(graphResponse.entities)) {
            data.nodes = graphResponse.entities.map((entity: any) => ({
                id: `${entity.type}:${entity.name}`,
                label: entity.name
            }));
        }

        const existingIds = new Set(data.nodes.map(n => n.id));

        // Related entities
        if (graphResponse.related_entities && Array.isArray(graphResponse.related_entities)) {
            // Avoid duplicate nodes by checking existing ids
            graphResponse.related_entities.forEach((entity: any) => {
                const id = `${entity.type}:${entity.name}`;
                if (!existingIds.has(id)) {
                    data.nodes.push({
                        id,
                        label: entity.name
                    });
                    existingIds.add(id);
                }
            });
        }

        // Relations/edges
        if (graphResponse.relationships && Array.isArray(graphResponse.relationships)) {
            graphResponse.relationships.forEach((rel: any) => {
                const sourceId = `${rel.source_type}:${rel.source}`;
                const targetid = `${rel.target_type}:${rel.target}`;
                // Ensure both source and target nodes exist
                if (existingIds.has(sourceId) && existingIds.has(targetid)) {
                    data.edges.push([sourceId, targetid]);
                }
            });
        }

        // Render the graph with the transformed data
        this.renderSigmaGraph(data, 5, { iterations: 1000, settings: { gravity: 10, scalingRatio: 1 } });
    },

    renderSigmaGraph(data: any, nodeSize: number = 1, forceAtlas2Settings: any = { iterations: 100, settings: { gravity: 0.3, scalingRatio: 3 } }) {
        // Destroy previous instance if exists
        if (this.sigmaInstance) {
            this.sigmaInstance.kill();
            this.sigmaInstance = null;
        }
        // Example: expects data to have nodes and edges arrays
        if (!data || !Array.isArray(data.nodes) || !Array.isArray(data.edges)) return;

        this.$nextTick(() => {
            const container = this.$refs.sigmaContainer as HTMLElement;
            if (!container) return; // Defensive: skip if not rendered

            const graph = new Graph();
            // Add nodes
            data.nodes.forEach((node: any) => {
                graph.addNode(node.id, { label: node.label, x: Math.random(), y: Math.random(), size: nodeSize });
            });
            // Add edges
            data.edges.forEach((edge: any[]) => {
                graph.addEdge(edge[0], edge[1]);
            });

            // Run ForceAtlas2 layout to compute node positions
            forceAtlas2.assign(graph, forceAtlas2Settings);

            this.sigmaInstance = new Sigma(graph, container, this.$options.settings);
        });
    }
  },

  mounted() {
        // Add Sigma node/edge selection logic after Sigma instance is created
        this.$watch(
            () => this.sigmaInstance,
            (sigmaInstance) => {
            if (!sigmaInstance) return;
            // Remove previous listeners if any
            sigmaInstance.getGraph().forEachNode((node) => {
                sigmaInstance.getGraph().setNodeAttribute(node, "highlighted", false);
            });
            sigmaInstance.getGraph().forEachEdge((edge) => {
                sigmaInstance.getGraph().setEdgeAttribute(edge, "highlighted", false);
            });

            sigmaInstance.on("clickNode", ({ node }) => {
                const graph = sigmaInstance.getGraph();
                // Highlight the clicked node
                graph.updateNodeAttribute(node, "highlighted", () => true);
                // Highlight all edges connected to the node
                graph.forEachEdge((edge, attr, source, target) => {
                if (source === node || target === node) {
                    graph.updateEdgeAttribute(edge, "highlighted", () => true);
                } else {
                    graph.updateEdgeAttribute(edge, "highlighted", () => false);
                }
                });
                // Optionally, unhighlight other nodes
                graph.forEachNode((n) => {
                if (n !== node) graph.updateNodeAttribute(n, "highlighted", () => false);
                });
            });

            sigmaInstance.on("clickStage", () => {
                // Unhighlight all nodes and edges
                const graph = sigmaInstance.getGraph();
                graph.forEachNode((node) => graph.updateNodeAttribute(node, "highlighted", () => false));
                graph.forEachEdge((edge) => graph.updateEdgeAttribute(edge, "highlighted", () => false));
            });
            },
            { immediate: true }
        );
    },
    settings: {
        nodeReducer(node, data) {
            if (data.highlighted) {
            return { ...data, color: "#f39c12" };
            }
            return data;
        },
        edgeReducer(edge, data) {
            if (data.highlighted) {
            return { ...data, color: "#f39c12", size: 2 };
            }
            return data;
        }
    }
};
</script>

<style lang="scss" scoped>

.grid--loading {
  pointer-events: none;
}

.grid__loading-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  gap: 16px;
  z-index: 10;
  background-color: rgba(255, 255, 255, 0.9);
  pointer-events: none;
}

.query-grid-label {
  margin-bottom: 0.8rem;
}

.query-grid {
  display: grid;
  grid-template-columns: 3fr 1fr 1fr;
  grid-template-rows: repeat(6, 60px);
  gap: 1rem;
  margin-top: 1.5rem;
  margin-bottom: 1rem;
  justify-content: start; /* Align grid to the left if not filling container */
}

.query-grid > div > div {
  padding: 0.5rem;
  height: 100%; /* Make each cell fill the row height */
  box-sizing: border-box;
  display: flex;
  flex-direction: column;
  justify-content: center; /* Optional: center content vertically */
}

</style>