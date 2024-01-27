using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.TextEmbedding
{
    /// <summary>
    /// Stores a vector embedding.
    /// This type should be serialized using Emedding.JsonConverter.
    /// </summary>
    public struct Embedding : IEquatable<Embedding>
    {
        /// <summary>
        /// The vector that represents the embedding.
        /// This property is only serialized when Embedding.JsonConverter is used.
        /// </summary>
        [JsonIgnore]
        public ReadOnlyMemory<float> Vector { get; set; } = new();

        /// <summary>
        /// Length of the vector representing the embedding.
        /// This property is only serialized when Embedding.JsonConverter is used.
        /// </summary>
        [JsonIgnore]
        public readonly int Length => this.Vector.Length;

        /// <summary>
        /// Creates an embedding from a vector represented as an array of real numbers.
        /// </summary>
        /// <param name="vector">The array containing the vector values.</param>
        public Embedding(float[] vector) => this.Vector = vector;

        /// <summary>
        /// Creates an embedding from a vector represents as a <see cref="ReadOnlyMemory{T}"/> object.
        /// </summary>
        /// <param name="vector"></param>
        public Embedding(ReadOnlyMemory<float> vector) => this.Vector = vector;

        /// <summary>
        /// Creates an embedding with a zero-initialzed vector of a specified size.
        /// </summary>
        /// <param name="size">The size of the vector representing the embedding.</param>
        public Embedding(int size) => this.Vector = new ReadOnlyMemory<float>(new float[size]);

        /// <inheritdoc/>
        public readonly bool Equals(Embedding other) => this.Vector.Equals(other.Vector);

        /// <summary>
        /// Inidicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="obj">An object to compare with this object.</param>
        /// <returns>True if the object is equal to the obj param and False otherwise.</returns>
        public override readonly bool Equals(object? obj) => (obj is Embedding other && this.Equals(other));

        /// <summary>
        /// Checks if two <see cref="Embedding"/> values are equal.
        /// </summary>
        /// <param name="v1">The first <see cref="Embedding"/> value to be cheched.</param>
        /// <param name="v2">The second <see cref="Embedding"/> value to be checked.</param>
        /// <returns>True if the two values are equal, False otherwise.</returns>
        public static bool operator ==(Embedding v1, Embedding v2) => v1.Equals(v2);

        /// <summary>
        /// Checks if two <see cref="Embedding"/> values are different.
        /// </summary>
        /// <param name="v1">The first <see cref="Embedding"/> value to be cheched.</param>
        /// <param name="v2">The second <see cref="Embedding"/> value to be checked.</param>
        /// <returns>True if the two values are different, False otherwise.</returns>
        public static bool operator !=(Embedding v1, Embedding v2) => !(v1 == v2);

        /// <summary>
        /// Calculated the hashcode for this <see cref="Embedding"/>.
        /// </summary>
        /// <returns>The hash value represented by an integer.</returns>
        public override readonly int GetHashCode() => this.Vector.GetHashCode();

        /// <summary>
        /// Serializes the content of an <see cref="Embedding"/> value.
        /// Note: use Embedding.JsonConverter to serialize objects using
        /// the Embedding type, for example:
        ///     [JsonPropertyName("vector")]
        ///     [JsonConverter(typeof(Embedding.JsonConverter))]
        ///     public Embedding Vector { get; set; }
        /// </summary>
        public sealed class JsonConverter : JsonConverter<Embedding>
        {
            /// <summary>An instance of a converter for float[] that all operations delegate to</summary>
            private static readonly JsonConverter<float[]> Converter =
                (JsonConverter<float[]>)new JsonSerializerOptions().GetConverter(typeof(float[]));

            /// <inheritdoc/>
            public override Embedding Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
                new(Converter.Read(ref reader, typeof(float[]), options) ?? []);

            /// <inheritdoc/>
            public override void Write(Utf8JsonWriter writer, Embedding value, JsonSerializerOptions options) =>
                Converter.Write(writer, MemoryMarshal.TryGetArray(value.Vector, out ArraySegment<float> array) && array.Count == value.Length
                    ? array.Array!
                    : value.Vector.ToArray(), options);
        }
    }
}
