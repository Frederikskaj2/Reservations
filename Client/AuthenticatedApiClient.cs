using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Shared.Web;
using Microsoft.Extensions.Logging;

namespace Frederikskaj2.Reservations.Client;

public class AuthenticatedApiClient : ApiClient
{
    readonly ApiClient apiClient;
    readonly AuthenticationService authenticationService;
    readonly EventAggregator eventAggregator;
    readonly ILogger logger;

    public AuthenticatedApiClient(
        ApiClient apiClient, AuthenticationService authenticationService, EventAggregator eventAggregator, HttpClient httpClient,
        JsonSerializerOptions jsonSerializerOptions, ILogger<AuthenticatedApiClient> logger)
        : base(eventAggregator, httpClient, jsonSerializerOptions, logger)
    {
        this.apiClient = apiClient;
        this.authenticationService = authenticationService;
        this.eventAggregator = eventAggregator;
        this.logger = logger;
    }

    protected override async ValueTask<HttpRequestMessage> CreateRequestAsync(HttpMethod method, string requestUri, HttpContent? content)
    {
        var request = await base.CreateRequestAsync(method, requestUri, content);
        var accessToken = await authenticationService.GetAccessTokenAsync();
        if (!string.IsNullOrEmpty(accessToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("bearer", accessToken);
        return request;
    }

    protected override async ValueTask<bool> HandleResponseRetryAsync(HttpResponseMessage response)
    {
        if (response.StatusCode != HttpStatusCode.Unauthorized)
            return false;
        var isSuccess = await CreateAccessTokenAsync();
        logger.LogDebug("Request was not authenticated; new access token created with success = {Success}", isSuccess);
        if (isSuccess)
            return true;
        await authenticationService.ClearAsync();
        eventAggregator.Publish(SignOutMessage.Instance);
        return false;
    }

    async ValueTask<bool> CreateAccessTokenAsync()
    {
        var response = await apiClient.PostAsync<Tokens>("user/create-access-token");
        if (response.Result is null)
            return false;
        await authenticationService.SetTokensAsync(response.Result!);
        return true;
    }
}
