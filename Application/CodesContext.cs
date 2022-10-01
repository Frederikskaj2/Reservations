using LanguageExt;

namespace Frederikskaj2.Reservations.Application;

record CodesContext(HashSet<CombinationCode> AllCodes, Seq<LockBoxCode> ExistingLockBoxCodes, Seq<LockBoxCode> LockBoxCodes);