using System;

namespace Frederikskaj2.Reservations.Users;

public class RemotePasswordCheckerOptions
{
    public bool IsEnabled { get; init; }
    public string ProductName { get; init; } = "lokaler.frederikskaj2.dk";
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(2);
    public int CacheSize { get; init; } = 256;
    public TimeSpan CacheExpiration { get; init; } = TimeSpan.FromHours(1);
    public bool AlwaysUseLocalData { get; init; }
}
