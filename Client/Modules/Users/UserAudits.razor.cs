using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Users;
using Microsoft.AspNetCore.Components;
using System;
using System.Linq;
using System.Threading;

namespace Frederikskaj2.Reservations.Client.Modules.Users;

public sealed partial class UserAudits : IDisposable
{
    Timer? timer;

    [Inject] public IDateProvider DateProvider { get; set; } = null!;
    [Inject] public Formatter Formatter { get; set; } = null!;
    [Inject] public ITimeConverter TimeConverter { get; set; } = null!;

    [Parameter] public UserDetailsDto? User { get; set; }

    public void Dispose()
    {
        timer?.Dispose();
        timer = null;
    }

    protected override void OnParametersSet()
    {
        if (User?.Audits is not null && (DateProvider.Now - User.Audits.Last().Timestamp).TotalMinutes < 60)
            timer = new(_ => OnTimerElapsed(), state: null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
    }

    void OnTimerElapsed() => StateHasChanged();

    static string GetAuditName(UserAuditDto audit) => audit.Type switch
    {
        UserAuditType.Import => "Import fra tidligere system",
        UserAuditType.SignUp => "Oprettelse af bruger",
        UserAuditType.ConfirmEmail => "Bekræftelse af mail",
        UserAuditType.RequestResendConfirmEmail => "Anmodning om genbekræftelse af mail",
        UserAuditType.RequestNewPassword => "Anmodning om ny adgangskode",
        UserAuditType.UpdatePassword => "Opdatering af adgangskode",
        UserAuditType.UpdateApartmentId => "Opdatering af adresse",
        UserAuditType.UpdateFullName => "Opdatering af navn",
        UserAuditType.UpdatePhone => "Opdatering af telefonnummer",
        UserAuditType.SetAccountNumber => "Opdatering af kontonummer",
        UserAuditType.RemoveAccountNumber => "Sletning af kontonummer",
        UserAuditType.UpdateEmailSubscriptions => "Opdatering af mailabonnementer",
        UserAuditType.UpdateRoles => "Opdatering af administrative adgange",
        UserAuditType.PlaceOrder => $"Oprettelse af bestilling {audit.OrderId!.Value}",
        UserAuditType.PlaceOwnerOrder => $"Oprettelse af bestilling {audit.OrderId!.Value} til ejer",
        UserAuditType.PayIn => "Indbetaling",
        UserAuditType.PayOut => "Udbetaling",
        UserAuditType.RequestDelete => "Anmodning om sletning af bruger",
        UserAuditType.Delete => "Sletning af bruger",
        UserAuditType.Reimburse => "Godtgørelse",
        _ => "?",
    };
}
