using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Moq;
using VirtoCommerce.ElasticAppSearch.Core;
using VirtoCommerce.ElasticAppSearch.Core.Models;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query;
using VirtoCommerce.ElasticAppSearch.Core.Services;
using VirtoCommerce.ElasticAppSearch.Data.Services;
using Xunit;

namespace VirtoCommerce.ElasticAppSearch.Tests;

[Trait("Category", "CI")]
[Trait("Category", "IntegrationTest")]
public class ElasticAppSearchApiClientTests
{
    [Fact]
    public async Task CanGetSearchSettings()
    {
        var engineName = "default-product";

        var client = GetSearchClient();

        var response = await client.GetSearchSettingsAsync(engineName);

        Assert.NotNull(response);
    }


    [Fact]
    public async Task CanSearchExplain()
    {
        var engineName = "default-product";

        var client = GetSearchClient();

        var response = await client.SearchExplainAsync(engineName, new SearchQuery { Query = "Red" });

        Assert.NotNull(response);
    }

    protected IElasticAppSearchApiClient GetSearchClient()
    {
        var host = Environment.GetEnvironmentVariable("TestElasticAppSearchHost") ?? "https://bc9b603e27e14757803709f6e2b57888.ent-search.us-central1.gcp.cloud.es.io";
        var privateApiKey = Environment.GetEnvironmentVariable("TestElasticAppSearchPrivateKey") ?? "private-zdqez5efwrmonegt4pqfs88g";

        IServiceCollection services = new ServiceCollection(); // [1]

        services.AddHttpClient(ModuleConstants.ModuleName, (_, httpClient) =>
        {
            httpClient.BaseAddress = new Uri($"{host}/api/as/v1/");

            httpClient.DefaultRequestHeaders.Add(HeaderNames.Authorization, $"Bearer {privateApiKey}");

        }).ConfigurePrimaryHttpMessageHandler(_ =>
        {
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.None
            };

            return handler;
        });

        var httpFactory = services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();


        var appSearchOptionsMock = Mock.Of<IOptions<ElasticAppSearchOptions>>();
        var client = new ElasticAppSearchApiClient(httpFactory, Mock.Of<ILogger<ElasticAppSearchApiClient>>(), appSearchOptionsMock);

        return client;
    }
}
