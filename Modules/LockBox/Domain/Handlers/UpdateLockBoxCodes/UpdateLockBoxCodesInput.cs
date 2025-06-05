using Frederikskaj2.Reservations.Persistence;

namespace Frederikskaj2.Reservations.LockBox;

record UpdateLockBoxCodesInput(UpdateLockBoxCodesCommand Command, OptionalEntity<LockBoxCodes> LockBoxCodesEntity);