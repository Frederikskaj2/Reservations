using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Frederikskaj2.Reservations.Client;

static class Url
{
    public static string FormatQuery(IEnumerable<(string Name, string Value)> parameters)
    {
        var stringBuilder = new StringBuilder();
        var delimiter = '?';
        foreach (var (name, value) in parameters)
        {
            stringBuilder.Append(CultureInfo.InvariantCulture, $"{delimiter}{Uri.EscapeDataString(name)}={Uri.EscapeDataString(value)}");
            delimiter = '&';
        }
        return stringBuilder.ToString();
    }
}
