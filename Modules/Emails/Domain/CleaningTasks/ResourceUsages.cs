using System;

namespace Frederikskaj2.Reservations.Emails;

[Flags]
enum ResourceUsages
{
    Free,
    InUseBefore = 1,
    CleaningBefore = 2,
    InUseBetween = 4,
    CleaningBetween = 8,
    InUseAfter = 16,
    CleaningAfter = 32,
    FreeReservation = InUseAfter,
    Reservation = InUseBefore | InUseBetween | InUseAfter,
    ReservationCleaningFree = InUseBefore | CleaningBetween,
    ReservationCleaning = InUseBefore | CleaningBetween | CleaningAfter,
    ReservationCleaningReservation = InUseBefore | CleaningBetween | InUseAfter,
    CleaningReservation = CleaningBefore | CleaningBetween | InUseAfter,
    CleaningFree = CleaningBefore | CleaningBetween,
    Cleaning = CleaningBefore | CleaningBetween | CleaningAfter,
}
