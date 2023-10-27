using FoundationaLLM.Common.Models.Search;
using Newtonsoft.Json.Linq;

namespace FoundationaLLM.Common.Models
{
    /// <summary>
    /// The model registry object.
    /// </summary>
    public class ModelRegistry
    {
        /// <summary>
        /// Dictionary of model names and their corresponding entries in the registry.
        /// </summary>
        public static Dictionary<string, ModelRegistryEntry> Models = new Dictionary<string, ModelRegistryEntry>
            {
                { 
                    nameof(Customer), 
                    new ModelRegistryEntry 
                    { 
                        Type = typeof(Customer),
                        TypeMatchingProperties = new List<string> { "customerId", "firstName" },
                        NamingProperties = new List<string> { "firstName", "lastName" },
                    } 
                },
                { 
                    nameof(Product),
                    new ModelRegistryEntry 
                    { 
                        Type = typeof(Product),
                        TypeMatchingProperties = new List<string> { "sku" },
                        NamingProperties = new List<string> { "name" }
                    } 
                },
                { 
                    nameof(SalesOrder),  
                    new ModelRegistryEntry 
                    { 
                        Type = typeof(SalesOrder),
                        TypeMatchingProperties = new List<string> { "orderDate", "shipDate" },
                        NamingProperties = new List<string> { "id" }
                    } 
                },
                {
                    nameof(ShortTermMemory),
                    new ModelRegistryEntry
                    {
                        Type = typeof(ShortTermMemory),
                        TypeMatchingProperties = new List<string> { "memory__" },
                        NamingProperties = new List<string>()
                    }
                }
            };

        /// <summary>
        /// Identifies the type of the object based on its properties.
        /// </summary>
        public static ModelRegistryEntry? IdentifyType(JObject obj)
        {
            var objProps = obj.Properties().Select(p => p.Name);

            var result = ModelRegistry
                .Models
                .Select(m => m.Value)
                .SingleOrDefault(x => objProps.Intersect(x.TypeMatchingProperties!).Count() == x.TypeMatchingProperties!.Count());

            return result;
        }
    }
    /// <summary>
    /// The model registry entry object.
    /// </summary>
    public class ModelRegistryEntry
    {
        /// <summary>
        /// The Type associated with the model registry entry.
        /// </summary>
        public Type? Type { get; init; }
        /// <summary>
        /// The list of type-matching properties associated with the model registry entry.
        /// </summary>
        public List<string>? TypeMatchingProperties { get; init; }
        /// <summary>
        /// The list of naming properties associated with the model registry entry.
        /// </summary>
        public List<string>? NamingProperties { get; init; }
    }
}
