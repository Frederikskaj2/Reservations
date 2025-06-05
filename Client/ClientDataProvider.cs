using Frederikskaj2.Reservations.Bank;
using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.LockBox;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Users;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client;

public sealed class ClientDataProvider(AuthenticatedApiClient apiClient) : IDisposable
{
    readonly SemaphoreSlim gate = new(1, 1);
    IReadOnlyDictionary<PostingAccount, string>? cachedAccountNames;
    IReadOnlyDictionary<ApartmentId, Apartment>? cachedApartments;
    GetConfigurationResponse? cachedConfiguration;
    IReadOnlyDictionary<ResourceId, Resource>? cachedResources;

    public MyOrderDto? CurrentOrder { get; set; }

    public void Dispose() => gate.Dispose();

    public async ValueTask<OrderingOptions?> GetOptions() => (await GetConfiguration())?.Options;

    public async ValueTask<IReadOnlyDictionary<ResourceId, Resource>> GetResources()
    {
        if (cachedResources is not null)
            return cachedResources;
        var configuration = await GetConfiguration();
        cachedResources = (IReadOnlyDictionary<ResourceId, Resource>?) configuration?.Resources.ToDictionary(resource => resource.ResourceId) ??
                          ReadOnlyDictionary<ResourceId, Resource>.Empty;
        return cachedResources;
    }

    public async ValueTask<IReadOnlyDictionary<ApartmentId, Apartment>> GetApartments()
    {
        if (cachedApartments is not null)
            return cachedApartments;
        var configuration = await GetConfiguration();
        cachedApartments = (IReadOnlyDictionary<ApartmentId, Apartment>?) configuration?.Apartments.ToDictionary(apartment => apartment.ApartmentId) ??
                           ReadOnlyDictionary<ApartmentId, Apartment>.Empty;
        return cachedApartments;
    }

    public async ValueTask<IReadOnlyDictionary<PostingAccount, string>?> GetAccountNames()
    {
        if (cachedAccountNames is not null)
            return cachedAccountNames;
        var configuration = await GetConfiguration();
        cachedAccountNames = configuration?.AccountNames.ToDictionary(accountName => accountName.Account, accountName => accountName.Name);
        return cachedAccountNames;
    }

    async ValueTask<GetConfigurationResponse?> GetConfiguration()
    {
        await gate.WaitAsync();
        try
        {
            if (cachedConfiguration is not null)
                return cachedConfiguration;
            var response = await apiClient.Get<GetConfigurationResponse>("configuration");
            if (!response.IsSuccess)
                return null;
            cachedConfiguration = response.Result;
            return cachedConfiguration;
        }
        finally
        {
            gate.Release();
        }
    }
}
