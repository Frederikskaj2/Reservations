using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;
using NodaTime.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.RoomAccess;

partial class NukiSmartLockService(HttpClient httpClient, ILogger<NukiSmartLockService> logger, IOptions<NukiOptions> options, ITimeConverter timeConverter)
    : ISmartLockService
{
    const int allWeekdays = 127;
    const int entryCodeAuthType = 13;

    readonly LanguageExt.HashSet<ulong> smartlockIds = toHashSet(options.Value.Smartlocks.Values);
    readonly HashMap<ulong, ResourceId> smartlockMap = toHashMap(options.Value.Smartlocks.Map(kvp => (kvp.Value, Resources.GetResourceIdUnsafe(kvp.Key))));

    [GeneratedRegex(@"^Bestilling (?<orderId>[1-9]\d{0,4})$", RegexOptions.None, 1000)]
    static partial Regex NameRegex { get; }

    public async Task<ISmartLockAuthorizationContext> GetSmartLockAuthorizationContext(CancellationToken cancellationToken)
    {
        if (!options.Value.IsEnabled)
            return new NukiSmartLockAuthorizationContext([]);

        var authorizations = await httpClient.GetFromJsonAsync<IEnumerable<NukiSmartLockAuthorization>>("smartlock/auth", cancellationToken);
        var tuples = authorizations
            .Filter(authorization => smartlockIds.Contains(authorization.SmartLockId) && authorization.Type is entryCodeAuthType)
            .Map(authorization => (Authorization: authorization, NameOption: ParseName(authorization.Name)))
            .Filter(tuple => tuple.NameOption.IsSome)
            .Map(tuple => (
                Authorization: new SmartLockAuthorization(
                    smartlockMap[tuple.Authorization.SmartLockId],
                    tuple.NameOption.Match(orderId => orderId, () => new()),
                    Instant.FromDateTimeUtc(tuple.Authorization.AllowedFromDate),
                    Instant.FromDateTimeUtc(tuple.Authorization.AllowedUntilDate),
                    EntryCode.FromString(tuple.Authorization.Code.ToString(CultureInfo.InvariantCulture))),
                tuple.Authorization.Id))
            .ToSeq();
        logger.LogDebug("Got {Count} smart lock authorizations", tuples.Count);
        return new NukiSmartLockAuthorizationContext(tuples);
    }

    public async Task<Unit> SynchronizeSmartLockAuthorizationSet(
        ISmartLockAuthorizationContext smartLockAuthorizationContext, CancellationToken cancellationToken)
    {
        if (!options.Value.IsEnabled)
            return unit;

        var context = (NukiSmartLockAuthorizationContext) smartLockAuthorizationContext;
        logger.LogDebug("Synchronizing smart lock authorizations");
        foreach (var (authorization, id) in context.ToDelete)
            await DeleteSmartLockAuthorization(id, authorization, cancellationToken);
        foreach (var authorization in context.AuthorizationsToAdd)
            await CreateSmartLockAuthorization(authorization, cancellationToken);
        return unit;
    }

    async ValueTask DeleteSmartLockAuthorization(string id, SmartLockAuthorization authorization, CancellationToken cancellationToken)
    {
        var from = LocalDateTimePattern.GeneralIso.Format(timeConverter.GetTime(authorization.FromTimestamp));
        var to = LocalDateTimePattern.GeneralIso.Format(timeConverter.GetTime(authorization.ToTimestamp));
        logger.LogInformation("Deleting Nuki auth for '{Resource}' from {From} to {To}", Resources.GetNameUnsafe(authorization.ResourceId), from, to);
        var smartLockId = GetSmartLockId(authorization);
        var response = await httpClient.DeleteAsync($"smartlock/{smartLockId}/auth/{id}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    async ValueTask CreateSmartLockAuthorization(SmartLockAuthorization authorization, CancellationToken cancellationToken)
    {
        var from = LocalDateTimePattern.GeneralIso.Format(timeConverter.GetTime(authorization.FromTimestamp));
        var to = LocalDateTimePattern.GeneralIso.Format(timeConverter.GetTime(authorization.ToTimestamp));
        logger.LogInformation("Creating Nuki auth for '{Resource}' from {From} to {To}", Resources.GetNameUnsafe(authorization.ResourceId), from, to);
        var request = new
        {
            name = FormatName(authorization),
            allowedFromDate = authorization.FromTimestamp.ToDateTimeUtc(),
            allowedUntilDate = authorization.ToTimestamp.ToDateTimeUtc(),
            allowedWeekDays = allWeekdays,
            type = entryCodeAuthType,
            code = int.Parse(authorization.EntryCode.ToString(), CultureInfo.InvariantCulture),
        };
        var smartLockId = GetSmartLockId(authorization);
        var response = await httpClient.PutAsJsonAsync($"smartlock/{smartLockId}/auth", request, cancellationToken: cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    ulong GetSmartLockId(SmartLockAuthorization authorization) =>
        smartlockMap.Find(tuple => tuple.Value == authorization.ResourceId).Match(tuple => tuple.Key, () => throw new UnreachableException());

    static string FormatName(SmartLockAuthorization authorization) =>
        $"Bestilling {authorization.OrderId}";

    static Option<OrderId> ParseName(string name) =>
        NameRegex.Match(name) switch
        {
            { Success: true } match => OrderId.FromInt32(int.Parse(match.Groups["orderId"].Value, CultureInfo.InvariantCulture)),
            _ => None,
        };
}
