using LanguageExt;
using System;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.LockBox;

public static class Resources
{
    public static Seq<Resource> All { get; } = new[]
    {
        new Resource(ResourceId.FromInt32(1), 0, ResourceType.BanquetFacilities, "Festlokale"),
        new Resource(ResourceId.FromInt32(2), 2, ResourceType.Bedroom, "Frederik (soveværelse)"),
        new Resource(ResourceId.FromInt32(3), 1, ResourceType.Bedroom, "Kaj (soveværelse)"),
    }.ToSeq();

    static readonly HashMap<ResourceId, Resource> hashMap = toHashMap(All.Map(resource => (resource.ResourceId, resource)));

    public static Option<string> GetName(ResourceId resourceId) =>
        hashMap.Find(resourceId).Map(resource => resource.Name);

    public static string GetNameUnsafe(ResourceId resourceId) =>
        hashMap.Find(resourceId).Map(resource => resource.Name).Match(name => name, () => throw new InvalidOperationException());

    public static Option<ResourceType> GetResourceType(ResourceId resourceId) =>
        hashMap.Find(resourceId).Map(resource => resource.Type);
}
