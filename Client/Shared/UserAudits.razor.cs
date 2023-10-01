using System;
using System.Collections.Generic;
using System.Threading;
using Frederikskaj2.Reservations.Shared.Core;
using Microsoft.AspNetCore.Components;

namespace Frederikskaj2.Reservations.Client.Shared;

public sealed partial class UserAudits : IDisposable
{
    Timer? timer;

    [Inject] public IDateProvider DateProvider { get; set; } = null!;
    [Inject] public Formatter Formatter { get; set; } = null!;

    [Parameter] public UserDetails? User { get; set; }

    public void Dispose()
    {
        timer?.Dispose();
        timer = null;
    }

    protected override void OnParametersSet()
    {
        if (User?.LatestSignIn is not null && (DateProvider.Now - User.LatestSignIn.Value).TotalMinutes < 60)
            timer = new Timer(_ => OnTimerElapsed(), null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
    }

    void OnTimerElapsed() => StateHasChanged();

    static string GetAuditName(UserAudit audit) => audit.Type switch
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
        UserAuditType.CreateOrder => $"Oprettelse af bestilling {audit.OrderId!.Value}",
        UserAuditType.CreateOwnerOrder => $"Oprettelse af bestilling {audit.OrderId!.Value} til ejer",
        UserAuditType.PayIn => "Indbetaling",
        UserAuditType.PayOut => "Udbetaling",
        UserAuditType.RequestDelete => "Anmodning om sletning af bruger",
        UserAuditType.Delete => "Sletning af bruger",
        UserAuditType.Reimburse => "Godtgørelse",
        _ => "?"
    };
}
