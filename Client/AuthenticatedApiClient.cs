using Frederikskaj2.Reservations.Users;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client;

public sealed class AuthenticatedApiClient(
    ApiClient apiClient,
    AuthenticationService authenticationService,
    EventAggregator eventAggregator,
    HttpClient httpClient,
    JsonSerializerOptions jsonSerializerOptions,
    ILogger<AuthenticatedApiClient> logger)
    : ApiClient(eventAggregator, httpClient, jsonSerializerOptions, logger), IDisposable
{
    readonly SemaphoreSlim gate = new(1, 1);

    public void Dispose() => gate.Dispose();

    protected override async ValueTask<string?> PrepareRequest(HttpRequestMessage request)
    {
        await base.PrepareRequest(request);
        var accessToken = await authenticationService.GetAccessToken();
        if (accessToken is not { Length: > 0 })
            return null;
        request.Headers.Authorization = new("bearer", accessToken);
        return accessToken;
    }

    protected override async ValueTask<bool> HandleResponseRetry(HttpResponseMessage response, string? previousAccessToken)
    {
        if (response.StatusCode != HttpStatusCode.Unauthorized)
            return false;
        await gate.WaitAsync();
        try
        {
            var currentAccessToken = await authenticationService.GetAccessToken();
            if (currentAccessToken != previousAccessToken)
                return true;
            var isSuccess = await CreateAccessToken();
            logger.LogDebug("The request was not authenticated; a new access token was created with success = {Success}", isSuccess);
            if (isSuccess)
                return true;
            await authenticationService.Clear();
            EventAggregator.Publish(SignOutMessage.Instance);
            return false;
        }
        finally
        {
            gate.Release();
        }
    }

    async ValueTask<bool> CreateAccessToken()
    {
        var response = await apiClient.Post<Tokens>("user/create-access-token");
        if (response.Result is null)
            return false;
        await authenticationService.SetTokens(response.Result!);
        return true;
    }
}
