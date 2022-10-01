using System;
using System.Collections.Immutable;

namespace Frederikskaj2.Reservations.EmailSender;

public record Picture(int Width, int Height, string MediaType, ImmutableArray<byte> Data)
{
    public string ToInlineHtml() =>
        $@"<img src=""data:{MediaType};base64,{Convert.ToBase64String(Data.AsArrayUnsafe())}"" width=""{Width}"" height=""{Height}"" />";
}
