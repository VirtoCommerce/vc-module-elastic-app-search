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
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Synonyms;
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
        var engineName = Environment.GetEnvironmentVariable("TestElasticAppSearchEngineName") ?? "default-product";

        var client = GetSearchClient();

        var response = await client.GetSearchSettingsAsync(engineName);

        Assert.NotNull(response);
    }


    [Fact]
    public async Task CanSearchExplain()
    {
        var engineName = Environment.GetEnvironmentVariable("TestElasticAppSearchEngineName") ?? "default-product";

        var client = GetSearchClient();

        var response = await client.SearchExplainAsync(engineName, new SearchQuery { Query = "Red" });

        Assert.NotNull(response);
    }

    [Fact]
    public async Task CanGetSynonymsWithoutPage()
    {
        var engineName = Environment.GetEnvironmentVariable("TestElasticAppSearchEngineName") ?? "default-product";

        var client = GetSearchClient();

        var response = await client.GetSynonymsAsync(engineName, new SynonymApiQuery());

        Assert.NotNull(response);
    }

    [Fact]
    public async Task CanGetSynonymsWithPage()
    {
        var engineName = Environment.GetEnvironmentVariable("TestElasticAppSearchEngineName") ?? "default-product";

        var client = GetSearchClient();

        var response = await client.GetSynonymsAsync(engineName, new SynonymApiQuery(1, 10));

        Assert.NotNull(response);
    }

    [Fact]
    public async Task CanCreateSynonym()
    {
        var engineName = Environment.GetEnvironmentVariable("TestElasticAppSearchEngineName") ?? "default-product";

        var client = GetSearchClient();

        var response = await client.CreateSynonymSetAsync(engineName, new SynonymSet(["kilogram", "kg"]));

        Assert.NotNull(response);
        Assert.NotEmpty(response.Id);
    }

    protected static IElasticAppSearchApiClient GetSearchClient()
    {
        var host = Environment.GetEnvironmentVariable("TestElasticAppSearchHost") ?? "http://localhost:3002";
        var privateApiKey = Environment.GetEnvironmentVariable("TestElasticAppSearchPrivateKey") ?? "";

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
