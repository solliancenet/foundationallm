using Castle.Core.Resource;
using FoundationaLLM.Common.Models.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Tests.Models.Search
{
    public class SalesOrderTests
    {
        [Fact]
        public void Constructor_SalesOrder_ShouldInitializeProperties()
        {
            // Arrange
            string expectedId = "Id_1";
            string expectedType = "Type_1";
            string expectedCustomerId = "Customer_1";
            string expectedOrderDate = "OrderDate_1";
            string expectedShipDate = "ShipDate_1";
            List<SalesOrderDetails> expectedDetails = new List<SalesOrderDetails>
            {
                new SalesOrderDetails("SKU_1","Name_1", 9.99, 10),
                new SalesOrderDetails("SKU_2","Name_2", 19.99, 20)
            };
            float[] expectedVector = new float[] { 1,2,3, 0.3f };

            // Act
            var salesOrder = CreateSalesOrder(
                expectedId,
                expectedType,
                expectedCustomerId,
                expectedOrderDate,
                expectedShipDate,
                expectedDetails,
                expectedVector
            );

            // Assert
            Assert.Equal(expectedId, salesOrder.id);
            Assert.Equal(expectedType, salesOrder.type);
            Assert.Equal(expectedCustomerId, salesOrder.customerId);
            Assert.Equal(expectedOrderDate, salesOrder.orderDate);
            Assert.Equal(expectedShipDate, salesOrder.shipDate);
            Assert.Equal(expectedDetails, salesOrder.details);
            Assert.Equal(expectedVector, salesOrder.vector);
        }

        public static IEnumerable<object[]> GetInvalidFieldsSalesOrder()
        {
            yield return new object[] { null, "Type_1", "Customer_1", "OrderDate_1", "ShipDate_1", new List<SalesOrderDetails>(), null };
            yield return new object[] { "Id_1",null, "Customer_1", "OrderDate_1", "ShipDate_1", new List<SalesOrderDetails>(), null };
            yield return new object[] { "Id_1", "Type_1",null, "OrderDate_1", "ShipDate_1", new List<SalesOrderDetails>(), null };
            yield return new object[] { "Id_1", "Type_1", "Customer_1", null, "ShipDate_1", new List<SalesOrderDetails>(), null };
            yield return new object[] { "Id_1", "Type_1", "Customer_1", "OrderDate_1", null, new List<SalesOrderDetails>(), null };
            yield return new object[] { "Id_1", "Type_1", "Customer_1", "OrderDate_1", "ShipDate_1", null, null };
            yield return new object[] { "Id_1", "Type_1", "Customer_1", "OrderDate_1", "ShipDate_1", new List<SalesOrderDetails>(), null };
            yield return new object[] { "Id_1", "Type_1", "Customer_1", "OrderDate_1", "ShipDate_1", new List<SalesOrderDetails>(), new float[0] };
            yield return new object[] { "Id_1", "Type_1", "Customer_1", "OrderDate_1", "ShipDate_1", new List<SalesOrderDetails>(), new float[] { 1, 2, 3 } };
        }

        public static IEnumerable<object[]> GetValidFieldsSalesOrder()
        {
            yield return new object[] { "Id_1", "Type_1", "Customer_1", "OrderDate_1", "ShipDate_1", new List<SalesOrderDetails>(), null };
            yield return new object[] { "Id_1", "Type_1", "Customer_1", "OrderDate_1", "ShipDate_1", new List<SalesOrderDetails>() , Enumerable.Range(0, 1536).Select(x => (float)x).ToArray() };
        }

        [Theory]
        [MemberData(nameof(GetInvalidFieldsSalesOrder))]
        public void Create_SalesOrder_FailsWithInvalidValues(string id, string type, string customerId, string orderDate, string shipDate, List<SalesOrderDetails> details, float[]? vector)
        {
            Assert.Throws<Exception>(() => CreateSalesOrder(id, type, customerId, orderDate, shipDate, details, vector));
        }

        [Theory]
        [MemberData(nameof(GetValidFieldsSalesOrder))]
        public void Create_SalesOrder_SucceedsWithValidValues(string id, string type, string customerId, string orderDate, string shipDate, List<SalesOrderDetails> details, float[]? vector)
        {
            //Act
            var exception = Record.Exception(() => CreateSalesOrder(id, type, customerId, orderDate, shipDate, details, vector));

            //Assert
            Assert.Null(exception);
        }

        public SalesOrder CreateSalesOrder(string id, string type, string customerId, string orderDate, string shipDate, List<SalesOrderDetails> details, float[]? vector)
        {
            return new SalesOrder(id, type, customerId, orderDate, shipDate, details, vector);
        }

        [Fact]
        public void Constructor_SalesOrderDetails_ShouldInitializeProperties()
        {
            // Arrange
            string expectedSku = "SKU_1";
            string expectedName = "Name_1";
            double expectedPrice = 99.99;
            double expectedQuantity = 2.0;

            // Act
            var salesOrderDetails = new SalesOrderDetails(
                expectedSku,
                expectedName,
                expectedPrice,
                expectedQuantity
            );

            // Assert
            Assert.Equal(expectedSku, salesOrderDetails.sku);
            Assert.Equal(expectedName, salesOrderDetails.name);
            Assert.Equal(expectedPrice, salesOrderDetails.price);
            Assert.Equal(expectedQuantity, salesOrderDetails.quantity);
        }

        public static IEnumerable<object[]> GetInvalidFieldsSalesOrderDetails()
        {
            yield return new object[] { null, "Name_1", 9.99, 10 };
            yield return new object[] { "SKU_1", null, 9.99, 10 };
            yield return new object[] { "SKU_1", "Name_1",null, 10 };
            yield return new object[] { "SKU_1", "Name_1", 9.99,null };
        }

        public static IEnumerable<object[]> GetValidFieldsSalesOrderDetails()
        {
            yield return new object[] { "SKU_1", "Name_1", 9.99 , 10 };
        }

        [Theory]
        [MemberData(nameof(GetInvalidFieldsSalesOrderDetails))]
        public void Create_SalesOrderDetails_FailsWithInvalidValues(string sku, string name, double price, double quantity)
        {
            Assert.Throws<Exception>(() => CreateSalesOrderDetails(sku, name, price, quantity));
        }

        [Theory]
        [MemberData(nameof(GetValidFieldsSalesOrderDetails))]
        public void Create_SalesOrderDetails_SucceedsWithValidValues(string sku, string name, double price, double quantity)
        {
            //Act
            var exception = Record.Exception(() => CreateSalesOrderDetails(sku, name, price, quantity));

            //Assert
            Assert.Null(exception);
        }

        public SalesOrderDetails CreateSalesOrderDetails(string sku, string name, double price, double quantity)
        {
            return new SalesOrderDetails(sku, name, price, quantity);
        }
    }
}
