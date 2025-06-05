using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Immutable;
using System.Globalization;

namespace Frederikskaj2.Reservations.Emails;

public record Picture(int Width, int Height, string MediaType, ImmutableArray<byte> Data)
{
    public MarkupString ToInlineHtml() =>
        new(
            string.Create(
                CultureInfo.InvariantCulture,
                $"""<img src="data:{MediaType};base64,{Convert.ToBase64String(Data.AsArrayUnsafe())}" width="{Width}" height="{Height}" />"""));
}
