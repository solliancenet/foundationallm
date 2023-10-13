using FoundationaLLM.Common.Models.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Tests.Models.Search
{
    public class CustomerTests
    {
        [Fact]
        public void Constructor_Customer_ShouldInitializeProperties()
        {
            // Arrange
            string expectedId = "Id_1";
            string expectedType = "Type_1";
            string expectedCustomerId = "CustId_1";
            string expectedTitle = "Title_1";
            string expectedFirstName = "FirstName_1";
            string expectedLastName = "LastName_1";
            string expectedEmailAddress = "Email_1";
            string expectedPhoneNumber = "Phone_1";
            string expectedCreationDate = "Date_1";
            List<CustomerAddress> expectedAddresses = new List<CustomerAddress>
        {
            new CustomerAddress("Address_Line_1", "Address_Line_2", "City_1", "State_1", "Country_1", "Zipcode_1", new Location("Type_1",new List<float>{1,2,3 }))
        };
            Password expectedPassword = new Password("hashed_password", "random_salt");
            double expectedSalesOrderCount = 5.0;
            float[] expectedVector = new float[] {1, 2, 3 };

            // Act
            var customer = CreateCustomer(
                expectedId,
                expectedType,
                expectedCustomerId,
                expectedTitle,
                expectedFirstName,
                expectedLastName,
                expectedEmailAddress,
                expectedPhoneNumber,
                expectedCreationDate,
                expectedAddresses,
                expectedPassword,
                expectedSalesOrderCount,
                expectedVector
            );

            // Assert
            Assert.Equal(expectedId, customer.id);
            Assert.Equal(expectedType, customer.type);
            Assert.Equal(expectedCustomerId, customer.customerId);
            Assert.Equal(expectedTitle, customer.title);
            Assert.Equal(expectedFirstName, customer.firstName);
            Assert.Equal(expectedLastName, customer.lastName);
            Assert.Equal(expectedEmailAddress, customer.emailAddress);
            Assert.Equal(expectedPhoneNumber, customer.phoneNumber);
            Assert.Equal(expectedCreationDate, customer.creationDate);
            Assert.Equal(expectedAddresses, customer.addresses);
            Assert.Equal(expectedPassword, customer.password);
            Assert.Equal(expectedSalesOrderCount, customer.salesOrderCount);
            Assert.Equal(expectedVector, customer.vector);
        }

        public static IEnumerable<object[]> GetInvalidFieldsCustomer()
        {
            yield return new object[] { null, "Type_1", "Customer_1", "Title_1", "FirstName_1", "LastName_1", "Email_1", "Numb_1", "CrDate_1", new List<CustomerAddress>(), new Password("Hash_1", "Salt_1"), 10, null };
            yield return new object[] { "Id_1", null, "Customer_1", "Title_1", "FirstName_1", "LastName_1", "Email_1", "Numb_1", "CrDate_1", new List<CustomerAddress>(), new Password("Hash_1", "Salt_1"), 10, null };
            yield return new object[] { "Id_1", "Type_1", null, "Title_1", "FirstName_1", "LastName_1", "Email_1", "Numb_1", "CrDate_1", new List<CustomerAddress>(), new Password("Hash_1", "Salt_1"), 10, null };
            yield return new object[] { "Id_1", "Type_1", "Customer_1", null, "FirstName_1", "LastName_1", "Email_1", "Numb_1", "CrDate_1", new List<CustomerAddress>(), new Password("Hash_1", "Salt_1"), 10, null };
            yield return new object[] { "Id_1", "Type_1", "Customer_1", "Title_1", null, "LastName_1", "Email_1", "Numb_1", "CrDate_1", new List<CustomerAddress>(), new Password("Hash_1", "Salt_1"), 10, null };
            yield return new object[] { "Id_1", "Type_1", "Customer_1", "Title_1", "FirstName_1", null, "Email_1", "Numb_1", "CrDate_1", new List<CustomerAddress>(), new Password("Hash_1", "Salt_1"), 10, null };
            yield return new object[] { "Id_1", "Type_1", "Customer_1", "Title_1", "FirstName_1", "LastName_1", null, "Numb_1", "CrDate_1", new List<CustomerAddress>(), new Password("Hash_1", "Salt_1"), 10, null };
            yield return new object[] { "Id_1", "Type_1", "Customer_1", "Title_1", "FirstName_1", "LastName_1", "Email_1", null, "CrDate_1", new List<CustomerAddress>(), new Password("Hash_1", "Salt_1"), 10, null };
            yield return new object[] { "Id_1", "Type_1", "Customer_1", "Title_1", "FirstName_1", "LastName_1", "Email_1", "Numb_1", null, new List<CustomerAddress>(), new Password("Hash_1", "Salt_1"), 10, null };
            yield return new object[] { "Id_1", "Type_1", "Customer_1", "Title_1", "FirstName_1", "LastName_1", "Email_1", "Numb_1", "CrDate_1", new List<CustomerAddress>(), new Password("Hash_1", "Salt_1"), 10, null };
            yield return new object[] { "Id_1", "Type_1", "Customer_1", "Title_1", "FirstName_1", "LastName_1", "Email_1", "Numb_1", "CrDate_1", new List<CustomerAddress>(), new Password("Hash_1", "Salt_1"), null, null };
            yield return new object[] { "Id_1", "Type_1", "Customer_1", "Title_1", "FirstName_1", "LastName_1", "Email_1", "Numb_1", "CrDate_1", new List<CustomerAddress>(), new Password("Hash_1", "Salt_1"), 10, new float[0] };
            yield return new object[] { "Id_1", "Type_1", "Customer_1", "Title_1", "FirstName_1", "LastName_1", "Email_1", "Numb_1", "CrDate_1", new List<CustomerAddress>(), new Password("Hash_1", "Salt_1"), 10, new float[] { 1, 2, 3 } };

        }

        public static IEnumerable<object[]> GetValidFieldsCustomer()
        {
            yield return new object[] { "Id_1", "Type_1", "Customer_1", "Title_1", "FirstName_1", "LastName_1", "Email_1", "Numb_1", "CrDate_1", new List<CustomerAddress>(), new Password("Hash_1", "Salt_1"), 10, null };
            yield return new object[] { "Id_2", "Type_2", "Customer_2", "Title_2", "FirstName_2", "LastName_2", "Email_2", "Numb_2", "CrDate_2", new List<CustomerAddress>(), new Password("Hash_2", "Salt_2"), 10, Enumerable.Range(0, 1536).Select(x => (float)x).ToArray() };
        }

        [Theory]
        [MemberData(nameof(GetInvalidFieldsCustomer))]
        public void Create_Customer_FailsWithInvalidValues(string id, string type, string customerId, string title,
            string firstName, string lastName, string emailAddress, string phoneNumber,
            string creationDate, List<CustomerAddress> addresses, Password password,
            double salesOrderCount, float[]? vector)
        {
            Assert.Throws<Exception>(() => CreateCustomer(id, type, customerId, title, firstName, lastName, emailAddress, phoneNumber, creationDate, addresses, password, salesOrderCount, vector));
        }

        [Theory]
        [MemberData(nameof(GetValidFieldsCustomer))]
        public void Create_Customer_SucceedsWithValidValues(string id, string type, string customerId, string title,
            string firstName, string lastName, string emailAddress, string phoneNumber,
            string creationDate, List<CustomerAddress> addresses, Password password,
            double salesOrderCount, float[]? vector)
        {
            //Act
            var exception = Record.Exception(() => CreateCustomer(id, type, customerId, title, firstName, lastName, emailAddress, phoneNumber, creationDate, addresses, password, salesOrderCount, vector));

            //Assert
            Assert.Null(exception);
        }

        public Customer CreateCustomer(string id, string type, string customerId, string title,
            string firstName, string lastName, string emailAddress, string phoneNumber,
            string creationDate, List<CustomerAddress> addresses, Password password,
            double salesOrderCount, float[]? vector)
        {
            return new Customer(id, type, customerId, title, firstName, lastName, emailAddress, phoneNumber, creationDate, addresses, password, salesOrderCount, vector);
        }

        [Fact]
        public void Constructor_Password_ShouldInitializeProperties()
        {
            // Arrange
            string expectedHash = "hashed_password";
            string expectedSalt = "random_salt";

            // Act
            var password = CreatePassword(expectedHash, expectedSalt);

            // Assert
            Assert.Equal(expectedHash, password.hash);
            Assert.Equal(expectedSalt, password.salt);
        }

        public static IEnumerable<object[]> GetInvalidFieldsPassword()
        {
            yield return new object[] { null, "Salt_1" };
            yield return new object[] { "Hash_1", null };
        }

        public static IEnumerable<object[]> GetValidFieldsPassword()
        {
            yield return new object[] { "Hash_1", "Salt_1" };
        }

        [Theory]
        [MemberData(nameof(GetInvalidFieldsPassword))]
        public void Create_Password_FailsWithInvalidValues(string hash, string salt)
        {
            Assert.Throws<Exception>(() => CreatePassword(hash, salt));
        }

        [Theory]
        [MemberData(nameof(GetValidFieldsPassword))]
        public void Create_Password_SucceedsWithValidValues(string hash, string salt)
        {
            //Act
            var exception = Record.Exception(() => CreatePassword(hash, salt));

            //Assert
            Assert.Null(exception);
        }

        public Password CreatePassword(string hash, string salt)
        {
            return new Password(hash, salt);
        }

        [Fact]
        public void Constructor_CustomerAddress_ShouldInitializeProperties()
        {
            // Arrange
            string expectedAddressLine1 = "Address_Line_1";
            string expectedAddressLine2 = "Address_Line_2";
            string expectedCity = "City_1";
            string expectedState = "State_1";
            string expectedCountry = "Country_1";
            string expectedZipCode = "Zipcode_1";
            Location expectedLocation = new Location("Type_1", new List<float> { 1,2,3 });

            // Act
            var customerAddress = CreateCustomerAddress(
                expectedAddressLine1,
                expectedAddressLine2,
                expectedCity,
                expectedState,
                expectedCountry,
                expectedZipCode,
                expectedLocation
            );

            // Assert
            Assert.Equal(expectedAddressLine1, customerAddress.addressLine1);
            Assert.Equal(expectedAddressLine2, customerAddress.addressLine2);
            Assert.Equal(expectedCity, customerAddress.city);
            Assert.Equal(expectedState, customerAddress.state);
            Assert.Equal(expectedCountry, customerAddress.country);
            Assert.Equal(expectedZipCode, customerAddress.zipCode);
            Assert.Equal(expectedLocation, customerAddress.location);
        }

        public static IEnumerable<object[]> GetInvalidFieldsCustomerAddress()
        {
            yield return new object[] { null, "AddrLine_2", "City_1", "State_1", "Country_1", "ZipCode_1", new Location("Type_1", new List<float> { 1,2,3 }) };
            yield return new object[] { "AddrLine_1", null, "City_1", "State_1", "Country_1", "ZipCode_1", new Location("Type_1", new List<float> { 1,2,3 }) };
            yield return new object[] { "AddrLine_1", "AddrLine_2", null, "State_1", "Country_1", "ZipCode_1", new Location("Type_1", new List<float> { 1,2,3 }) };
            yield return new object[] { "AddrLine_1", "AddrLine_2", "City_1", null, "Country_1", "ZipCode_1", new Location("Type_1", new List<float> { 1,2,3 }) };
            yield return new object[] { "AddrLine_1", "AddrLine_2", "City_1", "State_1", null, "ZipCode_1", new Location("Type_1", new List<float> { 1,2,3 }) };
            yield return new object[] { "AddrLine_1", "AddrLine_2", "City_1", "State_1", "Country_1", null, new Location("Type_1", new List<float> { 1,2,3 }) };
            yield return new object[] { "AddrLine_1", "AddrLine_2", "City_1", "State_1", "Country_1", "ZipCode_1", null };

        }

        public static IEnumerable<object[]> GetValidFieldsCustomerAddress()
        {
            yield return new object[] { "AddrLine_1", "AddrLine_2", "City_1", "State_1", "Country_1", "ZipCode_1", new Location("Type_1", new List<float> { 1,2,3 }) };
        }

        [Theory]
        [MemberData(nameof(GetInvalidFieldsCustomerAddress))]
        public void Create_CustomerAddress_FailsWithInvalidValues(string addressLine1, string addressLine2, string city, string state, string country, string zipCode, Location location)
        {
            Assert.Throws<Exception>(() => CreateCustomerAddress(addressLine1, addressLine2, city, state, country, zipCode, location));
        }

        [Theory]
        [MemberData(nameof(GetValidFieldsCustomerAddress))]
        public void Create_CustomerAddress_SucceedsWithValidValues(string addressLine1, string addressLine2, string city, string state, string country, string zipCode, Location location)
        {
            //Act
            var exception = Record.Exception(() => CreateCustomerAddress(addressLine1, addressLine2, city, state, country, zipCode, location));

            //Assert
            Assert.Null(exception);
        }

        public CustomerAddress CreateCustomerAddress(string addressLine1, string addressLine2, string city, string state, string country, string zipCode, Location location)
        {
            return new CustomerAddress(addressLine1, addressLine2, city, state, country, zipCode, location);
        }

        [Fact]
        public void Constructor_Location_ShouldInitializeProperties()
        {
            // Arrange
            string expectedType = "Type_1";
            List<float> expectedCoordinates = new List<float> { 1, 2, 3 };

            // Act
            var location = new Location(expectedType, expectedCoordinates);

            // Assert
            Assert.Equal(expectedType, location.type);
            Assert.Equal(expectedCoordinates, location.coordinates);
        }

        public static IEnumerable<object[]> GetInvalidFieldsLocation()
        {
            yield return new object[] { null, new List<float> { 1,2,3 } };
            yield return new object[] { "Type_1",null };
        }

        public static IEnumerable<object[]> GetValidFieldsLocation()
        {
            yield return new object[] { "Type_1", new List<float> { 1,2,3 } };
        }

        [Theory]
        [MemberData(nameof(GetInvalidFieldsLocation))]
        public void Create_Location_FailsWithInvalidValues(string type, List<float> coordinates)
        {
            Assert.Throws<Exception>(() => CreateLocation(type, coordinates));
        }

        [Theory]
        [MemberData(nameof(GetValidFieldsLocation))]
        public void Create_Location_SucceedsWithValidValues(string type, List<float> coordinates)
        {
            //Act
            var exception = Record.Exception(() => CreateLocation(type, coordinates));

            //Assert
            Assert.Null(exception);
        }

        public Location CreateLocation(string type, List<float> coordinates)
        {
            return new Location(type, coordinates);
        }
    }
}
