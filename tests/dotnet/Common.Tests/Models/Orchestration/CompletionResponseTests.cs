using FoundationaLLM.Common.Models.Orchestration;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Tests.Models.Orchestration
{
    public class CompletionResponseTests
    {
        public static IEnumerable<object?[]> GetInvalidFields()
        {
            yield return new object?[] { null, "Prompt_1", 100, 100, null };
            yield return new object?[] { "Completion_1", null, 100, 100, null };
            yield return new object?[] { "Completion_1", "Prompt_1", null, 100, null };
            yield return new object?[] { "Completion_1", "Prompt_1", 100, null, null };
            yield return new object?[] { "Completion_1", "Prompt_1", 100, 100, null };
            yield return new object?[] { "Completion_1", "Prompt_1", 100, 100, new float[0] };
            yield return new object?[] { "Completion_1", "Prompt_1", 100, 100, new float[] { 1, 2, 3 } };
        }

        public static IEnumerable<object?[]> GetValidFields()
        {
            yield return new object?[] { "Completion_1", "Prompt_1", 100, 100, null };
            yield return new object?[] { "Completion_2", "Prompt_2", 100, 100, Enumerable.Range(0, 1536).Select(x => (float)x).ToArray() };
        }

        //[Theory]
        //[MemberData(nameof(GetInvalidFields))]
        //public void Create_CompletionResponse_FailsWithInvalidValues(string completion, string userPrompt, int userPromptTokens, int responseTokens, float[]? userPromptEmbedding)
        //{
        //    Assert.Throws<Exception>(() => CreateCompletionResponse(completion, userPrompt, userPromptTokens, responseTokens, userPromptEmbedding));
        //}

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
            Assert.Equal(expectedUserPromptTokens, completionResponse.PromptTokens);
            Assert.Equal(expectedResponseTokens, completionResponse.CompletionTokens);
            Assert.Equal(expectedUserPromptEmbedding, completionResponse.UserPromptEmbedding);
        }

        [Fact]
        public void TestTotalTokensCalculation()
        {
            // Arrange
            var completionResponse = new CompletionResponse();
            completionResponse.PromptTokens = 5;
            completionResponse.CompletionTokens = 10;

            // Act
            var totalTokens = completionResponse.TotalTokens;

            // Assert
            Assert.Equal(15, totalTokens);
        }

        [Fact]
        public void TestDefaultCompletionResponseInitialization()
        {
            // Arrange
            var completionResponse = new CompletionResponse();

            // Act

            // Assert
            Assert.Equal(0, completionResponse.PromptTokens);
            Assert.Equal(0, completionResponse.CompletionTokens);
            Assert.Equal(0, completionResponse.TotalTokens);
            Assert.Equal(0.0f, completionResponse.TotalCost);
            Assert.Null(completionResponse.UserPromptEmbedding);
        }

        [Fact]
        public void OrchestrationRequest_Properties_Test()
        {
            // Arrange
            string expectedSessionId = "12345";
            string expectedUserPrompt = "Test user prompt";

            // Act
            var orchestrationRequest = new CompletionRequest
            {
                SessionId = expectedSessionId,
                UserPrompt = expectedUserPrompt
            };

            // Assert
            Assert.Equal(expectedSessionId, orchestrationRequest.SessionId);
            Assert.Equal(expectedUserPrompt, orchestrationRequest.UserPrompt);
        }

        public CompletionResponse CreateCompletionResponse(string completion, string userPrompt, int userPromptTokens, int responseTokens,float[]? userPromptEmbedding)
        {
            var completionResponse = new CompletionResponse(completion, userPrompt, userPromptTokens, responseTokens, userPromptEmbedding);
            return completionResponse;
        }
    }
}
