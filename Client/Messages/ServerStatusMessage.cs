namespace Frederikskaj2.Reservations.Client;

record ServerStatusMessage(bool IsUp)
{
    public static readonly ServerStatusMessage Down = new(IsUp: false);
    public static readonly ServerStatusMessage Up = new(IsUp: true);
}
