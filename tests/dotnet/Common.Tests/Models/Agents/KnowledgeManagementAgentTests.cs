using FoundationaLLM.Common.Models.Agents;

namespace FoundationaLLM.Common.Tests.Models.Agents
{
    public class KnowledgeManagementAgentTests
    {
        private KnowledgeManagementAgent _knowledgeManagementAgent = new KnowledgeManagementAgent()
            { Name = "Test_agent", ObjectId = "Test_objectid", Type = AgentTypes.KnowledgeManagement };

        [Fact]
        public void KnowledgeManagementAgent_Type_IsKnowledgeManagement()
        {
            // Assert
            Assert.Equal(AgentTypes.KnowledgeManagement, _knowledgeManagementAgent.Type);
        }

        [Fact]
        public void KnowledgeManagementAgent_IndexingProfile_DefaultIsNull()
        {
            // Assert
            Assert.Null(_knowledgeManagementAgent.IndexingProfileObjectIds);
        }

        [Fact]
        public void KnowledgeManagementAgent_EmbeddingProfile_DefaultIsNull()
        {
            // Assert
            Assert.Null(_knowledgeManagementAgent.TextEmbeddingProfileObjectId);
        }
    }
}
