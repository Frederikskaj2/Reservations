using Microsoft.AspNetCore.Components;

namespace Frederikskaj2.Reservations.Emails.Messages;

public class MessageComponentBase<TModel> : ComponentBase
{
    [Parameter] public EmailModel<TModel> Model { get; set; } = null!;
}
