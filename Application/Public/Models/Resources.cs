using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using System;
using System.Collections.Generic;
using System.Linq;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Application;

public static class Resources
{
    static readonly IReadOnlyDictionary<ResourceId, Resource> resources = new[]
    {
        new Resource(ResourceId.FromInt32(1), 0, ResourceType.BanquetFacilities, "Aktivitets-/festlokale"),
        new Resource(ResourceId.FromInt32(2), 2, ResourceType.Bedroom, "Frederik (soveværelse)"),
        new Resource(ResourceId.FromInt32(3), 1, ResourceType.Bedroom, "Kaj (soveværelse)")
    }.ToDictionary(resource => resource.ResourceId);

    public static string Name(ResourceId resourceId) => GetAll()[resourceId].Name;

    public static Func<ResourceId, Option<ResourceType>> GetResourceType => resourceId =>
        GetAll().TryGetValue(resourceId, out var resource) ? resource.Type : None;

    public static IReadOnlyDictionary<ResourceId, Resource> GetAll() => resources;
}
