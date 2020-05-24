using System.Diagnostics.CodeAnalysis;

namespace Frederikskaj2.Reservations.Shared
{
    [SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "URLs are conventionally lower case.")]
    public static class ProblemTypes
    {
        private const string typePrefix = "/problems/";

        public static string ReservationConflict { get; } = typePrefix + nameof(ReservationConflict).ToLowerInvariant();
        public static string DatabaseError { get; } = typePrefix + nameof(DatabaseError).ToLowerInvariant();

    }
}
