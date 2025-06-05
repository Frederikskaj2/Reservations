using OneOf;

namespace Frederikskaj2.Reservations.Persistence;

[GenerateOneOf]
public partial class EntityOperation : OneOfBase<EntityAdd, EntityUpdate, EntityUpsert, EntityRemove>;
