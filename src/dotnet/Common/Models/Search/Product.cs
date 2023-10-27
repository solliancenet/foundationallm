using Azure.Search.Documents.Indexes;
using FoundationaLLM.Common.Models.TextEmbedding;

namespace FoundationaLLM.Common.Models.Search
{
    /// <summary>
    /// The product object.
    /// </summary>
    public class Product : EmbeddedEntity
    {
        /// <summary>
        /// The unique identifier.
        /// </summary>
        [SearchableField(IsKey = true, IsFilterable = true)]
        public string id { get; set; }
        /// <summary>
        /// The categoryid of the product.
        /// </summary>
        [SimpleField]
        public string categoryId { get; set; }
        /// <summary>
        /// The category name of the product.
        /// </summary>
        [SearchableField(IsFilterable = true, IsFacetable = true)]
        [EmbeddingField(Label = "Product category name")]
        public string categoryName { get; set; }
        /// <summary>
        /// The product stock keeping unit.
        /// </summary>
        [SimpleField]
        [EmbeddingField(Label = "Product stock keeping unit (SKU)")]
        public string sku { get; set; }
        /// <summary>
        /// The product name.
        /// </summary>
        [SimpleField]
        [EmbeddingField(Label = "Product name")]
        public string name { get; set; }
        /// <summary>
        /// The product description.
        /// </summary>
        [SimpleField]
        [EmbeddingField(Label = "Product description")]
        public string description { get; set; }
        /// <summary>
        /// The price of the product.
        /// </summary>
        [SimpleField]
        [EmbeddingField(Label = "Product price")]
        public double price { get; set; }
        /// <summary>
        /// The product tags.
        /// </summary>
        [SimpleField]
        [EmbeddingField(Label = "Product tags")]
        public List<Tag> tags { get; set; }

        /// <summary>
        /// Constructor for Product.
        /// </summary>
        public Product(string id, string categoryId, string categoryName, string sku, string name, string description, double price, List<Tag> tags, float[]? vector = null)
        {
            this.id = id;
            this.categoryId = categoryId;
            this.categoryName = categoryName;
            this.sku = sku;
            this.name = name;
            this.description = description;
            this.price = price;
            this.tags = tags;
            this.vector = vector;
        }
    }

    /// <summary>
    /// The product category object.
    /// </summary>
    public class ProductCategory
    {
        /// <summary>
        /// The unique identifier.
        /// </summary>
        [SimpleField]
        public string id { get; set; }
        /// <summary>
        /// The product category type.
        /// </summary>
        [SimpleField]
        public string type { get; set; }
        /// <summary>
        /// The product category name.
        /// </summary>
        [SimpleField]
        public string name { get; set; }

        /// <summary>
        /// Constructor for Product Category.
        /// </summary>
        public ProductCategory(string id, string type, string name)
        {
            this.id = id;
            this.type = type;
            this.name = name;
        }
    }

    /// <summary>
    /// The tag object.
    /// </summary>
    public class Tag
    {
        /// <summary>
        /// The unique identifier.
        /// </summary>
        [SimpleField]
        public string id { get; set; }
        /// <summary>
        /// The product tag name.
        /// </summary>
        [SimpleField]
        [EmbeddingField(Label = "Product tag name")]
        public string name { get; set; }

        /// <summary>
        /// Constructor for Tag.
        /// </summary>
        public Tag(string id, string name)
        {
            this.id = id;
            this.name = name;
        }
    }
}
