using Frederikskaj2.Reservations.LockBox;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Emails;

public record LockBoxCodesOverviewDto(IEnumerable<Resource> Resources, IEnumerable<WeeklyLockBoxCodesDto> Codes);
