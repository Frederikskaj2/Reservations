using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client;

public class ApiClient(EventAggregator eventAggregator, HttpClient httpClient, JsonSerializerOptions jsonSerializerOptions, ILogger<ApiClient> logger)
{
    static readonly ProblemDetails serverConnectionError = new()
    {
        Type = "about:blank",
        Title = "Server Connection Error",
        Status = HttpStatusCode.InternalServerError,
    };

    protected EventAggregator EventAggregator { get; } = eventAggregator;

    public async ValueTask<ApiResponse<T>> Get<T>(string requestUri) where T : class
    {
        var response = await SendJson<T>(HttpMethod.Get, requestUri, value: null);
        if (!response.IsSuccess && (int) response.Problem!.Status >= 500)
            EventAggregator.Publish(ServerStatusMessage.Down);
        return response;
    }

    public async ValueTask<ApiResponse> Post(string requestUri) =>
        await SendJson<object>(HttpMethod.Post, requestUri, value: null);

    public async ValueTask<ApiResponse> Post(string requestUri, object request) =>
        await SendJson<object>(HttpMethod.Post, requestUri, request);

    public ValueTask<ApiResponse<T>> Post<T>(string requestUri)
        => SendJson<T>(HttpMethod.Post, requestUri, value: null);

    public ValueTask<ApiResponse<T>> Post<T>(string requestUri, HttpContent content) where T : class =>
        SendRequest<T>(() =>CreateRequest(HttpMethod.Post, requestUri, content, eTag: null));

    public ValueTask<ApiResponse<T>> Post<T>(string requestUri, object request) where T : class
        => SendJson<T>(HttpMethod.Post, requestUri, request);

    public async ValueTask<ApiResponse> Patch(string requestUri, object request) =>
        await SendJson<object>(HttpMethod.Patch, requestUri, request);

    public ValueTask<ApiResponse<T>> Patch<T>(string requestUri, object request) where T : class
        => SendJson<T>(HttpMethod.Patch, requestUri, request);

    public async ValueTask<ApiResponse> Delete(string requestUri, string? eTag) =>
        await SendJson<object>(HttpMethod.Delete, requestUri, value: null, eTag);

    public ValueTask<ApiResponse<T>> Delete<T>(string requestUri) =>
        SendJson<T>(HttpMethod.Delete, requestUri, value: null);

    async ValueTask<ApiResponse<T>> SendJson<T>(HttpMethod method, string requestUri, object? value, string? eTag = null)
    {
        using var content = value is not null ? JsonContent.Create(value, value.GetType(), mediaType: null, jsonSerializerOptions) : null;
        return await SendRequest<T>(() => CreateRequest(method, requestUri, content, eTag));
    }

    static HttpRequestMessage CreateRequest(HttpMethod method, string requestUri, HttpContent? content, string? eTag)
    {
        var request = new HttpRequestMessage(method, requestUri) { Content = content };
        request.SetBrowserRequestCache(BrowserRequestCache.NoStore);
        if (eTag is { Length: > 0 })
            request.Headers.IfMatch.Add(new(eTag));
        return request;
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "This method should not pass exceptions to callers.")]
    async ValueTask<ApiResponse<T>> SendRequest<T>(Func<HttpRequestMessage> requestFactory)
    {
        try
        {
            using var request = requestFactory();
            var accessToken = await PrepareRequest(request);
            var responseMessage = await httpClient.SendAsync(request);
            var shouldRetry = await HandleResponseRetry(responseMessage, accessToken);
            if (shouldRetry)
            {
                using var retryRequest = requestFactory();
                await PrepareRequest(retryRequest);
                responseMessage = await httpClient.SendAsync(retryRequest);
            }
            return await CreateResponse();

            async ValueTask<ApiResponse<T>> CreateResponse()
            {
                return responseMessage.IsSuccessStatusCode
                    ? await CreateSuccessResponseAsync()
                    : responseMessage.Content.Headers.ContentType?.MediaType is "application/problem+json"
                        ? new() { Problem = await responseMessage.Content.ReadFromJsonAsync<ProblemDetails>(jsonSerializerOptions) }
                        : CreateErrorResponse(responseMessage.StatusCode);

                async ValueTask<ApiResponse<T>> CreateSuccessResponseAsync() =>
                    responseMessage.Content.Headers.ContentLength is not 0
                        ? new() { Result = await responseMessage.Content.ReadFromJsonAsync<T>(jsonSerializerOptions) }
                        : new();

                static ApiResponse<T> CreateErrorResponse(HttpStatusCode status) => new()
                {
                    Problem = new()
                    {
                        Type = "about:blank",
                        Title = "Server Error",
                        Status = status,
                    },
                };
            }
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Unable to send the request");
            return new() { Problem = serverConnectionError with { Detail = exception.Message } };
        }
    }

    protected virtual ValueTask<string?> PrepareRequest(HttpRequestMessage request)
    {
        request.SetBrowserRequestCache(BrowserRequestCache.NoStore);
        return ValueTask.FromResult<string?>(null);
    }

    protected virtual ValueTask<bool> HandleResponseRetry(HttpResponseMessage response, string? previousAccessToken) =>
        ValueTask.FromResult(false);
}
