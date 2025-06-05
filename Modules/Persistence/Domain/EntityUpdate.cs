using Frederikskaj2.Reservations.Core;
using System;

namespace Frederikskaj2.Reservations.Persistence;

public record EntityUpdate(string Id, Type Type, object Value, ETag ETag);
