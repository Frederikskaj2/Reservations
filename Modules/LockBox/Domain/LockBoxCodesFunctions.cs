using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using NodaTime;
using NodaTime.Calendars;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static Frederikskaj2.Reservations.LockBox.LockBoxCodesGenerator;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.LockBox;

public static class LockBoxCodesFunctions
{
    const string singletonId = "";

    public static EitherAsync<Failure<Unit>, LockBoxCodes> ReadLockBoxCodes(IEntityReader reader, CancellationToken cancellationToken) =>
        reader.Read<LockBoxCodes>(singletonId, cancellationToken).MapReadError();

    public static EitherAsync<Failure<Unit>, OptionalEntity<LockBoxCodes>> ReadLockBoxCodesEntity(IEntityReader reader, CancellationToken cancellationToken) =>
        reader.ReadOptional<LockBoxCodes>(singletonId, () => new(Empty), cancellationToken).MapReadError();

    public static EitherAsync<Failure<Unit>, OptionalEntity<LockBoxCodes>> ReadLockBoxCodesEntity(
        IEntityReader reader, LocalDate date, CancellationToken cancellationToken) =>
        from lockBoxCodes in reader.ReadOptional<LockBoxCodes>(singletonId, () => new(Empty), cancellationToken).MapReadError()
        select UpdateLockBoxCodes(lockBoxCodes, date.PreviousMonday().PlusWeeks(-1));

    static OptionalEntity<LockBoxCodes> UpdateLockBoxCodes(OptionalEntity<LockBoxCodes> existingLockBoxCodes, LocalDate firstMonday) =>
        existingLockBoxCodes.Match<OptionalEntity<LockBoxCodes>>(
            entity => entity with { Value = GetLockBoxCodes(entity.Value,  firstMonday) },
            eTaggedEntity => eTaggedEntity with { Value = GetLockBoxCodes(eTaggedEntity.Value,  firstMonday) });

    public static Seq<WeeklyLockBoxCodes> CreateWeeklyLockBoxCodes(LockBoxCodes lockBoxCodes) =>
        lockBoxCodes.Codes
            .OrderBy(code => code.Date)
            .GroupBy(
                code => code.ResourceId,
                (key, values) => (ResourceId: key, Codes: CreateLockBoxCodes(values.Map(code => (code.Date, new CombinationCode(code.Code))))))
            .SelectMany(
                tuple => tuple.Codes,
                (parent, child) => (child.Date, Code: new WeeklyLockBoxCode(parent.ResourceId, child.Code, child.Difference)))
            .GroupBy(tuple => tuple.Date)
            .Map(
                grouping => new WeeklyLockBoxCodes(
                    WeekYearRules.Iso.GetWeekOfWeekYear(grouping.Key),
                    grouping.Key,
                    grouping.Map(tuple => tuple.Code).ToSeq()))
            .ToSeq();

    static IEnumerable<(LocalDate Date, string Code, string Difference)> CreateLockBoxCodes(IEnumerable<(LocalDate Date, CombinationCode Code)> codes) =>
        codes.AsPairs().Map(pair => CreateLockBoxCode(pair.Previous, pair.Current));

    static (LocalDate Date, string Code, string Difference) CreateLockBoxCode(
        Option<(LocalDate Date, CombinationCode Code)> previous, (LocalDate Date, CombinationCode Code) current) =>
        previous.Match(
            tuple => (current.Date, current.Code.ToString(), CombinationCode.GetDifference(current.Code, tuple.Code).ToString()),
            () => (current.Date, current.Code.ToString(), ""));
}
