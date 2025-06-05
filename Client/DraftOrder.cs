using Frederikskaj2.Reservations.Calendar;
using Frederikskaj2.Reservations.LockBox;
using Frederikskaj2.Reservations.Orders;
using System.Collections.Generic;
using System.Linq;

namespace Frederikskaj2.Reservations.Client;

public class DraftOrder(EventAggregator eventAggregator)
{
    readonly List<DraftReservation> reservations = [];

    public IEnumerable<DraftReservation> Reservations => reservations;

    public DraftReservation? DraftReservation { get; private set; }

    public void AddReservation(Resource resource, Extent extent) => DraftReservation = new(resource, extent);

    public void UpdateReservation(Extent extent)
    {
        DraftReservation = DraftReservation! with { Extent = extent };
        eventAggregator.Publish(DraftOrderUpdatedMessage.Instance);
    }

    public void ClearReservation()
    {
        DraftReservation = null;
        eventAggregator.Publish(DraftOrderUpdatedMessage.Instance);
    }

    public void AddReservationToOrder()
    {
        reservations.Add(DraftReservation!);
        ClearReservation();
        eventAggregator.Publish(DraftOrderUpdatedMessage.Instance);
    }

    public void RemoveReservation(DraftReservation reservation)
    {
        reservations.Remove(reservation);
        eventAggregator.Publish(DraftOrderUpdatedMessage.Instance);
    }

    public void Clear()
    {
        reservations.Clear();
        DraftReservation = null;
        eventAggregator.Publish(DraftOrderUpdatedMessage.Instance);
    }

    public IEnumerable<ReservationRequest> GetReservationRequests() =>
        reservations.Select(reservation => new ReservationRequest(reservation.Resource.ResourceId, reservation.Extent));

    public IEnumerable<ReservedDayDto> ReservedDays()
        => Reservations.SelectMany(reservation => reservation.ReservedDays());
}
