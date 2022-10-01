using System.Collections;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Infrastructure.Persistence;

class CompositeIndices : IEnumerable<CompositeIndex>
{
    readonly LanguageExt.HashSet<CompositeIndex> indices;

    public CompositeIndices() => indices = new LanguageExt.HashSet<CompositeIndex>();

    public CompositeIndices(IEnumerable<CompositeIndex> indices) => this.indices = new LanguageExt.HashSet<CompositeIndex>(indices);

    CompositeIndices(LanguageExt.HashSet<CompositeIndex> indices) => this.indices = indices;

    public IEnumerator<CompositeIndex> GetEnumerator() => indices.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public CompositeIndices AddIndex(CompositeIndex index) => new(indices.TryAdd(index));

    public override string ToString() => string.Join(", ", indices.Select(path => path.ToString()));
}
