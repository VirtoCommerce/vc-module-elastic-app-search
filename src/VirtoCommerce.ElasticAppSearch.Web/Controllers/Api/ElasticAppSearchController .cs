using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using VirtoCommerce.ElasticAppSearch.Core;
using VirtoCommerce.ElasticAppSearch.Core.Models;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Result;
using VirtoCommerce.ElasticAppSearch.Core.Services;

namespace VirtoCommerce.ElasticAppSearch.Web.Controllers.Api
{
    [Route("api/elastic-app-search")]
    public class ElasticAppSearchController : Controller
    {
        private readonly ElasticAppSearchOptions _options;
        private readonly IElasticAppSearchApiClient _appSearchApiClient;

        public ElasticAppSearchController(IOptions<ElasticAppSearchOptions> options, IElasticAppSearchApiClient appSearchApiClient)
        {
            _options = options.Value ?? new ElasticAppSearchOptions();
            _appSearchApiClient = appSearchApiClient;
        }

        [HttpGet]
        [Route("redirect")]
        [Authorize(ModuleConstants.Security.Permissions.Access)]
        public ActionResult Redirect()
        {
            if (string.IsNullOrEmpty(_options.KibanaBaseUrl))
            {
                return NotFound("ElasticAppSearch is not currently configured as a current search provider, or the Kibana Base URL has not been properly configured. Please check your configuration settings and try again.");
            }

            var uriBuilder = new UriBuilder(_options.KibanaBaseUrl);
            uriBuilder.Path = _options.KibanaPath;

            return Redirect(uriBuilder.ToString());
        }

        [HttpGet]
        [Route("diagnostic/{engineName}/search_settings")]
        [Authorize(ModuleConstants.Security.Permissions.Diagnostic)]
        public Task<SearchSettings> GetSearchSettings(string engineName)
        {
            return _appSearchApiClient.GetSearchSettingsAsync(engineName);
        }

        [HttpGet]
        [Route("diagnostic/{engineName}/search_explain")]
        [Authorize(ModuleConstants.Security.Permissions.Diagnostic)]
        public Task<SearchExplainResult> SearchExplain(string engineName, string query)
        {
            return _appSearchApiClient.SearchExplainAsync(engineName, new SearchQuery { Query = query });
        }

        [HttpGet]
        [Route("diagnostic/{engineName}/search")]
        [Authorize(ModuleConstants.Security.Permissions.Diagnostic)]
        public Task<SearchResult> Search(string engineName, string query)
        {
            return _appSearchApiClient.SearchAsync(engineName, new SearchQuery { Query = query });
        }
    }
}
