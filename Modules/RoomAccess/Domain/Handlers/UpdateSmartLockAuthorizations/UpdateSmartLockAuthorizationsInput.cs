using LanguageExt;

namespace Frederikskaj2.Reservations.RoomAccess;

record UpdateSmartLockAuthorizationsInput(
    UpdateSmartLockAuthorizationsCommand Command,
    ISmartLockAuthorizationContext SmartLockAuthorizationContext,
    Seq<ReservationInformation> Reservations);
