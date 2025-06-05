using NodaTime;

namespace Frederikskaj2.Reservations.Emails.Messages.NoFeeCancellationAllowed;

partial class Body
{
    string FormatDuration(Duration duration) =>
        duration switch
        {
            { Days: 1, Hours: 0, Minutes: 0 } => "den næste dag",
            { Days: > 1, Hours: 0, Minutes: 0 } => $"de næste {Model.FormatDuration(duration)}",
            { Days: 0, Hours: 1, Minutes: 0 } => "den næste time",
            { Days: 0, Hours: > 1, Minutes: 0 } => $"de næste {Model.FormatDuration(duration)}",
            { Days: 0, Hours: 0, Minutes: 1 } => "det næste minut",
            { Days: 0, Hours: 0, Minutes: > 1 } => $"de næste {Model.FormatDuration(duration)}",
            _ => $"i {Model.FormatDuration(duration)}",
        };
}
