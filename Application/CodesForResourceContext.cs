using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Application;

record CodesForResourceContext(HashSet<CombinationCode> AllCodes, HashMap<LocalDate, CombinationCode> CodesForResource, Seq<LockBoxCode> LockBoxCodes);