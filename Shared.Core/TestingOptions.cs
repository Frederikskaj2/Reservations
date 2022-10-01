namespace Frederikskaj2.Reservations.Shared.Core;

public class TestingOptions
{
    public bool IsTestingEnabled { get; set; }
    public bool IsSettlementAlwaysAllowed { get; set; }
    public bool IsConfigurationUnavailable { get; init; }
}
