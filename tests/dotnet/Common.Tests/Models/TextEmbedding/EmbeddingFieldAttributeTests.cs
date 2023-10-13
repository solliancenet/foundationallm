using FoundationaLLM.Common.Models.TextEmbedding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Tests.Models.TextEmbedding
{
    public class EmbeddingFieldAttributeTests
    {
        [Fact]
        public void EmbeddingFieldAttribute_ShouldHaveLabelProperty()
        {
            // Arrange & Act
            var attribute = new EmbeddingFieldAttribute { Label = "TestLabel" };

            // Assert
            Assert.Equal("TestLabel", attribute.Label);
        }
    }
}
