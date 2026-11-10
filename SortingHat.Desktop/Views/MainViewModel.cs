using System.Collections.Generic;
using System.ComponentModel;
using Funcky.Extensions;
using JetBrains.Annotations;
using SortingHat.Desktop.Shell;

namespace SortingHat.Desktop.Views;

/// <summary>The main window's view of the state: read-only projections for the bindings, and the start.</summary>
[UsedImplicitly]
internal sealed class MainViewModel : INotifyPropertyChanged
{
    private static readonly IReadOnlyList<string> Projections = [nameof(Messages), nameof(HasMessages)];

    private readonly Store _store;

    public MainViewModel(Store store)
    {
        _store = store;
        store.Subscribe(_ => Projections.ForEach(name => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name))));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public IReadOnlyList<string> Messages
        => _store.State.Messages;

    public bool HasMessages
        => _store.State.Messages.Count > 0;

    public void Start()
        => _store.Start();
}
