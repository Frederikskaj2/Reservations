using System;
using System.Net;

namespace Frederikskaj2.Reservations.Client;

public record ProblemDetails
{
    public string? Type { get; init; }
    public string? Title { get; init; }
    public HttpStatusCode Status { get; init; }
    public string? Detail { get; init; }
    public string? Error { get; init; }

    public T GetError<T>() where T : struct, Enum =>
        Error is not null && Enum.TryParse<T>(Error, out var value) ? value : default;
}
