using System;
using System.Collections.Generic;
using Moq;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Schema;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.SearchFields;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Sort;
using VirtoCommerce.ElasticAppSearch.Core.Services.Builders;
using VirtoCommerce.ElasticAppSearch.Core.Services.Converters;
using VirtoCommerce.ElasticAppSearch.Data.Services.Builders;
using VirtoCommerce.SearchModule.Core.Model;
using Xunit;

namespace VirtoCommerce.ElasticAppSearch.Tests
{
    public class SearchQueryBuilderTests
    {
        public static IEnumerable<object[]> SortData => new[]
        {
            new object[] { null, null },
            new object[] { Array.Empty<SortingField>(), new Sort() },
            new object[]
            {
                new SortingField[] { new("test") },
                new Sort(new[]
                {
                    new Field<SortOrder>
                    {
                        FieldName = "test",
                        Value = SortOrder.Asc
                    }
                })
            },
            new object[]
            {
                new SortingField[] { new("test1"), new("test2", true) },
                new Sort(new[]
                {
                    new Field<SortOrder>
                    {
                        FieldName = "test1",
                        Value = SortOrder.Asc
                    },
                    new Field<SortOrder>
                    {
                        FieldName = "test2",
                        Value = SortOrder.Desc
                    }
                })
            }
        };

        public static IEnumerable<object[]> SearchFieldsData => new[]
        {
            new object[] { null, null },
            new object[] { Array.Empty<string>(), new Dictionary<string, SearchFieldValue>() },
            new object[]
            {
                new[] { "test" },
                new Dictionary<string, SearchFieldValue>
                {
                    { "test", new SearchFieldValue() }
                }
            },
            new object[]
            {
                new[] { "test1", "test2" },
                new Dictionary<string, SearchFieldValue>
                {
                    { "test1", new SearchFieldValue() },
                    { "test2", new SearchFieldValue() }
                }
            }
        };

        [Theory]
        [MemberData(nameof(SortData))]
        public void ToSearchQuery_ConvertValidSorting_Successfully(SortingField[] sorting, object expectedResult)
        {
            Test(() => new SearchRequest { Sorting = sorting }, searchQuery => searchQuery.Sort, expectedResult);
        }

        [Theory]
        [MemberData(nameof(SearchFieldsData))]
        public void ToSearchQuery_ConvertValidSearchFields_Successfully(string[] searchFields, object expectedResult)
        {
            Test(() => new SearchRequest { SearchFields = searchFields }, searchQuery => searchQuery.SearchFields, expectedResult);
        }

        private static void Test(Func<SearchRequest> searchRequest, Func<SearchQuery, object> actualResultSelector, object expectedResult)
        {
            // Arrange
            var fieldNameConverter = GetFieldNameConverter();
            var searchFiltersBuilder = GetSearchFiltersBuilder();
            var searchQueryBuilder = new SearchQueryBuilder(fieldNameConverter, searchFiltersBuilder);
            var request = searchRequest();
            var schema = GetSchema();

            // Act
            var searchQuery = searchQueryBuilder.ToSearchQuery(request, schema);

            // Assert
            var actualResult = actualResultSelector(searchQuery);
            Assert.Equal(expectedResult, actualResult);
        }

        private static Schema GetSchema()
        {
            return new Schema
            {
                Fields = new Dictionary<string, FieldType>
                {
                    { "test", FieldType.Text },
                    { "test1", FieldType.Text },
                    { "test2", FieldType.Number },
                    { "test3", FieldType.Date },
                    { "test4", FieldType.Geolocation },
                }
            };
        }

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
    }
}
