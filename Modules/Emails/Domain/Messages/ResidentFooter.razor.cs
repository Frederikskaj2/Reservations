using Microsoft.AspNetCore.Components;

namespace Frederikskaj2.Reservations.Emails.Messages;

partial class ResidentFooter
{
    [Parameter] public EmailModel Model { get; set; } = null!;
}
