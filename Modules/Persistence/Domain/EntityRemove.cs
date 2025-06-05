using Frederikskaj2.Reservations.Core;
using System;

namespace Frederikskaj2.Reservations.Persistence;

public record EntityRemove(string Id, Type Type, ETag ETag);
