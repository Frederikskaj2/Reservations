using LanguageExt;
using NodaTime;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.LockBox;

public static class LockBoxCodesGenerator
{
    const int lockBoxCodeCount = 10;
    const int maximumChangeCount = 3;
    const int maximumDigits = 6;
    const int minimumDigits = 4;

    static readonly int[] allDigits = Range(0, 10).ToArray();
    static readonly ThreadLocal<Random> random = new(() => new());

    static Random Random => random.Value!;

    public static LockBoxCodes GetLockBoxCodes(LockBoxCodes lockBoxCodes, LocalDate firstMonday) =>
        new(GetLockBoxCodes(lockBoxCodes.Codes.Filter(lockBoxCode => lockBoxCode.Date >= firstMonday), firstMonday));

    static Seq<LockBoxCode> GetLockBoxCodes(Seq<LockBoxCode> lockBoxCodes, LocalDate firstMonday) =>
        UpdateCodesContext(
            firstMonday,
            new(toHashSet(lockBoxCodes.Map(code => new CombinationCode(code.Code))), lockBoxCodes, Empty)).LockBoxCodes;

    static CodesContext UpdateCodesContext(LocalDate firstMonday, CodesContext context) =>
        Resources.All.Map(resource => resource.ResourceId).Fold(context, (state, id) => UpdateCodesForResourceContext(firstMonday, state, id));

    static CodesContext UpdateCodesForResourceContext(LocalDate firstMonday, CodesContext context, ResourceId resourceId) =>
        UpdateCodesForResourceContext(
            firstMonday,
            context,
            resourceId,
            toHashMap(context.ExistingLockBoxCodes.Filter(code => code.ResourceId == resourceId).Map(code => (code.Date, new CombinationCode(code.Code)))));

    static CodesContext UpdateCodesForResourceContext(
        LocalDate firstMonday, CodesContext context, ResourceId resourceId, HashMap<LocalDate, CombinationCode> codesForResource) =>
        UpdateCodesContext(
            context,
            UpdateCodesForResourceContext(resourceId, firstMonday, new(context.AllCodes, codesForResource, Empty)));

    static CodesContext UpdateCodesContext(CodesContext context, CodesForResourceContext codesForResourceContext) =>
        context with { AllCodes = codesForResourceContext.AllCodes, LockBoxCodes = context.LockBoxCodes.Concat(codesForResourceContext.LockBoxCodes) };

    static CodesForResourceContext UpdateCodesForResourceContext(ResourceId resourceId, LocalDate firstMonday, CodesForResourceContext context) =>
        Range(0, lockBoxCodeCount).Map(firstMonday.PlusWeeks).Fold(context, (state, date) => UpdateCodesForResourceContext(resourceId, state, date));

    static CodesForResourceContext UpdateCodesForResourceContext(ResourceId resourceId, CodesForResourceContext context, LocalDate date) =>
        UpdateCodesForResourceContext(resourceId, context, date, CreateCode(context, date));

    static CodesForResourceContext UpdateCodesForResourceContext(
        ResourceId resourceId, CodesForResourceContext context, LocalDate date, CombinationCode code) =>
        new(context.AllCodes.TryAdd(code), context.CodesForResource.TryAdd(date, code), context.LockBoxCodes.Add(new(resourceId, date, code.ToString())));

    static CombinationCode CreateCode(CodesForResourceContext context, LocalDate date) =>
        context.CodesForResource.Find(date).Case switch
        {
            CombinationCode code => code,
            _ => CreateNewCode(context, date),
        };

    static CombinationCode CreateNewCode(CodesForResourceContext context, LocalDate date) =>
        context.CodesForResource.Find(date.PlusWeeks(-1)).Case switch
        {
            CombinationCode previous => CreateNextUniqueRandomCode(context.AllCodes, previous),
            _ => CreateUniqueRandomCode(context.AllCodes),
        };

    static CombinationCode CreateUniqueRandomCode(HashSet<CombinationCode> codes)
    {
        for (var i = 0; i < 100; i += 1)
        {
            var code = CreateRandomCode();
            if (!codes.Contains(code))
                return code;
        }

        // Blow up in case it's impossible to create a unique code. If the
        // number of codes is 17, this can actually happen, so the number of
        // codes has been lowered from 15 to 10 to move further away from
        // that limit.
        throw new LockBoxCodesException($"Cannot create unique lock box codes with {codes.Count} existing codes.");
    }

    static CombinationCode CreateNextUniqueRandomCode(HashSet<CombinationCode> codes, CombinationCode previousCode)
    {
        for (var i = 0; i < 100; i += 1)
        {
            var code = CreateNextRandomCode(previousCode);
            if (!codes.Contains(code))
                return code;
        }

        // Blow up in case it's impossible to create a unique code. If the
        // number of codes is 17, this can actually happen, so the number of
        // codes has been lowered from 15 to 10 to move further away from
        // that limit.
        throw new LockBoxCodesException($"Cannot create unique lock box codes with {codes.Count} existing codes.");
    }

    static CombinationCode CreateRandomCode()
    {
        var length = Random.Next(minimumDigits, maximumDigits + 1);
        var digits = (int[]) allDigits.Clone();
        return new(MemoryMarshal.ToEnumerable<int>(digits.AsMemory().Shuffle(length, Random)));
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
        digitCount > 0 ? code.ToMemory().Shuffle(digitCount, Random) : System.Array.Empty<int>();

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
            _ => change,
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
        change.RemoveCount + change.AddCount > maximumChangeCount && change is { RemoveCount: >= 1, AddCount: >= 1 }
            ? new(change.RemoveCount - 1, change.AddCount - 1)
            : change;
}
