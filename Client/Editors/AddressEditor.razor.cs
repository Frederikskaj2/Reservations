using Blazorise;
using Frederikskaj2.Reservations.Shared.Core;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Editors;

public partial class AddressEditor
{
    const char noLetter = '-';
    const int noApartmentId = -1;

    Dictionary<char, List<Apartment>>? apartments;
    int selectedApartmentId;
    char selectedLetter = noLetter;

    [Inject] public ClientDataProvider DataProvider { get; set; } = null!;

    [Parameter] public bool IsRequired { get; set; }
    [Parameter] public bool Disabled { get; set; }

    [Parameter]
    public ApartmentId? Value
    {
        get => selectedApartmentId != noApartmentId ? selectedApartmentId : null;
        set => selectedApartmentId = value?.ToInt32() ?? noApartmentId;
    }

    [Parameter] public EventCallback<ApartmentId?> ValueChanged { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var apartments = await DataProvider.GetApartmentsAsync();
        this.apartments = apartments!.GroupBy(apartment => apartment.Letter).ToDictionary(grouping => grouping.Key, grouping => grouping.ToList());
    }

    protected override void OnParametersSet()
    {
        if (!Value.HasValue)
            return;
        var apartment = apartments!.Values.SelectMany(list => list).FirstOrDefault(a => a.ApartmentId == Value.Value);
        selectedLetter = apartment?.Letter ?? noLetter;
        selectedApartmentId = apartment?.ApartmentId.ToInt32() ?? noApartmentId;
    }

    Task LetterChangedAsync(char letter)
    {
        selectedLetter = letter;
        if (selectedLetter == noLetter)
            return SetApartmentIdAsync(noApartmentId);
        var apartmentsForLetter = apartments![selectedLetter];
        var apartmentId = apartmentsForLetter.Count == 1 ? apartmentsForLetter[0].ApartmentId.ToInt32() : noApartmentId;
        return SetApartmentIdAsync(apartmentId);
    }

    Task SetApartmentIdAsync(int apartmentId)
    {
        if (apartmentId == selectedApartmentId)
            return Task.CompletedTask;
        selectedApartmentId = apartmentId;
        return ValueChanged.InvokeAsync(Value);
    }

    void Validate(ValidatorEventArgs e)
    {
        if (!IsRequired)
        {
            e.Status = ValidationStatus.Success;
            return;
        }

        var apartmentId = Convert.ToInt32(e.Value, CultureInfo.InvariantCulture);
        if (apartmentId != noApartmentId)
        {
            e.Status = ValidationStatus.Success;
            return;
        }

        e.Status = ValidationStatus.Error;
        e.ErrorText = "Du skal oplyse din adresse";
    }
}
