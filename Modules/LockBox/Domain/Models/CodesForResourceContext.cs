using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.LockBox;

record CodesForResourceContext(HashSet<CombinationCode> AllCodes, HashMap<LocalDate, CombinationCode> CodesForResource, Seq<LockBoxCode> LockBoxCodes);
