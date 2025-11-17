using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Moq;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Schema;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query;
using VirtoCommerce.ElasticAppSearch.Core.Services;
using VirtoCommerce.ElasticAppSearch.Core.Services.Builders;
using VirtoCommerce.ElasticAppSearch.Core.Services.Converters;
using VirtoCommerce.ElasticAppSearch.Data.Services;
using VirtoCommerce.Platform.Caching;
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
            appSearchClient.Setup(x => x.GetSchemaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(() => new Schema());
            appSearchClient.Setup(x => x.GetSearchSettingsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(() => new SearchSettings());

            var searchOptions = Options.Create(new SearchOptions());
            var documentConverter = new Mock<IDocumentConverter>();
            var searchQueryBuilder = new Mock<ISearchQueryBuilder>();
            var searchResponseBuilder = new Mock<ISearchResponseBuilder>();

            //  setup cache mocks
            var cacheEntry = new Mock<ICacheEntry>();
            cacheEntry.SetupGet(c => c.ExpirationTokens).Returns(new List<IChangeToken>());

            searchQueryBuilder
                .Setup(x => x.ToSearchQueries(It.IsAny<SearchRequest>(), It.IsAny<Schema>(), It.IsAny<SearchSettings>()))
                .Returns(new List<SearchQueryAggregationWrapper> { new SearchQueryAggregationWrapper() });

            var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            var platformMemoryCache = new PlatformMemoryCache(memoryCache, Options.Create(new CachingOptions()), new Mock<ILogger<PlatformMemoryCache>>().Object);

            var appSearchProvider = new ElasticAppSearchProvider(
                searchOptions,
                appSearchClient.Object,
                documentConverter.Object,
                searchQueryBuilder.Object,
                searchResponseBuilder.Object,
                platformMemoryCache
            );

            var searchRequest = new SearchRequest
            {
                RawQuery = testQuery,
            };


            // Act
            await appSearchProvider.SearchAsync("testDocumentType", searchRequest);

            // Assert
            appSearchClient.Verify(x => x.SearchAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), rawQueryTimesCall);
            appSearchClient.Verify(x => x.SearchAsync(It.IsAny<string>(), It.IsAny<SearchQuery>(), It.IsAny<CancellationToken>()), regularSearchQueryTimesCall);
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
