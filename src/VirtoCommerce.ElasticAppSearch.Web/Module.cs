using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using VirtoCommerce.ElasticAppSearch.Core;
using VirtoCommerce.ElasticAppSearch.Core.Models;
using VirtoCommerce.ElasticAppSearch.Data.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.SearchModule.Core.Services;

namespace VirtoCommerce.ElasticAppSearch.Web;

public class Module : IModule, IHasConfiguration
{
    public ManifestModuleInfo ModuleInfo { get; set; }

    public IConfiguration Configuration { get; set; }

    public void Initialize(IServiceCollection serviceCollection)
    {
        var provider = Configuration.GetValue<string>("Search:Provider");

        if (provider.EqualsInvariant(ModuleConstants.ModuleName))
        {
            serviceCollection.Configure<ElasticAppSearchOptions>(Configuration.GetSection($"Search:{ModuleConstants.ModuleName}"));
            serviceCollection.AddSingleton<IValidateOptions<ElasticAppSearchOptions>, ElasticAppSearchOptionsValidator>();

            serviceCollection.AddSingleton<ApiClient>();
            serviceCollection.AddSingleton<ElasticAppSearchQueryBuilder>();
            serviceCollection.AddSingleton<ElasticAppSearchResponseBuilder>();
            serviceCollection.AddSingleton<ISearchProvider, ElasticAppSearchProvider>();

            serviceCollection.AddHttpClient(ModuleConstants.ModuleName, (serviceProvider, httpClient) =>
            {
                var elasticAppSearchOptions = serviceProvider.GetRequiredService<IOptions<ElasticAppSearchOptions>>().Value;

                httpClient.BaseAddress = new Uri($"{elasticAppSearchOptions.Endpoint}/api/as/v1/");
                
                httpClient.DefaultRequestHeaders.Add(HeaderNames.Authorization, $"Bearer {elasticAppSearchOptions.PrivateApiKey}");

                if (elasticAppSearchOptions.EnableHttpCompression)
                {
                    httpClient.DefaultRequestHeaders.Add(HeaderNames.AcceptEncoding, DecompressionMethods.GZip.ToString());
                }
            }).ConfigurePrimaryHttpMessageHandler(serviceProvider =>
            {
                var elasticAppSearchOptions = serviceProvider.GetRequiredService<IOptions<ElasticAppSearchOptions>>().Value;

                var handler = new HttpClientHandler
                {
                    AutomaticDecompression = elasticAppSearchOptions.EnableHttpCompression ? DecompressionMethods.GZip : DecompressionMethods.None
                };

                return handler;
            });
        }
    }

    public void PostInitialize(IApplicationBuilder appBuilder)
    {
        // register settings
        var settingsRegistrar = appBuilder.ApplicationServices.GetRequiredService<ISettingsRegistrar>();
        settingsRegistrar.RegisterSettings(ModuleConstants.Settings.AllSettings, ModuleInfo.Id);

        // register permissions
        var permissionsProvider = appBuilder.ApplicationServices.GetRequiredService<IPermissionsRegistrar>();
        permissionsProvider.RegisterPermissions(ModuleConstants.Security.Permissions.AllPermissions.Select(x =>
            new Permission
            {
                GroupName = ModuleConstants.ModuleName,
                ModuleId = ModuleInfo.Id,
                Name = x
            }).ToArray());
    }

    public void Uninstall()
    {
        // do nothing in here
    }
}
