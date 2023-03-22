using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Moq;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Schema;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query;
using VirtoCommerce.ElasticAppSearch.Core.Services;
using VirtoCommerce.ElasticAppSearch.Core.Services.Builders;
using VirtoCommerce.ElasticAppSearch.Core.Services.Converters;
using VirtoCommerce.ElasticAppSearch.Data.Services;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.SearchModule.Core.Model;
using Xunit;

namespace VirtoCommerce.ElasticAppSearch.Tests
{
    public class ElasticAppSearchProviderTests
    {
        [Theory]
        [ClassData(typeof(QueryTestData))]
        public async Task TestRawQuerySearch(string testQuery, Times rawQueryTimesCall, Times regularSearchQueryTimesCall)
        {
            // Arrange
            var appSearchClient = new Mock<IElasticAppSearchApiClient>();

            var searchOptions = Options.Create(new SearchOptions());
            var documentConverter = new Mock<IDocumentConverter>();
            var searchQueryBuilder = new Mock<ISearchQueryBuilder>();
            var searchResponseBuilder = new Mock<ISearchResponseBuilder>();
            var platformMemoryCache = new Mock<IPlatformMemoryCache>();

            //  setup cache moqs
            var cacheEntry = new Mock<ICacheEntry>();
            var engineName = string.Join("-", searchOptions.Value.Scope, "testDocumentType").ToLowerInvariant();
            var cacheKey = CacheKey.With(typeof(ElasticAppSearchProvider), "GetSchemaAsync", engineName);

            cacheEntry.SetupGet(c => c.ExpirationTokens).Returns(new List<IChangeToken>());
            platformMemoryCache.Setup(pmc => pmc.CreateEntry(cacheKey)).Returns(cacheEntry.Object);
            platformMemoryCache.Setup(x => x.GetDefaultCacheEntryOptions()).Returns(() => new MemoryCacheEntryOptions());

            searchQueryBuilder
                .Setup(x => x.ToSearchQueries(It.IsAny<SearchRequest>(), It.IsAny<Schema>()))
                .Returns(new List<SearchQueryAggregationWrapper> { new SearchQueryAggregationWrapper() });

            var appSearchProvider = new ElasticAppSearchProvider(
                searchOptions,
                appSearchClient.Object,
                documentConverter.Object,
                searchQueryBuilder.Object,
                searchResponseBuilder.Object,
                platformMemoryCache.Object
            );

            var searchRequest = new SearchRequest
            {
                RawQuery = testQuery,
            };


            // Act
            await appSearchProvider.SearchAsync("testDocumentType", searchRequest);

            // Assert
            appSearchClient.Verify(x => x.SearchAsync(It.IsAny<string>(), It.IsAny<string>()), rawQueryTimesCall);
            appSearchClient.Verify(x => x.SearchAsync(It.IsAny<string>(), It.IsAny<SearchQuery>()), regularSearchQueryTimesCall);
        }

        private class QueryTestData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] { "testQuery", Times.Once(), Times.Never() };
                yield return new object[] { null, Times.Never(), Times.Once() };
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
