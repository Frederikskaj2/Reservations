using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using NodaTime;
using NodaTime.Calendars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using static Frederikskaj2.Reservations.Application.DatabaseFunctions;
using static LanguageExt.Prelude;
using Array = System.Array;

namespace Frederikskaj2.Reservations.Application;

static class LockBoxCodeFunctions
{
    const int lockBoxCodeCount = 10;
    const int maximumChangeCount = 3;
    const int maximumDigits = 6;
    const int minimumDigits = 4;
    const string singletonId = "";

    static readonly int[] allDigits = Range(0, 10).ToArray();
    static readonly ThreadLocal<Random> random = new(() => new Random());

    static Random Random => random.Value!;

    public static IEnumerable<WeeklyLockBoxCodes> CreateWeeklyLockBoxCodes(LockBoxCodes lockBoxCodes) =>
        lockBoxCodes.Codes
            .OrderBy(code => code.Date)
            .GroupBy(
                code => code.ResourceId,
                (key, values) => (ResourceId: key, Codes: CreateLockBoxCodes(values.Map(code => (code.Date, new CombinationCode(code.Code))))))
            .SelectMany(
                tuple => tuple.Codes,
                (parent, child) => (child.Date, Code: new Shared.Core.LockBoxCode(parent.ResourceId, child.Code, child.Difference)))
            .GroupBy(tuple => tuple.Date)
            .Map(
                grouping => new WeeklyLockBoxCodes(
                    WeekYearRules.Iso.GetWeekOfWeekYear(grouping.Key),
                    grouping.Key,
                    grouping.Map(tuple => tuple.Code)));

    static IEnumerable<(LocalDate Date, string Code, string Difference)> CreateLockBoxCodes(IEnumerable<(LocalDate Date, CombinationCode Code)> codes) =>
        codes.AsPrefixPairs().Map(pair => CreateLockBoxCode(pair.Previous, pair.Current));

    static (LocalDate Date, string Code, string Difference) CreateLockBoxCode(
        Option<(LocalDate Date, CombinationCode Code)> previous, (LocalDate Date, CombinationCode Code) current) =>
        previous.Match(
            tuple => (current.Date, current.Code.ToString(), CombinationCode.GetDifference(current.Code, tuple.Code).ToString()),
            () => (current.Date, current.Code.ToString(), ""));

    public static HashMap<(ResourceId, LocalDate), LockBoxCode> CreateLockBoxCodeMap(LockBoxCodes lockBoxCodes) =>
        toHashMap(lockBoxCodes.Codes.Map(code => ((code.ResourceId, code.Date), code)));

    public static IEnumerable<DatedLockBoxCode> CreateDatedLockBoxCodes(
        HashMap<(ResourceId, LocalDate), LockBoxCode> lockBoxCodes, Reservation reservation) =>
        CreateDatedLockBoxCodes(lockBoxCodes, reservation, GetPreviousMonday(reservation.Extent.Date), GetPreviousMonday(reservation.Extent.Ends()));

    static IEnumerable<DatedLockBoxCode> CreateDatedLockBoxCodes(
        HashMap<(ResourceId, LocalDate), LockBoxCode> lockBoxCodes, Reservation reservation, LocalDate firstMonday, LocalDate lastMonday) =>
        somes(
            Integers(0)
                .Map(firstMonday.PlusWeeks)
                .TakeWhile(monday => monday <= lastMonday)
                .Map(monday => GetDatedLockBoxCode(lockBoxCodes, reservation.ResourceId, monday, GetLockBoxCodeDateForReservation(reservation, monday))));

    static LocalDate GetLockBoxCodeDateForReservation(Reservation reservation, LocalDate monday) =>
        monday < reservation.Extent.Date ? reservation.Extent.Date : monday;

    static Option<DatedLockBoxCode> GetDatedLockBoxCode(
        HashMap<(ResourceId, LocalDate), LockBoxCode> lockBoxCodes, ResourceId resourceId, LocalDate monday, LocalDate date) =>
        lockBoxCodes.Find((resourceId, monday)).Case switch
        {
            LockBoxCode code => Some(new DatedLockBoxCode(date, code.Code)),
            _ => None
        };

    public static EitherAsync<Failure, IPersistenceContext> ReadLockBoxCodesContext(IPersistenceContext context, LocalDate date) =>
        from context1 in MapReadError(context.ReadItem(singletonId, () => new LockBoxCodes(Empty)))
        select UpdateLockBoxCodes(context1, date);

    static IPersistenceContext UpdateLockBoxCodes(IPersistenceContext context, LocalDate date) =>
        UpdateLockBoxCodes(context, context.Item<LockBoxCodes>(), GetLockBoxCodes(context.Item<LockBoxCodes>(), GetPreviousMonday(date).PlusWeeks(-1)));

    static IPersistenceContext UpdateLockBoxCodes(IPersistenceContext context, LockBoxCodes existingLockBoxCodes, LockBoxCodes newLockBoxCodes) =>
        newLockBoxCodes != existingLockBoxCodes ? context.UpdateItem(singletonId, newLockBoxCodes) : context;

    static LocalDate GetPreviousMonday(LocalDate date) => date.PlusDays(-(((int) date.DayOfWeek - 1)%7));

    static LockBoxCodes GetLockBoxCodes(LockBoxCodes lockBoxCodes, LocalDate monday) =>
        new(GetLockBoxCodes(lockBoxCodes.Codes.Filter(lockBoxCode => lockBoxCode.Date >= monday), monday));

    static Seq<LockBoxCode> GetLockBoxCodes(Seq<LockBoxCode> lockBoxCodes, LocalDate monday) =>
        UpdateCodesContext(
            monday,
            new CodesContext(toHashSet(lockBoxCodes.Map(code => new CombinationCode(code.Code))), lockBoxCodes, Empty)).LockBoxCodes;

    static CodesContext UpdateCodesContext(LocalDate monday, CodesContext context) =>
        Resources.GetAll().Keys.Fold(context, (state, id) => UpdateCodesForResourceContext(monday, state, id));

    static CodesContext UpdateCodesForResourceContext(LocalDate monday, CodesContext context, ResourceId resourceId) =>
        UpdateCodesForResourceContext(
            monday,
            context,
            resourceId,
            toHashMap(context.ExistingLockBoxCodes.Filter(code => code.ResourceId == resourceId).Map(code => (code.Date, new CombinationCode(code.Code)))));

    static CodesContext UpdateCodesForResourceContext(
        LocalDate monday, CodesContext context, ResourceId resourceId, HashMap<LocalDate, CombinationCode> codesForResource) =>
        UpdateCodesContext(
            context,
            UpdateCodesForResourceContext(resourceId, monday, new CodesForResourceContext(context.AllCodes, codesForResource, Empty)));

    static CodesContext UpdateCodesContext(CodesContext context, CodesForResourceContext codesForResourceContext) =>
        context with
        {
            AllCodes = codesForResourceContext.AllCodes,
            LockBoxCodes = context.LockBoxCodes.Concat(codesForResourceContext.LockBoxCodes)
        };

    static CodesForResourceContext UpdateCodesForResourceContext(ResourceId resourceId, LocalDate monday, CodesForResourceContext context) =>
        Range(0, lockBoxCodeCount).Map(monday.PlusWeeks).Fold(context, (state, date) => UpdateCodesForResourceContext(resourceId, state, date));

    static CodesForResourceContext UpdateCodesForResourceContext(ResourceId resourceId, CodesForResourceContext context, LocalDate date) =>
        UpdateCodesForResourceContext(resourceId, context, date, CreateCode(context, date));

    static CodesForResourceContext UpdateCodesForResourceContext(
        ResourceId resourceId, CodesForResourceContext context, LocalDate date, CombinationCode code) =>
        new(context.AllCodes.TryAdd(code), context.CodesForResource.TryAdd(date, code), context.LockBoxCodes.Add(new(resourceId, date, code.ToString())));

    static CombinationCode CreateCode(CodesForResourceContext context, LocalDate date) =>
        context.CodesForResource.Find(date).Case switch
        {
            CombinationCode code => code,
            _ => CreateNewCode(context, date)
        };

    static CombinationCode CreateNewCode(CodesForResourceContext context, LocalDate date) =>
        context.CodesForResource.Find(date.PlusWeeks(-1)).Case switch
        {
            CombinationCode previous => CreateNextUniqueRandomCode(context.AllCodes, previous),
            _ => CreateUniqueRandomCode(context.AllCodes)
        };

    static CombinationCode CreateUniqueRandomCode(LanguageExt.HashSet<CombinationCode> codes)
    {
        while (true)
        {
            var code = CreateRandomCode();
            if (!codes.Contains(code))
                return code;
        }
    }

    static CombinationCode CreateNextUniqueRandomCode(LanguageExt.HashSet<CombinationCode> codes, CombinationCode previousCode)
    {
        for (var i = 0; i < 100; i += 1)
        {
            var code = CreateNextRandomCode(previousCode);
            if (!codes.Contains(code))
                return code;
        }

        // Blow up in case it's impossible to create a unique code. If the
        // number of codes is 17 this can actually happen so the number of
        // codes have been lowered from 15 to 10 to move further away from
        // that limit.
        throw new LockBoxCodesException($"Cannot create unique lock box codes with {codes.Count} existing codes.");
    }

    static CombinationCode CreateRandomCode()
    {
        var length = Random.Next(minimumDigits, maximumDigits + 1);
        var digits = (int[]) allDigits.Clone();
        return new CombinationCode(MemoryMarshal.ToEnumerable<int>(digits.AsMemory().Shuffle(length, Random)));
    }

    static CombinationCode CreateNextRandomCode(CombinationCode previousCode) =>
        GetNextCode(previousCode, CreateNextRandomChange(previousCode));

    static CombinationCode GetNextCode(CombinationCode code, DigitChange change)
    {
        var digitsToRemove = GetDigits(code, change.RemoveCount);
        var digitsToAdd = GetDigits(code.Inverse(), change.AddCount);
        foreach (var digit in digitsToRemove.Span)
            code = code.RemoveDigit(digit);
        foreach (var digit in digitsToAdd.Span)
            code = code.AddDigit(digit);
        return code;
    }

    static Memory<int> GetDigits(CombinationCode code, int digitCount) =>
        digitCount > 0 ? code.ToMemory().Shuffle(digitCount, Random) : Array.Empty<int>();

    static DigitChange CreateNextRandomChange(CombinationCode code) =>
        TryReduceChange(TryAdjustChange(code, CreateRandomChange()));

    static DigitChange CreateRandomChange() =>
        new(Random.Next(1, maximumChangeCount), Random.Next(1, maximumChangeCount));

    static DigitChange TryAdjustChange(CombinationCode code, DigitChange change) =>
        TryAdjustChange(code.DigitCount - change.RemoveCount + change.AddCount, change);

    static DigitChange TryAdjustChange(int newDigitCount, DigitChange change) =>
        newDigitCount switch
        {
            < minimumDigits - 1 => IncreaseAddCount(newDigitCount, change),
            < minimumDigits => DecreaseRemoveCountOrIncreaseAddCount(change),
            > maximumDigits + 1 => IncreaseRemoveCount(newDigitCount, change),
            > maximumDigits => IncreaseRemoveCountOrDecreaseAddCount(change),
            _ => change
        };

    static DigitChange IncreaseAddCount(int newDigitCount, DigitChange change) =>
        change with { AddCount = minimumDigits + change.AddCount - newDigitCount };

    static DigitChange IncreaseRemoveCount(int newDigitCount, DigitChange change) =>
        change with { RemoveCount = newDigitCount - (maximumDigits - change.RemoveCount) };

    static DigitChange DecreaseRemoveCountOrIncreaseAddCount(DigitChange change) =>
        Random.Next(0, 1) is 0
            ? change with { RemoveCount = change.RemoveCount - 1 }
            : change with { AddCount = change.AddCount + 1 };

    static DigitChange IncreaseRemoveCountOrDecreaseAddCount(DigitChange change) =>
        Random.Next(0, 1) is 0
            ? change with { RemoveCount = change.RemoveCount + 1 }
            : change with { AddCount = change.AddCount - 1 };

    static DigitChange TryReduceChange(DigitChange change) =>
        change.RemoveCount + change.AddCount >= 4 && change.RemoveCount >= 1 && change.AddCount >= 1
            ? new(change.RemoveCount - 1, change.AddCount - 1)
            : change;

    static IEnumerable<int> Integers(int from)
    {
        while (true)
            yield return from++;
        // ReSharper disable once IteratorNeverReturns
    }

    public static EitherAsync<Failure, Unit> SendLockBoxCodesEmail(
        IEmailService emailService, Seq<ReservationWithOrder> reservations, LockBoxCodes lockBoxCodes, IEnumerable<EmailUser> users) =>
        SendLockBoxCodesEmail(emailService, reservations, CreateLockBoxCodeMap(lockBoxCodes), toHashMap(users.Map(user => (user.UserId, user))));

    static EitherAsync<Failure, Unit> SendLockBoxCodesEmail(
        IEmailService emailService, Seq<ReservationWithOrder> reservations, HashMap<(ResourceId, LocalDate), LockBoxCode> lockBoxCodes,
        HashMap<UserId, EmailUser> users) =>
        from _ in reservations
            .Map(
                reservation => SendLockBoxCodesEmail(
                    emailService,
                    reservation,
                    lockBoxCodes,
                    users[reservation.Order.UserId]))
            .TraverseSerial(identity)
        select unit;

    static EitherAsync<Failure, Unit> SendLockBoxCodesEmail(
        IEmailService emailService, ReservationWithOrder reservation, HashMap<(ResourceId, LocalDate), LockBoxCode> lockBoxCodes, EmailUser user) =>
        emailService.Send(
            new LockBoxCodesEmail(
                user.Email,
                user.FullName,
                reservation.Order.OrderId,
                reservation.Reservation.ResourceId,
                reservation.Reservation.Extent.Date,
                CreateDatedLockBoxCodes(lockBoxCodes, reservation.Reservation)));

    public static EitherAsync<Failure, Unit> SendLockBoxCodesEmail(IEmailService emailService, User user, IEnumerable<WeeklyLockBoxCodes> lockBoxCodes) =>
        emailService.Send(new LockBoxCodesOverviewEmail(user.Email(), user.FullName, lockBoxCodes));
}
