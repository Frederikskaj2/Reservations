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
        UserAuditType.Import => "Importeret fra tidligere system",
        UserAuditType.SignUp => "Bruger oprettet",
        UserAuditType.ConfirmEmail => "Mail bekræftet",
        UserAuditType.RequestResendConfirmEmail => "Genbekræftelse af mail anmodet",
        UserAuditType.RequestNewPassword => "Ny adgangskode anmodet",
        UserAuditType.UpdatePassword => "Adgangskode ændret",
        UserAuditType.UpdateApartmentId => "Adresse ændret",
        UserAuditType.UpdateFullName => "Navn ændret",
        UserAuditType.UpdatePhone => "Telefonnummer ændret",
        UserAuditType.SetAccountNumber => "Kontonummer ændret",
        UserAuditType.RemoveAccountNumber => "Kontonummer slettet",
        UserAuditType.UpdateEmailSubscriptions => "Mailabonnementer ændret",
        UserAuditType.UpdateRoles => "Administrative adgange ændret",
        UserAuditType.PlaceOrder => $"Bestilling {audit.OrderId!.Value} oprettet",
        UserAuditType.PlaceOwnerOrder => $"Bestilling {audit.OrderId!.Value} til ejer oprettet",
        UserAuditType.PayIn => "Indbetalt",
        UserAuditType.PayOut => "Udbetalt",
        UserAuditType.RequestDelete => "Sletning af bruger anmodet",
        UserAuditType.Delete => "Bruger slettet",
        UserAuditType.Reimburse => "Godtgjort",
        _ => "?",
    };
}
