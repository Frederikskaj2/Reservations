using Azure.Storage.Queues;
using Frederikskaj2.Reservations.Users;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Harness;

public sealed class SessionFixture : IAsyncLifetime, IDisposable
{
    string? administratorAccessToken;
    ApplicationFactory? factory;

    public async Task InitializeAsync()
    {
        await SessionThrottle.StartSession();
        factory = new(GetId());
        await factory.Initialize();
    }

    public async Task DisposeAsync()
    {
        if (factory is null)
            return;
        await factory.DisposeAsync();
        SessionThrottle.EndSession();
    }

    internal User? User { get; set; }

    internal IEnumerable<string>? Cookies { get; set; }

    internal string? AccessToken { get; set; }

    internal JsonSerializerOptions SerializerOptions => factory?.JsonSerializerOptions ?? throw new InvalidOperationException();

    internal QueueClient QueueClient => factory?.QueueClient ?? throw new InvalidOperationException();

    internal LocalDate TestStartDate => factory?.TestStartDate ?? throw new InvalidOperationException();

    internal LocalDate CurrentDate => factory?.CurrentDate ?? throw new InvalidOperationException();

    internal Period NowOffset
    {
        get => factory?.NowOffset ?? throw new InvalidOperationException();
        set
        {
            if (factory is null)
                throw new InvalidOperationException();
            factory.NowOffset = value;
        }
    }

    internal DateTimeZone TimeZone { get; } = DateTimeZoneProviders.Tzdb.GetZoneOrNull("Europe/Copenhagen")!;

    internal Calendar Calendar { get; } = new();

    void IDisposable.Dispose() => factory?.Dispose();

    internal IServiceScope CreateServiceScope() =>
        factory?.Services.CreateScope() ?? throw new InvalidOperationException();

    internal ValueTask<HttpResponseMessage> Get(string path)
    {
        if (AccessToken is null)
            throw new InvalidOperationException();
        return Get(path, AccessToken);
    }

    internal async ValueTask<HttpResponseMessage> AdministratorGet(string path)
    {
        await EnsureAdministratorIsSignedIn();
        return await Get(path, administratorAccessToken);
    }

    internal async ValueTask<HttpResponseMessage> PostAnonymous<T>(string path, T request)
    {
        using var client = CreateClient();
        return await client.PostAsJsonAsync(new Uri(path, UriKind.Relative), request, SerializerOptions);
    }

    internal ValueTask<HttpResponseMessage> Post(string path)
    {
        if (AccessToken is null)
            throw new InvalidOperationException();
        return Post<object?>(path, request: null, AccessToken, Cookies);
    }

    internal ValueTask<HttpResponseMessage> Post(string path, IEnumerable<string>? cookies)
    {
        if (AccessToken is null)
            throw new InvalidOperationException();
        return Post<object?>(path, request: null, AccessToken, cookies);
    }

    internal ValueTask<HttpResponseMessage> Post<T>(string path, T request)
    {
        if (AccessToken is null)
            throw new InvalidOperationException();
        return Post(path, request, AccessToken, Cookies);
    }

    internal async ValueTask<HttpResponseMessage> AdministratorPost(string path)
    {
        await EnsureAdministratorIsSignedIn();
        return await Post<object?>(path, request: null, administratorAccessToken);
    }

    internal async ValueTask<HttpResponseMessage> AdministratorPost<T>(string path, T request)
    {
        await EnsureAdministratorIsSignedIn();
        return await Post(path, request, administratorAccessToken);
    }

    internal async ValueTask<HttpResponseMessage> AdministratorPostContent(string path, HttpContent content)
    {
        await EnsureAdministratorIsSignedIn();
        return await PostContent(path, content, administratorAccessToken);
    }

    internal async ValueTask<HttpResponseMessage> Patch<T>(string path, T request)
    {
        if (AccessToken is null)
            throw new InvalidOperationException();
        using var client = CreateClient(AccessToken);
        return await client.PatchAsJsonAsync(new(path, UriKind.Relative), request, SerializerOptions);
    }

    internal async ValueTask<HttpResponseMessage> AdministratorPatch<T>(string path, T request)
    {
        await EnsureAdministratorIsSignedIn();
        using var client = CreateClient(administratorAccessToken);
        return await client.PatchAsJsonAsync(new(path, UriKind.Relative), request, SerializerOptions);
    }

    internal async ValueTask<HttpResponseMessage> Delete(string path)
    {
        if (AccessToken is null)
            throw new InvalidOperationException();
        using var client = CreateClient(AccessToken);
        return await client.DeleteAsync(new Uri(path, UriKind.Relative));
    }

    internal async ValueTask<HttpResponseMessage> AdministratorDelete(string path, string? eTag = null)
    {
        await EnsureAdministratorIsSignedIn();
        using var client = CreateClient(administratorAccessToken);
        if (eTag is { Length: > 0 })
            client.DefaultRequestHeaders.IfMatch.Add(new(eTag));
        return await client.DeleteAsync(new Uri(path, UriKind.Relative));
    }

    internal async ValueTask<T> Deserialize<T>(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<T>(SerializerOptions))!;
    }

    internal ValueTask<T?> Deserialize<T>(Stream stream) => JsonSerializer.DeserializeAsync<T>(stream, SerializerOptions);

    async ValueTask<HttpResponseMessage> Get(string path, string? accessToken)
    {
        using var client = CreateClient(accessToken);
        return await client.GetAsync(new Uri(path, UriKind.Relative));
    }
    async ValueTask<HttpResponseMessage> Post<T>(string path, T request, string? accessToken, IEnumerable<string>? cookies = null)
    {
        using var client = CreateClient(accessToken, cookies);
        return request is not null
            ? await client.PostAsJsonAsync(new Uri(path, UriKind.Relative), request, SerializerOptions)
            : await client.PostAsync(new Uri(path, UriKind.Relative), content: null);
    }

    async ValueTask<HttpResponseMessage> PostContent(string path, HttpContent content, string? accessToken, IEnumerable<string>? cookies = null)
    {
        using var client = CreateClient(accessToken, cookies);
        return await client.PostAsync(new Uri(path, UriKind.Relative), content);
    }

    async ValueTask EnsureAdministratorIsSignedIn()
    {
        if (administratorAccessToken is not null)
            return;
        var request = new SignInRequest(SeedData.AdministratorEmail, SeedData.AdministratorPassword, IsPersistent: false);
        var response = await Deserialize<TokensResponse>(await PostAnonymous("user/sign-in", request));
        administratorAccessToken = response.AccessToken;
    }

    HttpClient CreateClient(string? bearerToken = null, IEnumerable<string>? cookies = null)
    {
        var client = factory?.CreateClient() ?? throw new InvalidOperationException();
        client.DefaultRequestHeaders.Accept.Add(new(MediaTypeNames.Application.Json));
        if (bearerToken is not null)
            client.DefaultRequestHeaders.Authorization = new("Bearer", bearerToken);
        if (cookies is not null)
            client.DefaultRequestHeaders.Add("Cookie", cookies);
        return client;
    }

    static string GetId() => Random.Shared.Next().ToString("x8", CultureInfo.InvariantCulture);
}
