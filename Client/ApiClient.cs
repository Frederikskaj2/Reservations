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

public class ApiClient
{
    static readonly ProblemDetails serverConnectionError = new()
    {
        Type = "about:blank",
        Title = "Server Connection Error",
        Status = HttpStatusCode.InternalServerError
    };

    readonly EventAggregator eventAggregator;
    readonly HttpClient httpClient;
    readonly JsonSerializerOptions jsonSerializerOptions;
    readonly ILogger logger;

    public ApiClient(EventAggregator eventAggregator, HttpClient httpClient, JsonSerializerOptions jsonSerializerOptions, ILogger<ApiClient> logger)
    {
        this.eventAggregator = eventAggregator;
        this.httpClient = httpClient;
        this.jsonSerializerOptions = jsonSerializerOptions;
        this.logger = logger;
    }

    public async ValueTask<ApiResponse<T>> GetAsync<T>(string requestUri) where T : class
    {
        var response = await SendJsonAsync<T>(HttpMethod.Get, requestUri, null);
        if (!response.IsSuccess && (int) response.Problem!.Status >= 500)
            eventAggregator.Publish(ServerStatusMessage.Down);
        return response;
    }

    public async ValueTask<ApiResponse> PostAsync(string requestUri) =>
        await SendJsonAsync<object>(HttpMethod.Post, requestUri, null);

    public async ValueTask<ApiResponse> PostAsync(string requestUri, object request) =>
        await SendJsonAsync<object>(HttpMethod.Post, requestUri, request);

    public ValueTask<ApiResponse<T>> PostAsync<T>(string requestUri)
        => SendJsonAsync<T>(HttpMethod.Post, requestUri, null);

    public ValueTask<ApiResponse<T>> PostAsync<T>(string requestUri, object request) where T : class
        => SendJsonAsync<T>(HttpMethod.Post, requestUri, request);

    public async ValueTask<ApiResponse> PatchAsync(string requestUri, object request) =>
        await SendJsonAsync<object>(HttpMethod.Patch, requestUri, request);

    public ValueTask<ApiResponse<T>> PatchAsync<T>(string requestUri, object request) where T : class
        => SendJsonAsync<T>(HttpMethod.Patch, requestUri, request);

    public async ValueTask<ApiResponse> DeleteAsync(string requestUri) =>
        await SendJsonAsync<object>(HttpMethod.Delete, requestUri, null);

    public async ValueTask<ApiResponse<T>> DeleteAsync<T>(string requestUri) =>
        await SendJsonAsync<T>(HttpMethod.Delete, requestUri, null);

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "This method should not pass exceptions to callers.")]
    async ValueTask<ApiResponse<T>> SendJsonAsync<T>(HttpMethod method, string requestUri, object? value)
    {
        try
        {
            var responseMessage = await SendAsync();
            var shouldRetry = await HandleResponseRetryAsync(responseMessage);
            if (shouldRetry)
                responseMessage = await SendAsync();
            var apiResponse = await CreateResponseAsync();
            return apiResponse;

            HttpContent? CreateContent() => value is not null ? JsonContent.Create(value, value.GetType(), null, jsonSerializerOptions) : null;

            async ValueTask<HttpResponseMessage> SendAsync()
            {
                // Content is created here to work around the
                // problem where the HttpClient will dispose
                // it. This seems to be fixed in .NET but
                // apparently not the Blazor HttpClient. When
                // retries are made content has to be created
                // twice.
                using var content = CreateContent();
                using var request = await CreateRequestAsync(method, requestUri, content);
                return await httpClient.SendAsync(request);
            }

            async ValueTask<ApiResponse<T>> CreateResponseAsync()
            {
                if (responseMessage.IsSuccessStatusCode)
                    return await CreateSuccessResponseAsync();
                if (responseMessage.Content.Headers.ContentType?.MediaType is "application/problem+json")
                    return new ApiResponse<T> { Problem = await responseMessage.Content.ReadFromJsonAsync<ProblemDetails>(jsonSerializerOptions) };
                return CreateErrorResponse(responseMessage.StatusCode);

                async ValueTask<ApiResponse<T>> CreateSuccessResponseAsync() =>
                    responseMessage.Content.Headers.ContentLength is not 0
                        ? new ApiResponse<T> { Result = await responseMessage.Content.ReadFromJsonAsync<T>(jsonSerializerOptions) }
                        : new ApiResponse<T>();

                static ApiResponse<T> CreateErrorResponse(HttpStatusCode status) => new()
                {
                    Problem = new ProblemDetails
                    {
                        Type = "about:blank",
                        Title = "Server Error",
                        Status = status
                    }
                };
            }
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Unable to send request");
            return new ApiResponse<T> { Problem = serverConnectionError with { Detail = exception.Message } };
        }
    }

    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "The caller will dispose the object.")]
    protected virtual ValueTask<HttpRequestMessage> CreateRequestAsync(HttpMethod method, string requestUri, HttpContent? content)
    {
        var request = new HttpRequestMessage(method, requestUri) { Content = content };
        request.SetBrowserRequestCache(BrowserRequestCache.NoStore);
        return ValueTask.FromResult(request);
    }

    protected virtual ValueTask<bool> HandleResponseRetryAsync(HttpResponseMessage response) => ValueTask.FromResult(false);
}
