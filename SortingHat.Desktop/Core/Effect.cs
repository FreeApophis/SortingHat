namespace SortingHat.Desktop.Core;

/// <summary>Something <see cref="MainUpdate.Update"/> wants done outside the pure core, such as reading the database; the shell runs it and reports back with messages.</summary>
internal abstract record Effect
{
    private Effect()
    {
    }

    public sealed record LoadStartupWarnings : Effect;
}
