using System;

namespace Frederikskaj2.Reservations.Emails;

public class EmailsOptions
{
    public Uri BaseUrl { get; init; } = new("https://lokaler.frederikskaj2.dk/");
}
