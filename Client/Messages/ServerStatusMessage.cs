namespace Frederikskaj2.Reservations.Client;

record ServerStatusMessage(bool IsUp)
{
    public static readonly ServerStatusMessage Down = new(false);
    public static readonly ServerStatusMessage Up = new(true);
}