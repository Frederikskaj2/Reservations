namespace Frederikskaj2.Reservations.Client;

public record ApiResponse
{
    public ProblemDetails? Problem { get; init; }

    public bool IsSuccess => Problem is null;
}

public record ApiResponse<T> : ApiResponse
{
    public T? Result { get; init; }
}