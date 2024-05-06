using System.Net.Http;
using Polly;

namespace VirtoCommerce.ElasticAppSearch.Core.Services;

public interface IElasticAppSearchPolicySelector
{
    IAsyncPolicy<HttpResponseMessage> GetRetryPolicy();
}
