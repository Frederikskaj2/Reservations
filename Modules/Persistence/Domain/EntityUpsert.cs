using System;

namespace Frederikskaj2.Reservations.Persistence;

public record EntityUpsert(string Id, Type Type, object Value);
