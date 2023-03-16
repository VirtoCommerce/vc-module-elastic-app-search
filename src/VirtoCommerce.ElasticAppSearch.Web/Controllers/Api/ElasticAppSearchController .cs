using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using VirtoCommerce.ElasticAppSearch.Core;
using VirtoCommerce.ElasticAppSearch.Core.Models;

namespace VirtoCommerce.ElasticAppSearch.Web.Controllers.Api
{
    [Route("api/elastic-app-search")]
    public class ElasticAppSearchController : Controller
    {
        private readonly ElasticAppSearchOptions _options;

        public ElasticAppSearchController(IOptions<ElasticAppSearchOptions> options)
        {
            _options = options.Value ?? new ElasticAppSearchOptions();
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

    }
}
