using System;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
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
            serviceCollection.AddSingleton<IElasticAppSearchPolicySelector, ElasticAppSearchPolicySelector>();

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
                .AddPolicyHandler((serviceProvider, _) => serviceProvider.GetRequiredService<IElasticAppSearchPolicySelector>().GetRetryPolicy());
        }
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
