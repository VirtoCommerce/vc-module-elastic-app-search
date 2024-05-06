using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;
using VirtoCommerce.ElasticAppSearch.Core.Models;

namespace VirtoCommerce.ElasticAppSearch.Data.Services;

public class ElasticAppSearchPolicySelector
{
    private readonly ElasticAppSearchOptions _options;
    private readonly ILogger<ElasticAppSearchApiClient> _logger;

    public ElasticAppSearchPolicySelector(IOptions<ElasticAppSearchOptions> options, ILogger<ElasticAppSearchApiClient> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public virtual AsyncRetryPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions.HandleTransientHttpError()
            .WaitAndRetryAsync(_options.RetryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(_options.SleepDurationPowerBase, retryAttempt - _options.RetryCount)),
                (outcome, timespan, retryAttempt, _) =>
                {
                    _logger.LogWarning(
                            "Request failed with status code {StatusCode}, delaying for {Delay} milliseconds then making retry {RetryAttempt}",
                            outcome.Result?.StatusCode ?? (outcome.Exception as HttpRequestException)?.StatusCode,
                            timespan.TotalMilliseconds, retryAttempt);
                });
    }
}
