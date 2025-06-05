using Frederikskaj2.Reservations.Core;
using Liversage.Primitives;
using System;
using System.Text;

namespace Frederikskaj2.Reservations.Users;

[Primitive(StringComparison = StringComparison.OrdinalIgnoreCase)]
public readonly partial struct EmailAddress : IIsId
{
    readonly string emailAddress;

    public string GetId() => NormalizeEmail(emailAddress);

    public static string NormalizeEmail(EmailAddress email) => email.ToString()!.Normalize(NormalizationForm.FormKC).ToUpperInvariant();

    public static readonly EmailAddress Deleted = new("slettet@example.com");
}
