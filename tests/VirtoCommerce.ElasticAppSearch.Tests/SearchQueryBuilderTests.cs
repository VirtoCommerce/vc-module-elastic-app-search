using System;
using System.Collections.Generic;
using Moq;
using VirtoCommerce.ElasticAppSearch.Core.Services.Builders;
using VirtoCommerce.ElasticAppSearch.Core.Services.Converters;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.ElasticAppSearch.Tests
{
    public class SearchQueryBuilderTests
    {
        public static IEnumerable<object[]> SortData => new[]
        {
            new object[] { null, string.Empty },
            new object[] { Array.Empty<SortingField>(), string.Empty },
            new object[] { new SortingField[] { new ("test") }, @"""sort"":{""test"":""asc""}," },
            new object[] { new SortingField[] { new ("test1"), new ("test2", true) }, @"""sort"":[{""test1"":""asc""},{""test2"":""desc""}]," }
        };

        //[Theory(Skip = "Temporary")]
        //[MemberData(nameof(SortData))]
        //public void TestSort(SortingField[] sortingFields, string expectedResult)
        //{
        //    // Arrange
        //    var fieldNameConverter = GetFieldNameConverter();
        //    var searchFiltersBuilder = GetSearchFiltersBuilder();
        //    var searchQueryBuilder = new SearchQueryBuilder(fieldNameConverter, searchFiltersBuilder);
        //    var request = new SearchRequest { Sorting = sortingFields };

        //    // Act
        //    var searchQuery = searchQueryBuilder.ToSearchQuery(request);
        //    var serializedQuerySearchFields = JsonConvert.SerializeObject(searchQuery, ModuleConstants.Api.JsonSerializerSettings);

        //    // Assert
        //    Assert.Equal(WrapWithDefaultSearchQuery(expectedResult), serializedQuerySearchFields);
        //}

        //[Theory(Skip = "Temporary")]
        //[InlineData(null, "")]
        //[InlineData(new string[] {}, @"""search_fields"":{},")]
        //[InlineData(new[] { "test" }, @"""search_fields"":{""test"":{}},")]
        //[InlineData(new[] { "test1", "test2" }, @"""search_fields"":{""test1"":{},""test2"":{}},")]
        //public void TestSearchFields(string[] searchField, string expectedResult)
        //{
        //    // Arrange
        //    var fieldNameConverter = GetFieldNameConverter();
        //    var searchFiltersBuilder = GetSearchFiltersBuilder();
        //    var searchQueryBuilder = new SearchQueryBuilder(fieldNameConverter, searchFiltersBuilder);
        //    var request = new SearchRequest { SearchFields = searchField };

        //    // Act
        //    var searchQuery = searchQueryBuilder.ToSearchQuery(request);
        //    var serializedSearchFields = JsonConvert.SerializeObject(searchQuery, ModuleConstants.Api.JsonSerializerSettings);

        //    // Assert
        //    Assert.Equal(WrapWithDefaultSearchQuery(expectedResult), serializedSearchFields);
        //}

        private static IFieldNameConverter GetFieldNameConverter()
        {
            var mock = new Mock<IFieldNameConverter>();
            mock.Setup(x => x.ToProviderFieldName(It.IsAny<string>())).Returns<string>(x => x);
            return mock.Object;
        }

        private static ISearchFiltersBuilder GetSearchFiltersBuilder()
        {
            var mock = new Mock<ISearchFiltersBuilder>();
            return mock.Object;
        }

        private static string WrapWithDefaultSearchQuery(string expectedResult)
        {
            return $@"{{""query"":"""",{expectedResult}""page"":{{""size"":20,""current"":1}}}}";
        }
    }
}
