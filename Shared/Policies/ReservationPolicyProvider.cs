using System;
using System.Diagnostics.CodeAnalysis;

namespace Frederikskaj2.Reservations.Shared
{
    [SuppressMessage("Microsoft.Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "This class is instantiated vis dependency injection.")]
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