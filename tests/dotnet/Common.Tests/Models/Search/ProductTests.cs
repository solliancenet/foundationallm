using FoundationaLLM.Common.Models.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Tests.Models.Search
{
    public class ProductTests
    {
        [Fact]
        public void Constructor_Product_ShouldInitializeProperties()
        {
            // Arrange
            string expectedId = "Id_1";
            string expectedCategoryId = "Category_1";
            string expectedCategoryName = "CategoryName_1";
            string expectedSku = "SKU_1";
            string expectedName = "Name_1";
            string expectedDescription = "Desc_1";
            double expectedPrice = 599.99;
            List<Tag> expectedTags = new List<Tag> { new Tag("Id_1", "Name_1"), new Tag("Id_2", "Name_2") };
            float[] expectedVector = new float[] { 1,2,3, 0.3f };

            // Act
            var product = CreateProduct(
                expectedId,
                expectedCategoryId,
                expectedCategoryName,
                expectedSku,
                expectedName,
                expectedDescription,
                expectedPrice,
                expectedTags,
                expectedVector
            );

            // Assert
            Assert.Equal(expectedId, product.id);
            Assert.Equal(expectedCategoryId, product.categoryId);
            Assert.Equal(expectedCategoryName, product.categoryName);
            Assert.Equal(expectedSku, product.sku);
            Assert.Equal(expectedName, product.name);
            Assert.Equal(expectedDescription, product.description);
            Assert.Equal(expectedPrice, product.price);
            Assert.Equal(expectedTags, product.tags);
            Assert.Equal(expectedVector, product.vector);
        }

        [Fact]
        public void Constructor_ProductCategory_ShouldInitializeProperties()
        {
            // Arrange
            string expectedId = "Id_1";
            string expectedType = "Type_1";
            string expectedName = "Name_1";

            // Act
            var productCategory = CreateProductCategory(expectedId, expectedType, expectedName);

            // Assert
            Assert.Equal(expectedId, productCategory.id);
            Assert.Equal(expectedType, productCategory.type);
            Assert.Equal(expectedName, productCategory.name);
        }

        [Fact]
        public void Constructor_Tag_ShouldInitializeProperties()
        {
            // Arrange
            string expectedId = "Id_1";
            string expectedName = "Name_1";

            // Act
            var tag = CreateTag(expectedId, expectedName);

            // Assert
            Assert.Equal(expectedId, tag.id);
            Assert.Equal(expectedName, tag.name);
        }

        public static IEnumerable<object[]> GetInvalidFieldsProduct()
        {
            yield return new object[] { null, "Category_1", "CategoryName_1", "SKU_1", "Name_1", "Desc_1", 10.99, new List<Tag>(), null };
            yield return new object[] { "Id_1",null, "CategoryName_1", "SKU_1", "Name_1", "Desc_1", 10.99, new List<Tag>(), null };
            yield return new object[] { "Id_1", "Category_1",null, "SKU_1", "Name_1", "Desc_1", 10.99, new List<Tag>(), null };
            yield return new object[] { "Id_1", "Category_1", "CategoryName_1", null, "Name_1", "Desc_1", 10.99, new List<Tag>(), null };
            yield return new object[] { "Id_1", "Category_1", "CategoryName_1", "SKU_1", null, "Desc_1", 10.99, new List<Tag>(), null };
            yield return new object[] { "Id_1", "Category_1", "CategoryName_1", "SKU_1", "Name_1", null, 10.99, new List<Tag>(), null };
            yield return new object[] { "Id_1", "Category_1", "CategoryName_1", "SKU_1", "Name_1", "Desc_1", null, new List<Tag>(), null };
            yield return new object[] { "Id_1", "Category_1", "CategoryName_1", "SKU_1", "Name_1", "Desc_1", 10.99, null, null };
            yield return new object[] { "Id_1", "Category_1", "CategoryName_1", "SKU_1", "Name_1", "Desc_1", 10.99, new List<Tag>(), new float[0] };
            yield return new object[] { "Id_1", "Category_1", "CategoryName_1", "SKU_1", "Name_1", "Desc_1", 10.99, new List<Tag>(), new float[] { 1, 2, 3 } };
        }

        public static IEnumerable<object[]> GetValidFieldsProduct()
        {
            yield return new object[] { "Id_1", "Category_1", "CategoryName_1", "SKU_1", "Name_1", "Desc_1", 10.99, new List<Tag>(), null };
            yield return new object[] { "Id_1", "Category_1", "CategoryName_1", "SKU_1", "Name_1", "Desc_1", 10.99, new List<Tag>(), Enumerable.Range(0, 1536).Select(x => (float)x).ToArray() };
        }

        [Theory]
        [MemberData(nameof(GetInvalidFieldsProduct))]
        public void Create_Product_FailsWithInvalidValues(string id, string categoryId, string categoryName, string sku, string name, string description, double price, List<Tag> tags, float[]? vector)
        {
            Assert.Throws<Exception>(() => CreateProduct(id, categoryId, categoryName, sku, name, description, price, tags, vector));
        }

        [Theory]
        [MemberData(nameof(GetValidFieldsProduct))]
        public void Create_Product_SucceedsWithValidValues(string id, string categoryId, string categoryName, string sku, string name, string description, double price, List<Tag> tags, float[]? vector)
        {
            //Act
            var exception = Record.Exception(() => CreateProduct(id, categoryId, categoryName, sku, name, description, price, tags, vector));

            //Assert
            Assert.Null(exception);
        }

        public Product CreateProduct(string id, string categoryId, string categoryName, string sku, string name, string description, double price, List<Tag> tags, float[]? vector)
        {
            return new Product(id, categoryId, categoryName, sku,name,description,price,tags,vector);
        }

        public static IEnumerable<object[]> GetInvalidFieldsProductCategory()
        {
            yield return new object[] { null, "Type_1", "Name_1" };
            yield return new object[] { "Id_1",null, "Name_1" };
            yield return new object[] { "Id_1", "Type_1", null };
        }

        public static IEnumerable<object[]> GetValidFieldsProductCategory()
        {
            yield return new object[] { "Id_1", "Type_1", "Name_1" };
        }

        [Theory]
        [MemberData(nameof(GetInvalidFieldsProductCategory))]
        public void Create_ProductCategory_FailsWithInvalidValues(string id, string type, string name)
        {
            Assert.Throws<Exception>(() => CreateProductCategory(id, type, name));
        }

        [Theory]
        [MemberData(nameof(GetValidFieldsProductCategory))]
        public void Create_ProductCategory_SucceedsWithValidValues(string id, string type, string name)
        {
            //Act
            var exception = Record.Exception(() => CreateProductCategory(id, type, name));

            //Assert
            Assert.Null(exception);
        }

        public ProductCategory CreateProductCategory(string id, string type, string name)
        {
           return new ProductCategory(id, type, name);
        }

        public static IEnumerable<object[]> GetInvalidFieldsTag()
        {
            yield return new object[] { null, "Name_1" };
            yield return new object[] { "Id_1", null };
        }

        public static IEnumerable<object[]> GetValidFieldsTag()
        {
            yield return new object[] { "Id_1", "Name_1" };
        }

        [Theory]
        [MemberData(nameof(GetInvalidFieldsTag))]
        public void Create_Tag_FailsWithInvalidValues(string id, string name)
        {
            Assert.Throws<Exception>(() => CreateTag(id, name));
        }

        [Theory]
        [MemberData(nameof(GetValidFieldsTag))]
        public void Create_Tag_SucceedsWithValidValues(string id, string name)
        {
            //Act
            var exception = Record.Exception(() => CreateTag(id, name));

            //Assert
            Assert.Null(exception);
        }

        public Tag CreateTag(string id, string name)
        {
            return new Tag(id, name);
        }
    }
} 
