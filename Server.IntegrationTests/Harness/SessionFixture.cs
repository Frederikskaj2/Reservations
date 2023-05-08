using Azure.Storage.Queues;
using Frederikskaj2.Reservations.Shared.Web;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Harness;

public sealed class SessionFixture : IAsyncLifetime, IDisposable
{
    Tokens? administratorTokens;
    ApplicationFactory? factory;

    public async Task InitializeAsync()
    {
        await SessionThrottle.StartSessionAsync();
        factory = new ApplicationFactory(GetId());
        await factory.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        if (factory is null)
            return;
        await factory.DisposeAsync();
        SessionThrottle.EndSession();
    }

    public User? User { get; set; }

    public IEnumerable<string>? Cookies { get; set; }

    public Tokens? Tokens { get; set; }

    public JsonSerializerOptions SerializerOptions => factory?.JsonSerializerOptions ?? throw new InvalidOperationException();

    public QueueClient QueueClient => factory?.QueueClient ?? throw new InvalidOperationException();

    public LocalDate TestStartDate => factory?.TestStartDate ?? throw new InvalidOperationException();

    public LocalDate CurrentDate => factory?.CurrentDate ?? throw new InvalidOperationException();

    public Period NowOffset
    {
        get => factory?.NowOffset ?? throw new InvalidOperationException();
        set
        {
            if (factory is null)
                throw new InvalidOperationException();
            factory.NowOffset = value;
        }
    }

    public Calendar Calendar { get; } = new();

    void IDisposable.Dispose() => factory?.Dispose();

    public IServiceScope CreateServiceScope() =>
        factory?.Services.CreateScope() ?? throw new InvalidOperationException();

    public async ValueTask<HttpResponseMessage> AnonymousGetAsync(string path)
    {
        using var client = factory?.CreateClient() ?? throw new InvalidOperationException();
        return await client.GetAsync(new Uri(path, UriKind.Relative));
    }

    public ValueTask<HttpResponseMessage> GetAsync(string path)
    {
        if (Tokens is null)
            throw new InvalidOperationException();
        return GetAsync(path, Tokens);
    }

    public async ValueTask<HttpResponseMessage> AdministratorGetAsync(string path)
    {
        await EnsureAdministratorIsSignedInAsync();
        return await GetAsync(path, administratorTokens!);
    }

    public async ValueTask<HttpResponseMessage> PostAnonymousAsync<T>(string path, T request)
    {
        using var client = factory?.CreateClient() ?? throw new InvalidOperationException();
        return await client.PostAsJsonAsync(new Uri(path, UriKind.Relative), request, SerializerOptions);
    }

    public async ValueTask<HttpResponseMessage> PostAsync<T>(string path, T request)
    {
        if (Tokens is null)
            throw new InvalidOperationException();
        return await PostAsync(path, request, Tokens, Cookies);
    }

    public async ValueTask<HttpResponseMessage> AdministratorPostAsync<T>(string path, T request)
    {
        await EnsureAdministratorIsSignedInAsync();
        return await PostAsync(path, request, administratorTokens!);
    }

    public async ValueTask<HttpResponseMessage> PatchAsync<T>(string path, T request)
    {
        if (Tokens is null)
            throw new InvalidOperationException();
        using var client = factory?.CreateClient() ?? throw new InvalidOperationException();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Tokens.AccessToken);
        return await client.PatchAsJsonAsync(new Uri(path, UriKind.Relative), request, SerializerOptions);
    }

    public async ValueTask<HttpResponseMessage> AdministratorPatchAsync<T>(string path, T request)
    {
        await EnsureAdministratorIsSignedInAsync();
        using var client = factory?.CreateClient() ?? throw new InvalidOperationException();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", administratorTokens!.AccessToken);
        return await client.PatchAsJsonAsync(new Uri(path, UriKind.Relative), request, SerializerOptions);
    }

    public async ValueTask<HttpResponseMessage> DeleteAsync(string path)
    {
        if (Tokens is null)
            throw new InvalidOperationException();
        using var client = factory?.CreateClient() ?? throw new InvalidOperationException();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Tokens.AccessToken);
        return await client.DeleteAsync(new Uri(path, UriKind.Relative));
    }

    public async ValueTask<T> DeserializeAsync<T>(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<T>(SerializerOptions))!;
    }

    public ValueTask<T?> DeserializeAsync<T>(Stream stream) => JsonSerializer.DeserializeAsync<T>(stream, SerializerOptions);

    async ValueTask<HttpResponseMessage> GetAsync(string path, Tokens tokens)
    {
        using var client = factory?.CreateClient() ?? throw new InvalidOperationException();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        return await client.GetAsync(new Uri(path, UriKind.Relative));
    }

    async ValueTask<HttpResponseMessage> PostAsync<T>(string path, T request, Tokens tokens, IEnumerable<string>? cookies = default)
    {
        using var client = factory?.CreateClient() ?? throw new InvalidOperationException();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        if (cookies is not null)
            client.DefaultRequestHeaders.Add("Cookie", cookies);
        return await client.PostAsJsonAsync(new Uri(path, UriKind.Relative), request, SerializerOptions);
    }

    async ValueTask EnsureAdministratorIsSignedInAsync()
    {
        if (administratorTokens is not null)
            return;
        var request = new SignInRequest { Email = TestData.AdministratorEmail, Password = TestData.AdministratorPassword };
        administratorTokens = await DeserializeAsync<Tokens>(await PostAnonymousAsync("user/sign-in", request));
    }

    static string GetId() => Random.Shared.Next().ToString("x8", CultureInfo.InvariantCulture);
}
