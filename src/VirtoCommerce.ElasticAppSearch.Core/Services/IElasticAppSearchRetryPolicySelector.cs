using System.Net.Http;
using Polly.Retry;

namespace VirtoCommerce.ElasticAppSearch.Core.Services;

public interface IElasticAppSearchRetryPolicySelector
{
    AsyncRetryPolicy<HttpResponseMessage> GetRetryPolicy();
}
