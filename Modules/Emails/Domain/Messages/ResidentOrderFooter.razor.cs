using Frederikskaj2.Reservations.Users;
using Microsoft.AspNetCore.Components;

namespace Frederikskaj2.Reservations.Emails.Messages;

partial class ResidentOrderFooter
{
    [Parameter] public EmailModel Model { get; set; } = null!;
    [Parameter] public OrderId OrderId { get; set; }
}
