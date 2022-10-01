using System.Globalization;

namespace Frederikskaj2.Reservations.Client;

static class StringExtensions
{
    public static string Capitalize(this string text, CultureInfo cultureInfo) => cultureInfo.TextInfo.ToUpper(text[0]) + text.Substring(1);
}