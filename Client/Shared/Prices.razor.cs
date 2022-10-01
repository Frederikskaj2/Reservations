using Frederikskaj2.Reservations.Shared.Core;
using Microsoft.AspNetCore.Components;

namespace Frederikskaj2.Reservations.Client.Shared;

public partial class Prices
{
    [Inject] public Formatter Formatter { get; set; } = null!;

    [Parameter] public Price? Price { get; set; }

    string GetRentAndCleaning() => Price is not null ? Formatter.FormatMoneyShort(Price.Rent + Price.Cleaning) : "";

    string GetDeposit() => Price is not null ? Formatter.FormatMoneyShort(Price.Deposit) : "";

    string GetTotal() => Price is not null ? Formatter.FormatMoneyShort(Price.Total()) : "";
}
