using Blazorise;
using Frederikskaj2.Reservations.Users;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client.Modules.Users;

partial class AddressEditor
{
    const char noLetter = '-';
    const int noApartmentId = -1;

    Dictionary<char, List<Apartment>>? apartmentsByLetter;
    int selectedApartmentId;
    char selectedLetter = noLetter;

    [Inject] public ClientDataProvider DataProvider { get; set; } = null!;

    [Parameter] public bool IsRequired { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public ApartmentId? Value { get; set; }
    [Parameter] public EventCallback<ApartmentId?> ValueChanged { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var apartments = await DataProvider.GetApartments();
        apartmentsByLetter = apartments.Values.GroupBy(apartment => apartment.Letter).ToDictionary(grouping => grouping.Key, grouping => grouping.ToList());
    }

    protected override void OnParametersSet()
    {
        if (Value.HasValue)
        {
            var apartment = apartmentsByLetter!.Values.SelectMany(list => list).FirstOrDefault(a => a.ApartmentId == Value.Value);
            selectedLetter = apartment?.Letter ?? noLetter;
            selectedApartmentId = apartment?.ApartmentId.ToInt32() ?? noApartmentId;
        }
        else
        {
            selectedLetter = noLetter;
            selectedApartmentId = noApartmentId;
        }
    }

    Task LetterChanged(char letter)
    {
        selectedLetter = letter;
        if (selectedLetter is noLetter)
            return SetApartmentId(noApartmentId);
        var apartmentsForLetter = apartmentsByLetter![selectedLetter];
        var apartmentId = apartmentsForLetter.Count is 1 ? apartmentsForLetter[0].ApartmentId.ToInt32() : noApartmentId;
        return SetApartmentId(apartmentId);
    }

    Task SetApartmentId(int apartmentId)
    {
        if (apartmentId == selectedApartmentId)
            return Task.CompletedTask;
        selectedApartmentId = apartmentId;
        Value = selectedApartmentId is not noApartmentId ? ApartmentId.FromInt32(selectedApartmentId) : null;
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
        if (apartmentId is not noApartmentId)
        {
            e.Status = ValidationStatus.Success;
            return;
        }

        e.Status = ValidationStatus.Error;
        e.ErrorText = "Du skal oplyse din adresse";
    }
}
