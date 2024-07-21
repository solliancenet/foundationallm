using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel;

namespace FoundationaLLM.SemanticKernel.Core.Connectors.AzureML.Models
{
    /// <summary>
    /// Represents the overridable settings for executing a prompt with AzureML.
    /// </summary>
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public class AzureMLPromptExecutionSettings : PromptExecutionSettings
    {
        private double? _temperature;
        private double? _topP;
        private int? _topK;
        private int? _maxNewTokens;

        /// <summary>
        /// Default max tokens for a text generation.
        /// </summary>
        public static int DefaultTextMaxTokens { get; } = 256;

        /// <summary>
        /// Temperature controls the randomness of the completion.
        /// The higher the temperature, the more random the completion.
        /// Range is 0.0 to 1.0.
        /// </summary>
        [JsonPropertyName("temperature")]
        public double? Temperature
        {
            get => this._temperature;
            set
            {
                this.ThrowIfFrozen();
                this._temperature = value;
            }
        }

        /// <summary>
        /// TopP controls the diversity of the completion.
        /// The higher the TopP, the more diverse the completion.
        /// </summary>
        [JsonPropertyName("top_p")]
        public double? TopP
        {
            get => this._topP;
            set
            {
                this.ThrowIfFrozen();
                this._topP = value;
            }
        }

        /// <summary>
        /// Gets or sets the value of the TopK property.
        /// The TopK property represents the maximum value of a collection or dataset.
        /// </summary>
        [JsonPropertyName("top_k")]
        public int? TopK
        {
            get => this._topK;
            set
            {
                this.ThrowIfFrozen();
                this._topK = value;
            }
        }

        /// <summary>
        /// The maximum number of tokens to generate in the completion.
        /// </summary>
        [JsonPropertyName("max_new_tokens")]
        public int? MaxNewTokens
        {
            get => this._maxNewTokens;
            set
            {
                this.ThrowIfFrozen();
                this._maxNewTokens = value;
            }
        }

        /// <inheritdoc />
        public override PromptExecutionSettings Clone() => new AzureMLPromptExecutionSettings()
        {
            ModelId = this.ModelId,
            ExtensionData = this.ExtensionData is not null ? new Dictionary<string, object>(this.ExtensionData) : null,
            Temperature = this.Temperature,
            TopP = this.TopP,
            TopK = this.TopK,
            MaxNewTokens = this.MaxNewTokens,
        };

        /// <summary>
        /// Converts a <see cref="PromptExecutionSettings"/> object to a <see cref="AzureMLPromptExecutionSettings"/> object.
        /// </summary>
        /// <param name="executionSettings">The <see cref="PromptExecutionSettings"/> object to convert.</param>
        /// <returns>
        /// The converted <see cref="AzureMLPromptExecutionSettings"/> object. If <paramref name="executionSettings"/> is null,
        /// a new instance of <see cref="AzureMLPromptExecutionSettings"/> is returned. If <paramref name="executionSettings"/>
        /// is already a <see cref="AzureMLPromptExecutionSettings"/> object, it is casted and returned. Otherwise, the method
        /// tries to deserialize <paramref name="executionSettings"/> to a <see cref="AzureMLPromptExecutionSettings"/> object.
        /// If deserialization is successful, the converted object is returned. If deserialization fails or the converted object
        /// is null, an <see cref="ArgumentException"/> is thrown.
        /// </returns>
        public static AzureMLPromptExecutionSettings FromExecutionSettings(PromptExecutionSettings? executionSettings)
        {
            switch (executionSettings)
            {
                case null:
                    return new AzureMLPromptExecutionSettings() { MaxNewTokens = DefaultTextMaxTokens };
                case AzureMLPromptExecutionSettings settings:
                    return settings;
            }

            var json = JsonSerializer.Serialize(executionSettings);
            return JsonSerializer.Deserialize<AzureMLPromptExecutionSettings>(json)!;
        }

    }
}
