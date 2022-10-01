using Frederikskaj2.Reservations.Shared.Core;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Shared.Email;

public record CleaningSchedule(
    EmailAddress Email,
    string FullName,
    Core.CleaningSchedule Schedule,
    CleaningTasksDelta Delta,
    IEnumerable<Resource> Resources) : MessageBase(Email, FullName);
