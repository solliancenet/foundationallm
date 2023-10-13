using FoundationaLLM.Common.Models.Chat;

namespace FoundationaLLM.Common.Tests.Models.Chat
{
    public class DocumentVectorTests
    {
        public static IEnumerable<object[]> GetInvalidFields()
        {
            yield return new object[] { null, "Partition_1", "Container_1", null };
            yield return new object[] { "Item_1", null, "Container_1", null };
            yield return new object[] { "Item_1", "Partition_1", null, null };
            yield return new object[] { "Item_1", "Partition_1", "Container_1", null };
            yield return new object[] { "Item_1", "Partition_1", "Container_1", new float[0] };
            yield return new object[] { "Item_1", "Partition_1", "Container_1", new float[] { 1, 2, 3 } };
        }

        public static IEnumerable<object[]> GetValidFields()
        {
            yield return new object[] { "Item_1", "Partition_1", "Container_1", null };
            yield return new object[] { "Item_1", "Partition_1", "Container_1", Enumerable.Range(0, 1536).Select(x => (float)x).ToArray() };
        }

        [Theory]
        [MemberData(nameof(GetInvalidFields))]
        public void Create_DocumentVector_FailsWithInvalidValues(string itemId, string partitionKey, string containerName, float[]? vector)
        {
            Assert.Throws<Exception>(() => CreateDocumentVector(itemId, partitionKey, containerName, vector));
        }

        [Theory]
        [MemberData(nameof(GetValidFields))]
        public void Create_DocumentVector_SucceedsWithValidValues(string itemId, string partitionKey, string containerName, float[]? vector)
        {
            //Act
            var exception = Record.Exception(() => CreateDocumentVector(itemId, partitionKey, containerName, vector));

            //Assert
            Assert.Null(exception);
        }

        [Fact]
        public void Constructor_ShouldInitializeProperties()
        {
            // Arrange
            string expectedItemId = "Item_1";
            string expectedPartitionKey = "Partition_1";
            string expectedContainerName = "Container_1";

            // Act
            var documentVector = CreateDocumentVector(expectedItemId, expectedPartitionKey, expectedContainerName, null);

            // Assert
            Assert.NotNull(documentVector.id);
            Assert.Equal(expectedItemId, documentVector.itemId);
            Assert.Equal(expectedPartitionKey, documentVector.partitionKey);
            Assert.Equal(expectedContainerName, documentVector.containerName);
            Assert.Null(documentVector.vector);
        }

        public DocumentVector CreateDocumentVector(string itemId, string partitionKey, string containerName, float[]? vector)
        {
            return new DocumentVector(itemId, partitionKey, containerName, vector);
        }
    }
}
