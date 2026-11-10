using System.Collections.Generic;

namespace SortingHat.Desktop.Core;

/// <summary>The state after a message, and what the shell should do next.</summary>
internal sealed record Transition(MainState State, IReadOnlyList<Effect> Effects);
