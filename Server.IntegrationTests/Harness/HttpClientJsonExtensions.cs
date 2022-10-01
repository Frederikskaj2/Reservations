// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Harness;

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope",
    Justification = "Content should not be disposed until task is complete; furthermore content does not use unmanaged resources.")]
[SuppressMessage("Usage", "CA2234:Pass system uri objects instead of strings",
    Justification = "This code is a copy of Microsoft source with 'POST' replaced with 'PATCH' and there is no need to modify it based on this rule.")]
public static class HttpClientJsonExtensions
{
    public static Task<HttpResponseMessage> PatchAsJsonAsync<TValue>(
        this HttpClient client, string? requestUri, TValue value, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
    {
        if (client is null)
            throw new ArgumentNullException(nameof(client));

        var content = JsonContent.Create(value, null, options);
        return client.PatchAsync(requestUri, content, cancellationToken);
    }

    public static Task<HttpResponseMessage> PatchAsJsonAsync<TValue>(
        this HttpClient client, Uri? requestUri, TValue value, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
    {
        if (client is null)
            throw new ArgumentNullException(nameof(client));

        var content = JsonContent.Create(value, null, options);
        return client.PatchAsync(requestUri, content, cancellationToken);
    }

    public static Task<HttpResponseMessage> PatchAsJsonAsync<TValue>(
        this HttpClient client, string? requestUri, TValue value, CancellationToken cancellationToken)
        => client.PatchAsJsonAsync(requestUri, value, null, cancellationToken);

    public static Task<HttpResponseMessage> PatchAsJsonAsync<TValue>(
        this HttpClient client, Uri? requestUri, TValue value, CancellationToken cancellationToken)
        => client.PatchAsJsonAsync(requestUri, value, null, cancellationToken);
}