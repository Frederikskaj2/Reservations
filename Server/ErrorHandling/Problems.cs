using Frederikskaj2.Reservations.Server.ErrorHandling;
using Frederikskaj2.Reservations.Shared;

namespace BlazorApp1.Server.Controllers
{
    public static class Problems
    {
        public static readonly Problem ReservationConflict = new Problem(ProblemTypes.ReservationConflict, "Reservation Conflict", 409);

        public static readonly Problem DatabaseError = new Problem(ProblemTypes.DatabaseError, "Database Error", 500);
    }
}
