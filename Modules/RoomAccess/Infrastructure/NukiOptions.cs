using System;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.RoomAccess;

public class NukiOptions
{
    public bool IsEnabled { get; init; }
    public Uri Endpoint  { get; init; } = new("https://api.nuki.io/");
    public string? ApiKey { get; init; }

    public IReadOnlyDictionary<string, ulong> Smartlocks { get; init; } = new Dictionary<string, ulong>
    {
        ["Festlokale"] = 22715936399,
        ["Frederik (soveværelse)"] = 22715941167,
        ["Kaj (soveværelse)"] = 22715944287,
    };
}
