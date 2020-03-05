namespace Frederikskaj2.Reservations.Shared
{
    public interface IReservationPolicyProvider
    {
        IReservationPolicy GetPolicy(ResourceType resourceType);
    }
}