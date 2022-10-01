using System;

namespace Frederikskaj2.Reservations.Infrastructure.Persistence;

// TODO: This shouldn't be public but is used in BatchOperation that has to be public to work correctly.
public record Key(string Id, Type Type);