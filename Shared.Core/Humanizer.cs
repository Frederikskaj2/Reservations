using NodaTime;
using System.Globalization;
using System.Text;

namespace Frederikskaj2.Reservations.Shared.Core;

public static class Humanizer
{
    public static string FormatPeriod(Period period)
    {
        var stringBuilder = new StringBuilder();
        if (period.HasDateComponent)
        {
            stringBuilder.Append(CultureInfo.InvariantCulture, $"{period.Days} dag{(period.Days > 1 ? "e": "")}");
            if (period.HasTimeComponent)
                stringBuilder.Append(" og ");
        }
        if (period.HasTimeComponent)
            stringBuilder.Append(CultureInfo.InvariantCulture, $"{period.Hours} time{(period.Hours > 1 ? "r" : "")}");
        return stringBuilder.ToString();
    }
}
