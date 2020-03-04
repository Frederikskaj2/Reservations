namespace Frederikskaj2.Reservations.Shared
{
    public interface IReservationPolicy
    {
        // Answers
        //   - Min and max number of days
        //       Required: reservations from DB, holiday calendar
        //   - Price
        //       Required: Holiday calendar
        // Validations
        //   - No double reservations
        //       Required: reservations from DB
        //   - Minimum reservation time satisfied
        //       Required: Holiday calendar
    }
}