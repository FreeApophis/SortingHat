using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Funcky.Extensions;
using JetBrains.Annotations;
using SortingHat.Desktop.Core;

namespace SortingHat.Desktop.Shell;

/// <summary>
/// The imperative shell around <see cref="MainUpdate"/>: it keeps the current state, tells the view about every change
/// and runs the effects. Messages are dispatched on the UI thread; effects run in the background and report back there.
/// </summary>
[UsedImplicitly]
internal sealed class Store(EffectRunner effectRunner)
{
    private ImmutableList<Action<MainState>> _subscribers = [];
    private ImmutableList<Task> _running = [];

    public MainState State { get; private set; } = MainState.Initial;

    /// <summary><paramref name="changed"/> hears of every new state.</summary>
    public void Subscribe(Action<MainState> changed)
        => _subscribers = _subscribers.Add(changed);

    /// <summary>Runs what the window does when it opens.</summary>
    public void Start()
        => Run(MainState.Start);

    public void Dispatch(Message message)
    {
        var transition = MainUpdate.Update(State, message);

        State = transition.State;
        _subscribers.ForEach(changed => changed(State));
        Run(transition.Effects);
    }

    /// <summary>Completes once no effect runs any more, also those that started while waiting.</summary>
    public async Task Idle()
    {
        while (_running is [_, ..] running)
        {
            await Task.WhenAll(running);
            _running = _running.RemoveRange(running);
        }
    }

    private void Run(IReadOnlyList<Effect> effects)
        => _running = _running.AddRange(effects.Select(RunInBackground));

    /// <summary>The continuation returns to the UI thread, so the messages are dispatched there.</summary>
    private async Task RunInBackground(Effect effect)
        => (await Task.Run(() => effectRunner.Run(effect))).ForEach(Dispatch);
}
