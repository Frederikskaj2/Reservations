using LanguageExt;

namespace Frederikskaj2.Reservations.Persistence;

public static class EntityExtension
{
    public static Seq<T> ToValues<T>(this Seq<ETaggedEntity<T>> seq) =>
        seq.Map(entity => entity.Value);
}
