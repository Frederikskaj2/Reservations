using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using NodaTime;
using static Frederikskaj2.Reservations.LockBox.LockBoxCodesGenerator;

namespace Frederikskaj2.Reservations.LockBox;

static class UpdateLockBoxCodes
{
    public static UpdateLockBoxCodesOutput UpdateLockBoxCodesCore(UpdateLockBoxCodesInput input) =>
        new(UpdateLockBoxCodesCore(input.LockBoxCodesEntity, input.Command.Date.PreviousMonday().PlusWeeks(-1)));

    static OptionalEntity<LockBoxCodes> UpdateLockBoxCodesCore(OptionalEntity<LockBoxCodes> existingLockBoxCodes, LocalDate firstMonday) =>
        existingLockBoxCodes.Match<OptionalEntity<LockBoxCodes>>(
            entity => entity with { Value = GetLockBoxCodes(entity.Value, firstMonday) },
            eTaggedEntity => eTaggedEntity with { Value = GetLockBoxCodes(eTaggedEntity.Value, firstMonday) });
}
