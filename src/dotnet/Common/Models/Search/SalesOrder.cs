using Azure.Search.Documents.Indexes;
using FoundationaLLM.Common.Models.TextEmbedding;

namespace FoundationaLLM.Common.Models.Search
{
    /// <summary>
    /// The sales order object.
    /// </summary>
    public class SalesOrder : EmbeddedEntity
    {
        /// <summary>
        /// The unique identifier.
        /// </summary>
        [SearchableField(IsKey = true, IsFilterable = true)]
        public string id { get; set; }
        /// <summary>
        /// The type of the customer sales order.
        /// </summary>
        [SimpleField]
        [EmbeddingField(Label = "Customer sales order type")]
        public string type { get; set; }
        /// <summary>
        /// The customer identifier associated with the sales order.
        /// </summary>
        [SimpleField]
        public string customerId { get; set; }
        /// <summary>
        /// The date of the sales order.
        /// </summary>
        [SimpleField]
        public string orderDate { get; set; }
        /// <summary>
        /// The date of the shipment associated with the sales order.
        /// </summary>
        [SimpleField]
        public string shipDate { get; set; }
        /// <summary>
        /// The details associated with the customer sales order.
        /// </summary>
        [SimpleField]
        [EmbeddingField(Label = "Customer sales order details")]
        public List<SalesOrderDetails> details { get; set; }

        /// <summary>
        /// Constructor for Sales Order.
        /// </summary>
        public SalesOrder(string id, string type, string customerId, string orderDate, string shipDate, List<SalesOrderDetails> details, float[]? vector = null)
        {
            this.id = id;
            this.type = type;
            this.customerId = customerId;
            this.orderDate = orderDate;
            this.shipDate = shipDate;
            this.details = details;
            this.vector = vector;
        }
    }

    /// <summary>
    /// The sales order details object
    /// </summary>
    public class SalesOrderDetails
    {
        /// <summary>
        /// The stock keeping unit (SKU) for the customer sales order detail
        /// </summary>
        [SimpleField]
        [EmbeddingField(Label = "Customer sales order detail stock keeping unit (SKU)")]
        public string sku { get; set; }
        /// <summary>
        /// The product name associated with the customer sales order detail
        /// </summary>
        [SimpleField]
        [EmbeddingField(Label = "Customer sales order detail product name")]
        public string name { get; set; }
        /// <summary>
        /// The price of the product associated with the customer sales order detail
        /// </summary>
        [SimpleField]
        [EmbeddingField(Label = "Customer sales order detail product price")]
        public double price { get; set; }
        /// <summary>
        /// The quantity of the product associated with the customer sales order detail
        /// </summary>
        [SimpleField]
        [EmbeddingField(Label = "Customer sales order detail product quantity")]
        public double quantity { get; set; }

        /// <summary>
        /// Constructor for Sales Order Details
        /// </summary>
        public SalesOrderDetails(string sku, string name, double price, double quantity)
        {
            this.sku = sku;
            this.name = name;
            this.price = price;
            this.quantity = quantity;
        }
    }
}
