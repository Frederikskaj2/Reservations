using OneOf;

namespace Frederikskaj2.Reservations.Persistence;

[GenerateOneOf]
public partial class OptionalEntity<T> : OneOfBase<Entity<T>, ETaggedEntity<T>>
{
    public T EntityValue => Match(entity => entity.Value, eTaggedEntity => eTaggedEntity.Value);
}
