using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api;
using VirtoCommerce.SearchModule.Core.Exceptions;

namespace VirtoCommerce.ElasticAppSearch.Data.Extensions;

public static class HttpClientExtensions
{
    private static readonly JsonSerializerSettings JsonSerializerSettings = new()
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        Converters = new List<JsonConverter> { new StringEnumConverter(new CamelCaseNamingStrategy()) }
    };

    public static async Task<TValue> GetFromJsonAsync<TValue>(this HttpClient client, string requestUri,
        JsonSerializerSettings jsonSerializerSettings = null, CancellationToken cancellationToken = default)
    {
        var responseMessage = await client.GetAsync(requestUri, cancellationToken);
        return await responseMessage.Content.ReadFromJsonAsync<TValue>(jsonSerializerSettings, cancellationToken);
    }
    public static async Task<TValue> GetFromJsonAsync<TValue>(this HttpClient client, Uri requestUri,
        JsonSerializerSettings jsonSerializerSettings = null, CancellationToken cancellationToken = default)
    {
        var responseMessage = await client.GetAsync(requestUri, cancellationToken);
        return await responseMessage.Content.ReadFromJsonAsync<TValue>(jsonSerializerSettings, cancellationToken);
    }

    public static async Task<HttpResponseMessage> PostAsJsonAsync<TValue>(this HttpClient client, string requestUri, TValue value,
        JsonSerializerSettings jsonSerializerSettings = null, CancellationToken cancellationToken = default)
    {
        return await client.PostAsync(requestUri, value.ToJson(), cancellationToken);
    }

    public static async Task<HttpResponseMessage> PostAsJsonAsync<TValue>(this HttpClient client, Uri requestUri, TValue value,
        JsonSerializerSettings jsonSerializerSettings = null, CancellationToken cancellationToken = default)
    {
        return await client.PostAsync(requestUri, value.ToJson(), cancellationToken);
    }

    public static async Task<HttpResponseMessage> PutAsJsonAsync<TValue>(this HttpClient client, string requestUri, TValue value,
        JsonSerializerSettings jsonSerializerSettings = null, CancellationToken cancellationToken = default)
    {
        return await client.PutAsync(requestUri, value.ToJson(), cancellationToken);
    }

    public static async Task<HttpResponseMessage> PutAsJsonAsync<TValue>(this HttpClient client, Uri requestUri, TValue value,
        JsonSerializerSettings jsonSerializerSettings = null, CancellationToken cancellationToken = default)
    {
        return await client.PutAsync(requestUri, value.ToJson(), cancellationToken);
    }

    public static async Task<TValue> ReadFromJsonAsync<TValue>(this HttpContent httpContent,
        JsonSerializerSettings jsonSerializerSettings = null, CancellationToken cancellationToken = default)
    {
        var content = await httpContent.ReadAsStringAsync(cancellationToken);
        return JsonConvert.DeserializeObject<TValue>(content, jsonSerializerSettings ?? JsonSerializerSettings);
    }

    public static async Task EnsureSuccessStatusCodeAsync(this HttpResponseMessage httpResponseMessage)
    {
        try
        {
            httpResponseMessage.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException exception)
        {
            var content = await httpResponseMessage.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<Result>(content, JsonSerializerSettings);
            throw new SearchException(string.Join(Environment.NewLine, result.Errors), exception);
        }
    }

    private static StringContent ToJson<TValue>(this TValue value, JsonSerializerSettings jsonSerializerSettings = null)
    {
        var content = JsonConvert.SerializeObject(value, jsonSerializerSettings ?? JsonSerializerSettings);
        return new StringContent(content, Encoding.UTF8, "application/json");
    }
}
