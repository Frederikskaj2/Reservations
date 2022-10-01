using Frederikskaj2.Reservations.Shared.Core;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Pages;

public partial class IndexPage
{
    bool isInitialized;
    OrderingOptions? options;

    [Inject] public ClientDataProvider ClientDataProvider { get; set; } = null!;
    [Inject] public Formatter Formatter { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        options = await ClientDataProvider.GetOptionsAsync();
        isInitialized = true;
    }

    string FormatPrice(Amount price) => Formatter.FormatMoneyShort(price);

    string FormatPrice(Amount highPrice, Amount lowPrice) =>
        highPrice != lowPrice
            ? $"{Formatter.FormatMoneyShort(highPrice)} / {Formatter.FormatMoneyShort(lowPrice)}"
            : Formatter.FormatMoneyShort(highPrice);
}
