using Blazorise;
using Frederikskaj2.Reservations.Bank;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Users;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Modules.Bank;

partial class FindResidentDialog
{
    IReadOnlyDictionary<ApartmentId, Apartment>? apartments;
    Modal modal = null!;
    List<ResidentDto>? residents;
    ResidentDto? selectedResident;
    BankTransactionDto? selectedTransaction;
    SortColumn sortColumn;
    SortDirection sortDirection;

    [Inject] public ClientDataProvider ClientDataProvider { get; set; } = null!;

    [Parameter] public EventCallback<(BankTransactionDto, ResidentDto)> OnSelectResident { get; set; }
    [Parameter] public IEnumerable<ResidentDto>? Residents { get; set; }

    protected override async Task OnInitializedAsync() => apartments = await ClientDataProvider.GetApartments();

    protected override void OnParametersSet() => SortResidents();

    public Task Show(BankTransactionDto transaction)
    {
        selectedTransaction = transaction;
        selectedResident = null;
        return modal.Show();
    }

    void SortResidents()
    {
        if (Residents is null)
        {
            residents = null;
            return;
        }

        residents = sortDirection is SortDirection.Ascending
            ? Residents.OrderBy(GetKeySelector()).ToList()
            : Residents.OrderByDescending(GetKeySelector()).ToList();

        Func<ResidentDto, object> GetKeySelector() =>
            sortColumn switch
            {
                SortColumn.PaymentId => resident => resident.PaymentId.ToString(),
                SortColumn.Name => resident => resident.UserIdentity.FullName,
                SortColumn.Address => resident => resident.UserIdentity.ApartmentId!.Value.ToInt32(),
                SortColumn.Balance => resident => resident.Balance.ToDecimal(),
                _ => throw new UnreachableException(),
            };
    }

    void Sort(SortColumn column)
    {
        if (column == sortColumn)
            sortDirection = sortDirection is SortDirection.Ascending ? SortDirection.Descending : SortDirection.Ascending;
        else
        {
            sortColumn = column;
            sortDirection = SortDirection.Ascending;
        }
        SortResidents();
    }

    void SelectResident(ResidentDto resident) => selectedResident = resident;

    Task Cancel() => modal.Hide();

    async Task Confirm()
    {
        await modal.Hide();
        await OnSelectResident.InvokeAsync((selectedTransaction!, selectedResident!));
    }

    enum SortColumn
    {
        PaymentId,
        Name,
        Address,
        Balance,
    }

    enum SortDirection
    {
        Ascending,
        Descending,
    }
}
