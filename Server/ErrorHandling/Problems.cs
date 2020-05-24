using Frederikskaj2.Reservations.Shared;

namespace Frederikskaj2.Reservations.Server.ErrorHandling
{
    public static class Problems
    {
        public static readonly Problem ReservationConflict = new Problem(ProblemTypes.ReservationConflict, "Reservation Conflict", 409);

        public static readonly Problem DatabaseError = new Problem(ProblemTypes.DatabaseError, "Database Error", 500);
    }
}
