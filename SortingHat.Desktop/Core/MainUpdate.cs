using System;

namespace SortingHat.Desktop.Core;

/// <summary>The pure core of the main window: how a message changes the state, and which effects follow.</summary>
internal static class MainUpdate
{
    public static Transition Update(MainState state, Message message)
        => message switch
        {
            Message.StartupWarningsLoaded loaded => new(state with { Messages = [.. state.Messages, .. loaded.Warnings] }, []),
            Message.Wrote wrote => new(state with { Messages = [.. state.Messages, wrote.Line] }, []),
            _ => throw new ArgumentOutOfRangeException(nameof(message), message, "Every message is handled."),
        };
}
