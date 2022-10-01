using Frederikskaj2.Reservations.Shared.Core;
using Frederikskaj2.Reservations.Shared.Web;
using NodaTime;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client;

public class ClientDataProvider
{
    readonly AuthenticatedApiClient apiClient;
    Configuration? cachedConfiguration;
    IReadOnlyDictionary<ResourceId, Resource>? cachedResources;

    public ClientDataProvider(AuthenticatedApiClient apiClient, IDateProvider dateProvider)
    {
        this.apiClient = apiClient;
        Holidays = dateProvider.Holidays;
    }

    public MyOrder? CurrentOrder { get; set; }

    // TODO: Get rid of this and use IDateProvider directly.
    public IReadOnlySet<LocalDate> Holidays { get; }

    public async ValueTask<OrderingOptions?> GetOptionsAsync() => (await GetConfigurationAsync())?.Options;

    public async ValueTask<IReadOnlyDictionary<ResourceId, Resource>?> GetResourcesAsync()
    {
        if (cachedResources is not null)
            return cachedResources;
        var configuration = await GetConfigurationAsync();
        cachedResources = configuration?.Resources.ToDictionary(resource => resource.ResourceId);
        return cachedResources;
    }

    public async ValueTask<IEnumerable<Apartment>?> GetApartmentsAsync()
    {
        var configuration = await GetConfigurationAsync();
        return configuration?.Apartments;
    }

    public async ValueTask<IEnumerable<AccountName>?> GetAccountNamesAsync()
    {
        var configuration = await GetConfigurationAsync();
        return configuration?.AccountNames;
    }

    async ValueTask<Configuration?> GetConfigurationAsync()
    {
        if (cachedConfiguration is not null)
            return cachedConfiguration;
        var response = await apiClient.GetAsync<Configuration>("configuration");
        if (!response.IsSuccess)
            return null;
        cachedConfiguration = response.Result;
        return cachedConfiguration;
    }
}
