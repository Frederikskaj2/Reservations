using System.Diagnostics.CodeAnalysis;

namespace Frederikskaj2.Reservations.Shared
{
    [SuppressMessage("Microsoft.Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "This class is instantiated vis dependency injection.")]
    internal class BedroomReservationPolicy : ReservationPolicy, IReservationPolicy
    {
        private const ResourceType ResourceType = Shared.ResourceType.Bedroom;

        public BedroomReservationPolicy(IDataProvider dataProvider, ReservationsOptions options, IDateProvider dateProvider)
            : base(dataProvider, options, options.Prices[ResourceType], dateProvider)
        {
        }

        public ResourceType Type => ResourceType;
    }
}