using FoundationaLLM.Common.Models.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Tests.Models.Search
{
    public class EmbeddedEntityTests
    {
        [Fact]
        public void Constructor_ShouldInitializeVectorToNull()
        {
            var embeddedEntity = new EmbeddedEntity();

            Assert.Null(embeddedEntity.vector);
        }

        [Fact]
        public void Constructor_ShouldInitializeEntityType()
        {
            var embeddedEntity = new EmbeddedEntity{ entityType__ = "Type_1"};

            Assert.NotNull(embeddedEntity.entityType__);
        }
    }
}
