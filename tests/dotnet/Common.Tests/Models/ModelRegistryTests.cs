using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoundationaLLM.Common.Models;
using FoundationaLLM.Common.Models.Search;
using NSubstitute;

namespace FoundationaLLM.Common.Tests.Models
{
    public class ModelRegistryTests
    {
        [Fact]
        public void IdentifyType_ReturnsCorrectModelType()
        {
            var customer = new Customer(
                "id",
                "type",
                "customerId",
                "title",
                "firstName",
                "lastName",
                "emailAddress",
                "phoneNumber",
                "creationDate",
                new List<CustomerAddress>(),
                new Password("", ""),
                0.0
            );
            var customerObj = JObject.FromObject(customer);
            var expectedEntry = ModelRegistry.Models[nameof(Customer)];

            var actualEntry = ModelRegistry.IdentifyType(customerObj);

            Assert.Equal(expectedEntry, actualEntry);
        }
    }
}
