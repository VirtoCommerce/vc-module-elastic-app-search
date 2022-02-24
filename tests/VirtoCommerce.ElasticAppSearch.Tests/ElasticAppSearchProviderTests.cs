using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search;
using VirtoCommerce.ElasticAppSearch.Core.Services;
using VirtoCommerce.ElasticAppSearch.Data.Services;
using VirtoCommerce.SearchModule.Core.Model;
using Xunit;

namespace VirtoCommerce.ElasticAppSearch.Tests
{
    public class ElasticAppSearchProviderTests
    {
        [Fact]
        public async Task TestRawQuerySearch()
        {
            // Arrange
            var appSearchClient = new Mock<IElasticAppApiClient>();

            var searchOptions = Options.Create(new SearchOptions());
            var documentConverter = new Mock<IDocumentConverter>();
            var searchQueryBuilder = new Mock<ISearchQueryBuilder>();
            var searchResponseBuilder = new Mock<ISearchResponseBuilder>();

            var appSearchProvider = new ElasticAppSearchProvider(
                searchOptions,
                appSearchClient.Object,
                documentConverter.Object,
                searchQueryBuilder.Object,
                searchResponseBuilder.Object
                );

            var searchRequest = new SearchRequest
            {
                RawQuery = "testQuery",
            };

            // Act
            var response = await appSearchProvider.SearchAsync("testDocumentType", searchRequest);

            // Assert
            appSearchClient.Verify(x => x.SearchAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            appSearchClient.Verify(x => x.SearchAsync(It.IsAny<string>(), It.IsAny<SearchQuery>()), Times.Never);
        }

        [Fact]
        public async Task TestRegularSearchCall()
        {
            // Arrange
            var appSearchClient = new Mock<IElasticAppApiClient>();

            var searchOptions = Options.Create(new SearchOptions());
            var documentConverter = new Mock<IDocumentConverter>();
            var searchQueryBuilder = new Mock<ISearchQueryBuilder>();
            var searchResponseBuilder = new Mock<ISearchResponseBuilder>();

            var appSearchProvider = new ElasticAppSearchProvider(
                searchOptions,
                appSearchClient.Object,
                documentConverter.Object,
                searchQueryBuilder.Object,
                searchResponseBuilder.Object
            );

            var searchRequest = new SearchRequest();

            // Act
            var response = await appSearchProvider.SearchAsync("testDocumentType", searchRequest);

            // Assert
            appSearchClient.Verify(x => x.SearchAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            appSearchClient.Verify(x => x.SearchAsync(It.IsAny<string>(), It.IsAny<SearchQuery>()), Times.Once);
        }
    }
}
