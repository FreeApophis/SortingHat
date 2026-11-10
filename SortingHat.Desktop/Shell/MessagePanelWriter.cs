using System;
using Apophis.CLI.Writer;
using Avalonia.Threading;
using JetBrains.Annotations;
using SortingHat.Desktop.Core;

namespace SortingHat.Desktop.Shell;

/// <summary>What plugins write goes to the message panel of the window. Plugins may write from any thread.</summary>
[UsedImplicitly]
internal sealed class MessagePanelWriter(Lazy<Store> store) : IConsoleWriter
{
    public void WriteLine(string line)
        => Dispatcher.UIThread.Post(() => store.Value.Dispatch(new Message.Wrote(line)));

    public void WriteLine()
        => WriteLine(string.Empty);
}
