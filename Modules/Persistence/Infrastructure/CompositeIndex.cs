using LanguageExt;
using System;
using System.Collections;
using System.Collections.Generic;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Persistence;

class CompositeIndex : IEquatable<CompositeIndex>, IEnumerable<CompositeIndexPath>
{
    readonly Seq<CompositeIndexPath> paths;

    public CompositeIndex() => paths = Empty;

    public CompositeIndex(IEnumerable<CompositeIndexPath> paths) => this.paths = new(paths);

    CompositeIndex(Seq<CompositeIndexPath> paths) => this.paths = paths;

    public CompositeIndex AddPath(CompositeIndexPath path) => new(paths.Add(path));

    // ReSharper disable once NotDisposedResourceIsReturned
    public IEnumerator<CompositeIndexPath> GetEnumerator() => paths.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool Equals(CompositeIndex? other)
    {
        if (ReferenceEquals(this, other))
            return true;
        if (other is null)
            return false;
        return paths == other.paths;
    }

    public override bool Equals(object? obj) => obj is CompositeIndex index && Equals(index);

    public override int GetHashCode() => paths.GetHashCode();

    public override string ToString() => $"({string.Join('|', paths.Select(path => path.ToString()))})";
}
