using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SortingHat.API.Events;

/// <summary>Tells every <see cref="IFileEventHandler{TEvent}"/> about a <typeparamref name="TEvent"/>: for hosts, after they stored a change.</summary>
public interface IFileEvents<in TEvent>
{
    /// <returns>A warning for every handler that failed; the host shows them.</returns>
    Task<IReadOnlyList<string>> Publish(TEvent fileEvent, CancellationToken cancellationToken);
}
