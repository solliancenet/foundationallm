using Azure.Search.Documents.Indexes;
using FoundationaLLM.Common.Models.TextEmbedding;

namespace FoundationaLLM.Common.Models.Search
{
    /// <summary>
    /// The customer object.
    /// </summary>
    public class Customer : EmbeddedEntity
    {
        /// <summary>
        /// The unique identifier.
        /// </summary>
        [SearchableField(IsKey = true, IsFilterable = true)]
        public string id { get; set; }
        /// <summary>
        /// The customer type.
        /// </summary>
        [SimpleField]
        [EmbeddingField(Label = "Customer type")]
        public string type { get; set; }
        /// <summary>
        /// The customer id.
        /// </summary>
        [SimpleField]
        public string customerId { get; set; }
        /// <summary>
        /// The customer title.
        /// </summary>
        [SimpleField]
        [EmbeddingField(Label = "Customer title")]
        public string title { get; set; }
        /// <summary>
        /// The customer first name.
        /// </summary>
        [SimpleField]
        [EmbeddingField(Label = "Customer first name")]
        public string firstName { get; set; }
        /// <summary>
        /// The customer last name.
        /// </summary>
        [SimpleField]
        [EmbeddingField(Label = "Customer last name")]
        public string lastName { get; set; }
        /// <summary>
        /// The email of the customer.
        /// </summary>
        [SimpleField]
        [EmbeddingField(Label = "Customer email address")]
        public string emailAddress { get; set; }
        /// <summary>
        /// The customer phone number.
        /// </summary>
        [SimpleField]
        [EmbeddingField(Label = "Customer phone number")]
        public string phoneNumber { get; set; }
        /// <summary>
        /// The creation of the customer.
        /// </summary>
        [SimpleField]
        public string creationDate { get; set; }
        /// <summary>
        /// The customer address of the customer.
        /// </summary>
        [SimpleField]
        [EmbeddingField(Label = "Customer addresses")]
        public List<CustomerAddress> addresses { get; set; }
        /// <summary>
        /// The customer password in hash and salt.
        /// </summary>
        [SimpleField(IsHidden = true)]
        public Password password { get; set; }
        /// <summary>
        /// The count of the sales order.
        /// </summary>
        [SimpleField]
        public double salesOrderCount { get; set; }

        /// <summary>
        /// Constructor for Customer.
        /// </summary>
        public Customer(string id, string type, string customerId, string title,
            string firstName, string lastName, string emailAddress, string phoneNumber,
            string creationDate, List<CustomerAddress> addresses, Password password,
            double salesOrderCount, float[]? vector = null)
        {
            this.id = id;
            this.type = type;
            this.customerId = customerId;
            this.title = title;
            this.firstName = firstName;
            this.lastName = lastName;
            this.emailAddress = emailAddress;
            this.phoneNumber = phoneNumber;
            this.creationDate = creationDate;
            this.addresses = addresses;
            this.password = password;
            this.salesOrderCount = salesOrderCount;
            this.vector = vector;
        }
    }

    /// <summary>
    /// The password object.
    /// </summary>
    public class Password
    {
        /// <summary>
        /// The hashed representation of the password.
        /// </summary>
        [SimpleField(IsHidden = true)]
        public string hash { get; set; }
        /// <summary>
        /// The salt value used during password hashing.
        /// </summary>
        [SimpleField(IsHidden = true)]
        public string salt { get; set; }

        /// <summary>
        /// Constructor for Password.
        /// </summary>
        public Password(string hash, string salt)
        {
            this.hash = hash;
            this.salt = salt;
        }
    }

    /// <summary>
    /// The customer address object.
    /// </summary>
    public class CustomerAddress
    {
        /// <summary>
        /// First line from customer address.
        /// </summary>
        [SimpleField]
        [EmbeddingField(Label = "Customer address line 1")]
        public string addressLine1 { get; set; }
        /// <summary>
        /// Second line from customer address.
        /// </summary>
        [SimpleField]
        [EmbeddingField(Label = "Customer address line 2")]
        public string addressLine2 { get; set; }
        /// <summary>
        /// The customer city.
        /// </summary>
        [SimpleField]
        [EmbeddingField(Label = "Customer address city")]
        public string city { get; set; }
        /// <summary>
        /// The customer state.
        /// </summary>
        [SimpleField]
        [EmbeddingField(Label = "Customer address state")]
        public string state { get; set; }
        /// <summary>
        /// The customer country.
        /// </summary>
        [SimpleField]
        [EmbeddingField(Label = "Customer address country")]
        public string country { get; set; }
        /// <summary>
        /// The customer zip code.
        /// </summary>
        [SimpleField]
        [EmbeddingField(Label = "Customer address zip code")]
        public string zipCode { get; set; }
        /// <summary>
        /// The location of the customer.
        /// </summary>
        [SimpleField]
        public Location location { get; set; }

        /// <summary>
        /// Constructor for Customer Address.
        /// </summary>
        public CustomerAddress(string addressLine1, string addressLine2, string city, string state, string country, string zipCode, Location location)
        {
            this.addressLine1 = addressLine1;
            this.addressLine2 = addressLine2;
            this.city = city;
            this.state = state;
            this.country = country;
            this.zipCode = zipCode;
            this.location = location;
        }
    }

    /// <summary>
    /// The location object.
    /// </summary>
    public class Location
    {
        /// <summary>
        /// The type of the location.
        /// </summary>
        [SimpleField]
        public string type { get; set; }
        /// <summary>
        /// The coordinates of the location.
        /// </summary>
        [FieldBuilderIgnore]
        public List<float> coordinates { get; set; }

        /// <summary>
        /// Constructor for Location.
        /// </summary>
        public Location(string type, List<float> coordinates)
        {
            this.type = type;
            this.coordinates = coordinates;
        }
    }
}
