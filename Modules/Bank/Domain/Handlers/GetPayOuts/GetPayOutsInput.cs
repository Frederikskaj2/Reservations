using Frederikskaj2.Reservations.Persistence;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Bank;

record GetPayOutsInput(Seq<ETaggedEntity<PayOut>> PayOutEntities, Seq<UserExcerpt> UserExcerpts, Option<Instant> LatestImportTimestamp);