using System;

namespace Frederikskaj2.Reservations.Shared
{
    internal class ReservationPolicyProvider : IReservationPolicyProvider
    {
        private readonly BanquetFacilitiesReservationPolicy banquetFacilitiesReservationPolicy;
        private readonly BedroomReservationPolicy bedroomReservationPolicy;

        public ReservationPolicyProvider(
            BanquetFacilitiesReservationPolicy banquetFacilitiesReservationPolicy,
            BedroomReservationPolicy bedroomReservationPolicy)
        {
            this.banquetFacilitiesReservationPolicy = banquetFacilitiesReservationPolicy;
            this.bedroomReservationPolicy = bedroomReservationPolicy;
        }

        public IReservationPolicy GetPolicy(ResourceType resourceType)
            => resourceType switch
            {
                ResourceType.BanquetFacilities => (IReservationPolicy) banquetFacilitiesReservationPolicy,
                ResourceType.Bedroom => bedroomReservationPolicy,
                _ => throw new ArgumentOutOfRangeException(nameof(resourceType), resourceType, null)
            };
    }
}