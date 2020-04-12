using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client
{
    public class ApiClient
    {
        private readonly HttpClient httpClient;
        private readonly JsonSerializerOptions jsonSerializerOptions;

        public ApiClient(HttpClient httpClient, JsonSerializerOptions jsonSerializerOptions)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this.jsonSerializerOptions = jsonSerializerOptions;
        }

        public async Task<Maybe<T>> GetJsonAsync<T>(string requestUri)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return Maybe.None;
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, jsonSerializerOptions);
        }

        public Task<Maybe<T>> PostJsonAsync<T>(string requestUri, object? content)
            => SendJsonAsync<T>(HttpMethod.Post, requestUri, content);

        public Task<Maybe<T>> PatchJsonAsync<T>(string requestUri, object? content)
            => SendJsonAsync<T>(HttpMethod.Patch, requestUri, content);

        private async Task<Maybe<T>> SendJsonAsync<T>(HttpMethod method, string requestUri, object? content)
        {
            var requestJson = JsonSerializer.Serialize(content, jsonSerializerOptions);
            using var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");
            using var request = new HttpRequestMessage(method, requestUri) { Content = requestContent };
            var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return Maybe.None;
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, jsonSerializerOptions);
        }
    }
}