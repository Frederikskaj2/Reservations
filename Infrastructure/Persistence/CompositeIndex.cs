using LanguageExt;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Infrastructure.Persistence;

class CompositeIndex : IEquatable<CompositeIndex>, IEnumerable<CompositeIndexPath>
{
    readonly Seq<CompositeIndexPath> paths;

    public CompositeIndex() => paths = new Seq<CompositeIndexPath>();

    public CompositeIndex(IEnumerable<CompositeIndexPath> paths) => this.paths = new Seq<CompositeIndexPath>(paths);

    CompositeIndex(Seq<CompositeIndexPath> paths) => this.paths = paths;

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

    public CompositeIndex AddPath(CompositeIndexPath path) => new(paths.Add(path));

    public override bool Equals(object? obj) => obj is CompositeIndex index && Equals(index);

    public override int GetHashCode() => paths.GetHashCode();

    public override string ToString() => $"({string.Join('|', paths.Select(path => path.ToString()))})";
}
