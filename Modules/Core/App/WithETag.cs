namespace Frederikskaj2.Reservations.Core;

public static class WithETag
{
    public static WithETag<T> Create<T>(T result, string eTag) => new(result, eTag);
}

public record WithETag<T>(T Result, string? ETag);
