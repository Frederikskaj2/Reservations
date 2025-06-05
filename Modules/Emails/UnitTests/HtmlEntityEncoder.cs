using System.IO;
using System.Text.Encodings.Web;

namespace Frederikskaj2.Reservations.Emails.UnitTests;

static class HtmlEntityEncoder
{
    public static string HtmlEncode(string @string)
    {
        using var stringWriter = new StringWriter();
        HtmlEncoder.Default.Encode(stringWriter, @string, 0, @string.Length);
        return stringWriter.ToString();
    }
}
