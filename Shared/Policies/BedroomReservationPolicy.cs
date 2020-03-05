namespace Frederikskaj2.Reservations.Shared
{
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