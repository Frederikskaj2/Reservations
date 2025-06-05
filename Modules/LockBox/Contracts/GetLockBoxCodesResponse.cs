using System.Collections.Generic;

namespace Frederikskaj2.Reservations.LockBox;

public record GetLockBoxCodesResponse(IEnumerable<WeeklyLockBoxCodesDto> LockBoxCodes);
