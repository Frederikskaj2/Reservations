namespace Frederikskaj2.Reservations.Shared.Web;

public class ErrorResponse
{
    public string? Details { get; init; }
}

public class ErrorResponse<TError> where TError : struct
{
    public TError? Error { get; init; }
    public string? Details { get; init; }
}