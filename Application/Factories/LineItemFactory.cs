using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Application;

static class LineItemFactory
{
    public static IEnumerable<Shared.Core.LineItem> CreateLineItems(UserOrder order) =>
        order.AdditionalLineItems.Map(CreateLineItem);

    static Shared.Core.LineItem CreateLineItem(LineItem lineItem) =>
        new(
            lineItem.Timestamp,
            lineItem.Type,
            CreateCancellationFee(lineItem.CancellationFee),
            CreateDamages(lineItem.Damages),
            lineItem.Amount);

    static Shared.Core.CancellationFee? CreateCancellationFee(CancellationFee? cancellationFee) =>
        cancellationFee is not null ? new Shared.Core.CancellationFee(cancellationFee.Reservations) : null;

    static Shared.Core.Damages? CreateDamages(Damages? damages) =>
        damages is not null ? new Shared.Core.Damages(damages.Reservation, damages.Description) : null;
}
