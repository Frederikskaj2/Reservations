using System.Collections;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Persistence;

class CompositeIndices : IEnumerable<CompositeIndex>
{
    readonly LanguageExt.HashSet<CompositeIndex> indices;

    public CompositeIndices() => indices = [];

    public CompositeIndices(IEnumerable<CompositeIndex> indices) => this.indices = new(indices);

    CompositeIndices(LanguageExt.HashSet<CompositeIndex> indices) => this.indices = indices;

    // ReSharper disable once NotDisposedResourceIsReturned
    public IEnumerator<CompositeIndex> GetEnumerator() => indices.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public CompositeIndices AddIndex(CompositeIndex index) => new(indices.TryAdd(index));

    public override string ToString() => string.Join(", ", indices.Select(path => path.ToString()));
}
