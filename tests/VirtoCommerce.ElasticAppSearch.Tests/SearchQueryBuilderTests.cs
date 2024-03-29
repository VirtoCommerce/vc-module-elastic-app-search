using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Schema;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Sorting;
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
            new object[] { Array.Empty<SortingField>(), Array.Empty<Field<SortOrder>>() },
            new object[]
            {
                new SortingField[] { new("test") },
                new FieldSort[]
                {
                    new()
                    {
                        FieldName = "test",
                        Value = SortOrder.Asc
                    }
                }
            },
            new object[]
            {
                new SortingField[] { new("test1"), new("test2", true) },
                new FieldSort[]
                {
                    new()
                    {
                        FieldName = "test1",
                        Value = SortOrder.Asc
                    },
                    new()
                    {
                        FieldName = "test2",
                        Value = SortOrder.Desc
                    }
                }
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

        public static IEnumerable<object[]> ResultFieldsData => new[]
{
            new object[] { null, null },
            new object[] { Array.Empty<string>(), new Dictionary<string, ResultFieldValue>() },
            new object[]
            {
                new[] { "test" },
                new Dictionary<string, ResultFieldValue>
                {
                    { "test", new ResultFieldValue() }
                }
            },
            new object[]
            {
                new[] { "test1", "test2" },
                new Dictionary<string, ResultFieldValue>
                {
                    { "test1", new ResultFieldValue() },
                    { "test2", new ResultFieldValue() }
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

        [Theory]
        [MemberData(nameof(ResultFieldsData))]
        public void ToSearchQuery_ConvertValidResultFields_Successfully(string[] resultFields, object expectedResult)
        {
            Test(() => new SearchRequest { IncludeFields = resultFields }, searchQuery => searchQuery.ResultFields, expectedResult);
        }

        private static void Test(Func<SearchRequest> searchRequest, Func<SearchQuery, object> actualResultSelector, object expectedResult)
        {
            // Arrange
            var logger = GetLogger<SearchQueryBuilder>();
            var fieldNameConverter = GetFieldNameConverter();
            var searchFiltersBuilder = GetSearchFiltersBuilder();
            var searchFacetsQueryBuilder = GetSearchFacetsQueryBuilder();
            var searchBoostsBuilder = GetSearchBoostsBulder();
            var searchQueryBuilder = new SearchQueryBuilder(logger, fieldNameConverter, searchFiltersBuilder, searchFacetsQueryBuilder, searchBoostsBuilder);
            var request = searchRequest();
            var schema = GetSchema();
            var settings = new SearchSettings();

            // Act
            var searchQueries = searchQueryBuilder.ToSearchQueries(request, schema, settings);

            // Assert
            var searchQuery = searchQueries.FirstOrDefault(x => x.AggregationId == null).SearchQuery;
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

        private static ILogger<T> GetLogger<T>()
        {
            var mock = new Mock<ILogger<T>>();
            return mock.Object;
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

        private static ISearchFacetsQueryBuilder GetSearchFacetsQueryBuilder()
        {
            var mock = new Mock<ISearchFacetsQueryBuilder>();
            return mock.Object;
        }

        private static ISearchBoostsBuilder GetSearchBoostsBulder()
        {
            var mock = new Mock<ISearchBoostsBuilder>();
            return mock.Object;
        }
    }
}
