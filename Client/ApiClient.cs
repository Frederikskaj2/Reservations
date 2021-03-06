﻿using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client
{
    public class ApiClient
    {
        private static readonly ProblemDetails internalServerError = new ProblemDetails
        {
            Type = "about:blank",
            Title = "Internal Server Error",
            Status = 500
        };

        private readonly HttpClient HttpClient;
        private readonly JsonSerializerOptions JsonSerializerOptions;

        public ApiClient(HttpClient httpClient, JsonSerializerOptions jsonSerializerOptions)
        {
            HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            JsonSerializerOptions = jsonSerializerOptions ?? throw new ArgumentNullException(nameof(jsonSerializerOptions));
        }

        public Task<(TResponse? Response, ProblemDetails? Problem)> Get<TResponse>(string requestUri)
            where TResponse : class
        {
            if (requestUri is null)
                throw new ArgumentNullException(nameof(requestUri));

            return SendJsonAsync<TResponse>(HttpMethod.Get, requestUri, null);
        }

        public async Task<ProblemDetails?> Post(string requestUri)
        {
            if (requestUri is null)
                throw new ArgumentNullException(nameof(requestUri));

            var (_, problem) = await SendJsonAsync<object>(HttpMethod.Post, requestUri, null);
            return problem;
        }

        public async Task<ProblemDetails?> Post(string requestUri, object request)
        {
            if (requestUri is null)
                throw new ArgumentNullException(nameof(requestUri));
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var (_, problem) = await SendJsonAsync<object>(HttpMethod.Post, requestUri, request);
            return problem;
        }

        public Task<(T? Response, ProblemDetails? Problem)> Post<T>(string requestUri) where T : class
        {
            if (requestUri is null)
                throw new ArgumentNullException(nameof(requestUri));

            return SendJsonAsync<T>(HttpMethod.Post, requestUri, null);
        }

        public Task<(T? Response, ProblemDetails? Problem)> Post<T>(string requestUri, object request)
            where T : class
        {
            if (requestUri is null)
                throw new ArgumentNullException(nameof(requestUri));
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            return SendJsonAsync<T>(HttpMethod.Post, requestUri, request);
        }

        public async Task<ProblemDetails?> Patch(string requestUri, object request)
        {
            if (requestUri is null)
                throw new ArgumentNullException(nameof(requestUri));
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var (_, problem) = await SendJsonAsync<object>(HttpMethod.Patch, requestUri, request);
            return problem;
        }

        public Task<(T? Response, ProblemDetails? Problem)> Patch<T>(string requestUri, object request)
            where T : class
        {
            if (requestUri is null)
                throw new ArgumentNullException(nameof(requestUri));
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            return SendJsonAsync<T>(HttpMethod.Patch, requestUri, request);
        }

        private async Task<(TResponse? Response, ProblemDetails? Problem)> SendJsonAsync<TResponse>(HttpMethod method, string requestUri, object? value)
             where TResponse : class
        {
            using var request = new HttpRequestMessage(method, requestUri)
            {
                Content = CreateContent()
            };
            var response = await HttpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
                return (await CreateResponse(), null);
            else if (response.Content.Headers.ContentType.MediaType == "application/problem+json")
                return (null, await response.Content.ReadFromJsonAsync<ProblemDetails>());
            else
                return (null, internalServerError);

            HttpContent? CreateContent()
            {
                if (value == null)
                    return null;
                return JsonContent.Create(value, value.GetType(), null, JsonSerializerOptions);
            }

            async Task<TResponse?> CreateResponse()
            {
                if (response!.Content.Headers.ContentLength == 0)
                    return null;
                return await response.Content.ReadFromJsonAsync<TResponse>(JsonSerializerOptions);
            }
        }
    }
}
