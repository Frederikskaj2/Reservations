using NodaTime;

namespace Frederikskaj2.Reservations.Users;

record SignUpOutput(Instant Timestamp, UserEmail UserEmail, User User);
