using FoundationaLLM.Common.Models.Orchestration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Tests.Models.Orchestration
{
    public class CompletionResponseTests
    {
        public static IEnumerable<object[]> GetInvalidFields()
        {
            yield return new object[] { null, "Prompt_1", 100, 100, null };
            yield return new object[] { "Completion_1",null, 100, 100, null };
            yield return new object[] { "Completion_1", "Prompt_1", null, 100, null };
            yield return new object[] { "Completion_1", "Prompt_1", 100, null, null };
            yield return new object[] { "Completion_1", "Prompt_1", 100, 100, null };
            yield return new object[] { "Completion_1", "Prompt_1", 100, 100, new float[0] };
            yield return new object[] { "Completion_1", "Prompt_1", 100, 100, new float[] { 1, 2, 3 } };
        }

        public static IEnumerable<object[]> GetValidFields()
        {
            yield return new object[] { "Completion_1", "Prompt_1", 100, 100, null };
            yield return new object[] { "Completion_2", "Prompt_2", 100, 100, Enumerable.Range(0, 1536).Select(x => (float)x).ToArray() };
        }

        [Theory]
        [MemberData(nameof(GetInvalidFields))]
        public void Create_CompletionResponse_FailsWithInvalidValues(string completion, string userPrompt, int userPromptTokens, int responseTokens, float[]? userPromptEmbedding)
        {
            Assert.Throws<Exception>(() => CreateCompletionResponse(completion, userPrompt, userPromptTokens, responseTokens, userPromptEmbedding));
        }

        [Theory]
        [MemberData(nameof(GetValidFields))]
        public void Create_CompletionResponse_SucceedsWithValidValues(string completion, string userPrompt, int userPromptTokens, int responseTokens, float[]? userPromptEmbedding)
        {
            //Act
            var exception = Record.Exception(() => CreateCompletionResponse(completion, userPrompt, userPromptTokens, responseTokens, userPromptEmbedding));

            //Assert
            Assert.Null(exception);
        }

        [Fact]
        public void Constructor_ShouldInitializeProperties()
        {
            // Arrange
            string expectedCompletion = "Completion_1";
            string expectedUserPrompt = "Prompt_1";
            int expectedUserPromptTokens = 5;
            int expectedResponseTokens = 10;
            float[] expectedUserPromptEmbedding = new float[] { 1,2,3 };

            // Act
            var completionResponse = CreateCompletionResponse(
                expectedCompletion,
                expectedUserPrompt,
                expectedUserPromptTokens,
                expectedResponseTokens,
                expectedUserPromptEmbedding
            );

            // Assert
            Assert.Equal(expectedCompletion, completionResponse.Completion);
            Assert.Equal(expectedUserPrompt, completionResponse.UserPrompt);
            Assert.Equal(expectedUserPromptTokens, completionResponse.UserPromptTokens);
            Assert.Equal(expectedResponseTokens, completionResponse.ResponseTokens);
            Assert.Equal(expectedUserPromptEmbedding, completionResponse.UserPromptEmbedding);
        }

        public CompletionResponse CreateCompletionResponse(string completion, string userPrompt, int userPromptTokens, int responseTokens,float[]? userPromptEmbedding)
        {
            return new CompletionResponse(completion, userPrompt, userPromptTokens, responseTokens, userPromptEmbedding);
        }
    }
}
