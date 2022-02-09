using System.Net.Http;
using Microsoft.Extensions.Options;
using VirtoCommerce.ElasticAppSearch.Core.Models;

namespace VirtoCommerce.ElasticAppSearch.Data.Services;

public class ElasticAppSearchOptionsValidator: IValidateOptions<ElasticAppSearchOptions>
{
    public ValidateOptionsResult Validate(string name, ElasticAppSearchOptions options)
    {
        var httpClientHandler = new HttpClientHandler();
        if (options.EnableHttpCompression && !httpClientHandler.SupportsAutomaticDecompression)
        {
            return ValidateOptionsResult.Fail("Automatic HTTP response content compression isn't supported by platform");
        }
        return ValidateOptionsResult.Success;
    }
}
