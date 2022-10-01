using Liversage.Primitives;
using System;
using System.Text;

namespace Frederikskaj2.Reservations.Shared.Core;

[Primitive(StringComparison = StringComparison.OrdinalIgnoreCase)]
public readonly partial struct EmailAddress
{
    readonly string emailAddress;

    public static string NormalizeEmail(EmailAddress email) => email.ToString().Normalize(NormalizationForm.FormKC).ToUpperInvariant();

    public static readonly EmailAddress Deleted = new("slettet@example.com");
}
