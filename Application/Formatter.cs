using NodaTime;
using NodaTime.Text;
using System.Globalization;

namespace Frederikskaj2.Reservations.Application;

class Formatter : IFormatter
{
    readonly LocalDatePattern shortDatePattern;

    public Formatter(CultureInfo cultureInfo) => shortDatePattern = LocalDatePattern.Create(@"d'-'M'-'yyyy", cultureInfo);

    public string FormatDateShort(LocalDate date) => shortDatePattern.Format(date);
}
