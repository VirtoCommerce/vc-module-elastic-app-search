using System;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;
using VirtoCommerce.ElasticAppSearch.Core;
using VirtoCommerce.ElasticAppSearch.Core.Models;
using VirtoCommerce.ElasticAppSearch.Core.Services;
using VirtoCommerce.ElasticAppSearch.Core.Services.Builders;
using VirtoCommerce.ElasticAppSearch.Core.Services.Converters;
using VirtoCommerce.ElasticAppSearch.Data.Services;
using VirtoCommerce.ElasticAppSearch.Data.Services.Builders;
using VirtoCommerce.ElasticAppSearch.Data.Services.Converters;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.SearchModule.Core.Extensions;

namespace VirtoCommerce.ElasticAppSearch.Web;

public class Module : IModule, IHasConfiguration
{
    public ManifestModuleInfo ModuleInfo { get; set; }
    public IConfiguration Configuration { get; set; }

    public void Initialize(IServiceCollection serviceCollection)
    {
        if (Configuration.SearchProviderActive(ModuleConstants.ModuleName))
        {
            serviceCollection.Configure<ElasticAppSearchOptions>(Configuration.GetSection($"Search:{ModuleConstants.ModuleName}"));
            serviceCollection.AddSingleton<IValidateOptions<ElasticAppSearchOptions>, ElasticAppSearchOptionsValidator>();

            serviceCollection.AddSingleton<IElasticAppSearchApiClient, ElasticAppSearchApiClient>();
            serviceCollection.AddSingleton<IFieldNameConverter, FieldNameConverter>();
            serviceCollection.AddSingleton<IDocumentConverter, DocumentConverter>();
            serviceCollection.AddSingleton<ISearchFiltersBuilder, SearchFiltersBuilder>();
            serviceCollection.AddSingleton<ISearchBoostsBuilder, SearchBoostsBuilder>();
            serviceCollection.AddSingleton<ISearchQueryBuilder, SearchQueryBuilder>();
            serviceCollection.AddSingleton<ISearchResponseBuilder, SearchResponseBuilder>();
            serviceCollection.AddSingleton<ElasticAppSearchProvider>();
            serviceCollection.AddSingleton<ISearchFacetsQueryBuilder, SearchFacetsQueryBuilder>();
            serviceCollection.AddSingleton<IAggregationsResponseBuilder, AggregationsResponseBuilder>();

            serviceCollection.AddHttpClient(ModuleConstants.ModuleName, (serviceProvider, httpClient) =>
                {
                    var elasticAppSearchOptions = serviceProvider.GetRequiredService<IOptions<ElasticAppSearchOptions>>().Value;

                    httpClient.BaseAddress = new Uri($"{elasticAppSearchOptions.Endpoint}/api/as/v1/");

                    httpClient.DefaultRequestHeaders.Add(HeaderNames.Authorization, $"Bearer {elasticAppSearchOptions.PrivateApiKey}");

                    if (elasticAppSearchOptions.EnableHttpCompression)
                    {
                        httpClient.DefaultRequestHeaders.Add(HeaderNames.AcceptEncoding, DecompressionMethods.GZip.ToString());
                    }
                })
                .ConfigurePrimaryHttpMessageHandler(serviceProvider =>
                {
                    var elasticAppSearchOptions = serviceProvider.GetRequiredService<IOptions<ElasticAppSearchOptions>>().Value;

                    var handler = new HttpClientHandler
                    {
                        AutomaticDecompression = elasticAppSearchOptions.EnableHttpCompression ? DecompressionMethods.GZip : DecompressionMethods.None,
                    };

                    return handler;
                })
                .AddPolicyHandler((serviceProvider, _) => GetRetryPolicy(serviceProvider));
        }
    }

    private static AsyncRetryPolicy<HttpResponseMessage> GetRetryPolicy(IServiceProvider services)
    {
        const int retryCount = 2;
        const int sleepDurationPowerBase = 2;
        return HttpPolicyExtensions.HandleTransientHttpError()
            .WaitAndRetryAsync(retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(sleepDurationPowerBase, retryAttempt - retryCount)),
                (outcome, timespan, retryAttempt, _) =>
                {
                    services.GetService<ILogger<ElasticAppSearchApiClient>>()?
                        .LogWarning(
                            "Request failed with status code {StatusCode}, delaying for {Delay} milliseconds then making retry {RetryAttempt}",
                            outcome.Result?.StatusCode ?? (outcome.Exception as HttpRequestException)?.StatusCode,
                            timespan.TotalMilliseconds, retryAttempt);
                });
    }

    public void PostInitialize(IApplicationBuilder appBuilder)
    {
        // register settings
        var settingsRegistrar = appBuilder.ApplicationServices.GetRequiredService<ISettingsRegistrar>();
        settingsRegistrar.RegisterSettings(ModuleConstants.Settings.AllSettings, ModuleInfo.Id);

        // register permissions
        var permissionsRegistrar = appBuilder.ApplicationServices.GetRequiredService<IPermissionsRegistrar>();
        permissionsRegistrar.RegisterPermissions(ModuleInfo.Id, ModuleConstants.ModuleName, ModuleConstants.Security.Permissions.AllPermissions);

        if (Configuration.SearchProviderActive(ModuleConstants.ModuleName))
        {
            appBuilder.UseSearchProvider<ElasticAppSearchProvider>(ModuleConstants.ModuleName);
        }
    }

    public void Uninstall()
    {
        // do nothing in here
    }
}
