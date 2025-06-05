using Frederikskaj2.Reservations.Core;
using Liversage.Primitives;

namespace Frederikskaj2.Reservations.Users;

[Primitive(Features.Equatable | Features.Formattable)]
public readonly partial struct UserId : IIsId
{
    readonly int id;

    public string GetId() => ToString();

    public static readonly UserId System = new(0);
}
