using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VirtoCommerce.SearchModule.Core.Exceptions;

namespace VirtoCommerce.ElasticAppSearch.Data.Extensions;

public static class HttpClientExtensions
{
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
        return await client.PostAsync(requestUri, value.ToJson(jsonSerializerSettings), cancellationToken);
    }

    public static async Task<HttpResponseMessage> PostAsJsonAsync<TValue>(this HttpClient client, Uri requestUri, TValue value,
        JsonSerializerSettings jsonSerializerSettings = null, CancellationToken cancellationToken = default)
    {
        return await client.PostAsync(requestUri, value.ToJson(jsonSerializerSettings), cancellationToken);
    }

    public static async Task<HttpResponseMessage> PutAsJsonAsync<TValue>(this HttpClient client, string requestUri, TValue value,
        JsonSerializerSettings jsonSerializerSettings = null, CancellationToken cancellationToken = default)
    {
        return await client.PutAsync(requestUri, value.ToJson(jsonSerializerSettings), cancellationToken);
    }

    public static async Task<HttpResponseMessage> PutAsJsonAsync<TValue>(this HttpClient client, Uri requestUri, TValue value,
        JsonSerializerSettings jsonSerializerSettings = null, CancellationToken cancellationToken = default)
    {
        return await client.PutAsync(requestUri, value.ToJson(jsonSerializerSettings), cancellationToken);
    }

    public static async Task<HttpResponseMessage> DeleteAsJsonAsync<TValue>(this HttpClient client, string requestUri, TValue value,
        JsonSerializerSettings jsonSerializerSettings = null, CancellationToken cancellationToken = default)
    {
        var httpMessage = new HttpRequestMessage(HttpMethod.Delete, requestUri) { Content = value.ToJson(jsonSerializerSettings) };
        return await client.SendAsync(httpMessage, cancellationToken);
    }

    public static async Task<HttpResponseMessage> DeleteAsJsonAsync<TValue>(this HttpClient client, Uri requestUri, TValue value,
        JsonSerializerSettings jsonSerializerSettings = null, CancellationToken cancellationToken = default)
    {
        var httpMessage = new HttpRequestMessage(HttpMethod.Delete, requestUri) { Content = value.ToJson(jsonSerializerSettings) };
        return await client.SendAsync(httpMessage, cancellationToken);
    }

    public static async Task<TValue> ReadFromJsonAsync<TValue>(this HttpContent httpContent,
        JsonSerializerSettings jsonSerializerSettings = null, CancellationToken cancellationToken = default)
    {
        var content = await httpContent.ReadAsStringAsync(cancellationToken);
        return JsonConvert.DeserializeObject<TValue>(content, jsonSerializerSettings);
    }

    public static async Task EnsureSuccessStatusCodeAsync<TResult>(this HttpResponseMessage httpResponseMessage, JsonSerializerSettings jsonSerializerSettings = null)
    {
        try
        {
            httpResponseMessage.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException exception)
        {
            var content = await httpResponseMessage.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<TResult>(content, jsonSerializerSettings);
            throw new SearchException(result?.ToString(), exception);
        }
        finally
        {
            throw new SearchException($"ElasticAppSearch: ({httpResponseMessage.StatusCode}) {httpResponseMessage.ReasonPhrase}");
        }
    }

    public static StringContent ToJson<TValue>(this TValue value, JsonSerializerSettings jsonSerializerSettings = null)
    {
        var content = JsonConvert.SerializeObject(value, jsonSerializerSettings);
        return new StringContent(content, Encoding.UTF8, "application/json");
    }
}
