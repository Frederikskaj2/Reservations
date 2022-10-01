using LanguageExt;

namespace Frederikskaj2.Reservations.Infrastructure.Persistence;

record Entity(object Item, EntityStatus Status, Option<string> ETag);