using LanguageExt;

namespace Frederikskaj2.Reservations.LockBox;

record CodesContext(HashSet<CombinationCode> AllCodes, Seq<LockBoxCode> ExistingLockBoxCodes, Seq<LockBoxCode> LockBoxCodes);