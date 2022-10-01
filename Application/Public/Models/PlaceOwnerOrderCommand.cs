using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Application;

public record PlaceOwnerOrderCommand(Instant Timestamp, UserId UserId, string Description, Seq<ReservationModel> Reservations, bool IsCleaningRequired);
