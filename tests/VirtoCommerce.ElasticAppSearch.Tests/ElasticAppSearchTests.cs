using System;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Moq;
using VirtoCommerce.ElasticAppSearch.Core;
using VirtoCommerce.ElasticAppSearch.Data.Services;
using VirtoCommerce.ElasticAppSearch.Data.Services.Builders;
using VirtoCommerce.ElasticAppSearch.Data.Services.Converters;
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;
using Xunit;

namespace VirtoCommerce.ElasticAppSearch.Tests
{
    [Trait("Category", "CI")]
    [Trait("Category", "IntegrationTest")]
    public class ElasticAppSearchTests : SearchProviderTests
    {
        protected override ISearchProvider GetSearchProvider()
        {

            var host = Environment.GetEnvironmentVariable("TestElasticAppSearchHost") ?? "http://localhost:9200";
            var privateApiKey = Environment.GetEnvironmentVariable("TestElasticAppSearchPrivateKey") ?? "";

            var elasticOptions = Options.Create(new SearchOptions { Scope = "test" });
            var searchOptions = Options.Create(new SearchOptions { Scope = "test-core", Provider = "ElasticSearch" });

            IServiceCollection services = new ServiceCollection(); // [1]

            services.AddHttpClient(ModuleConstants.ModuleName, (serviceProvider, httpClient) =>
            {

                httpClient.BaseAddress = new Uri($"{host}/api/as/v1/");

                httpClient.DefaultRequestHeaders.Add(HeaderNames.Authorization, $"Bearer {privateApiKey}");

            }).ConfigurePrimaryHttpMessageHandler(serviceProvider =>
            {
                var handler = new HttpClientHandler
                {
                    AutomaticDecompression = DecompressionMethods.None
                };

                return handler;
            });

            var httpFactory = services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();

            var apiClient = new ElasticAppSearchApiClient(httpFactory);
            var fieldNameConverter = new FieldNameConverter();
            var documentConverter = new DocumentConverter(Mock.Of<ILogger<DocumentConverter>>(), fieldNameConverter);
            var searchFilterBuilder = new SearchFiltersBuilder(Mock.Of<ILogger<SearchFiltersBuilder>>(), fieldNameConverter);
            var facetsBuilder = new SearchFacetsQueryBuilder(Mock.Of<ILogger<SearchFacetsQueryBuilder>>(), fieldNameConverter, searchFilterBuilder);
            var searchQueryBuilder = new SearchQueryBuilder(Mock.Of<ILogger<SearchQueryBuilder>>(), fieldNameConverter, searchFilterBuilder, facetsBuilder);
            var aggrResponsebuilder = new AggregationsResponseBuilder(fieldNameConverter);
            var searchResponseBuilder = new SearchResponseBuilder(documentConverter, aggrResponsebuilder);
            var memoryCache = new MemoryCache(new MemoryCacheOptions());

            var cachingOptions = Options.Create(new CachingOptions { CacheEnabled = true });
            var cache = new PlatformMemoryCache(memoryCache, cachingOptions, Mock.Of<ILogger<PlatformMemoryCache>>());

            var provider = new ElasticAppSearchProvider(searchOptions, apiClient, documentConverter, searchQueryBuilder, searchResponseBuilder, cache);
            return provider;
        }
    }
}
