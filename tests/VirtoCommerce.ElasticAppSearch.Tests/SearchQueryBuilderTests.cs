using System.Collections.Generic;
using Moq;
using Newtonsoft.Json;
using VirtoCommerce.ElasticAppSearch.Core.Services;
using VirtoCommerce.ElasticAppSearch.Data.Services;
using VirtoCommerce.SearchModule.Core.Model;
using Xunit;

namespace VirtoCommerce.ElasticAppSearch.Tests
{
    public class SearchQueryBuilderTests
    {

        [Theory]
        [InlineData(new object[] { new[] { "test" }, "{\"test\":{}}" })]
        [InlineData(new object[] { new[] { "test1", "test2" }, "{\"test1\":{},\"test2\":{}}" })]
        public void TestSearchFields(string[] searchField, string result)
        {
            // Arrange
            var fieldNameConverterMock = new Mock<IFieldNameConverter>();
            fieldNameConverterMock.Setup(x => x.ToProviderFieldName(It.IsAny<string>())).Returns<string>(x => x);

            var searchQueryBuilder = new SearchQueryBuilder(fieldNameConverterMock.Object);

            var request = new SearchRequest
            {
                SearchFields = new List<string>(searchField),
            };

            // Act
            var searchQuery = searchQueryBuilder.ToSearchQuery(request);
            var serializedQuerySearchFields = JsonConvert.SerializeObject(searchQuery.SearchFields);

            // Assert
            Assert.Equal(result, serializedQuerySearchFields);
        }

        [Fact]
        public void NullSearchFields()
        {
            // Arrange
            var fieldNameConverterMock = new Mock<IFieldNameConverter>();

            var searchQueryBuilder = new SearchQueryBuilder(fieldNameConverterMock.Object);

            var request = new SearchRequest { SearchFields = null, };

            // Act

            var result = searchQueryBuilder.ToSearchQuery(request);

            // Assert

            Assert.NotNull(result);
            Assert.Null(result.SearchFields);
        }
    }
}
