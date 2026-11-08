using System.Threading;
using System.Threading.Tasks;

namespace SortingHat.API.Events;

/// <summary>
/// Hears about a <typeparamref name="TEvent"/>, such as <see cref="FileTagged"/>, after hat has stored the change.
/// Register implementations as <c>IFileEventHandler&lt;TEvent&gt;</c> in a module; one class can handle several events.
/// hat calls the handlers one after the other. When one throws, hat warns, the change stays and the other handlers still run.
/// </summary>
public interface IFileEventHandler<in TEvent>
{
    Task Handle(TEvent fileEvent, CancellationToken cancellationToken);
}
