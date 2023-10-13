using FoundationaLLM.Common.Models.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Tests.Models.Search
{
    public class ShortTermMemoryTests
    {
        [Fact]
        public void Constructor_ShouldInitializeMemoryProperty()
        {
            // Arrange
            string expectedMemory = "Memory_1";

            // Act
            var shortTermMemory = new ShortTermMemory
            {
                memory__ = expectedMemory
            };

            // Assert
            Assert.Equal(expectedMemory, shortTermMemory.memory__);
        }
    }
}
