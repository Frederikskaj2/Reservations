using System;
using System.Net;

namespace Frederikskaj2.Reservations.Server;

public class Problem
{
    public Problem(string type, string title, HttpStatusCode status)
    {
        Type = type;
        Title = title;
        Status = status;
    }

    public string Type { get; }
    public string Title { get; }
    public HttpStatusCode Status { get; }

    public void Throw(string detail) => ThrowException(detail, null);
    public void Throw(Exception exception) => ThrowException(null, exception);
    public void Throw(string detail, Exception exception) => ThrowException(detail, exception);

    private void ThrowException(string? detail, Exception? innerException) => throw new ProblemException(Type, Title, Status, detail, innerException);
}