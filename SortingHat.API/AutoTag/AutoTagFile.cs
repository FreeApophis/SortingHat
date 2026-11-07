using System;
using System.Collections.Generic;
using System.IO;
using Funcky.Extensions;

namespace SortingHat.API.AutoTag;

/// <summary>The file hat fills the variables of a mask for. It remembers what the auto-tags read from it, so a file is read once per fact.</summary>
public sealed class AutoTagFile(FileInfo info)
{
    private readonly Dictionary<(string Key, Type Type), object> _facts = [];

    public FileInfo Info { get; } = info;

    /// <summary>
    /// The fact named <paramref name="key"/>, read with <paramref name="read"/> the first time any variable asks for it.
    /// Use the plugin's name as a prefix of the key, such as <c>Exif.Metadata</c>.
    /// </summary>
    public T Remember<T>(string key, Func<FileInfo, T> read)
        where T : notnull
        => _facts.GetValueOrNone((key, typeof(T))).Match(
            none: () => Store(key, read(Info)),
            some: fact => (T)fact);

    private T Store<T>(string key, T fact)
        where T : notnull
    {
        _facts[(key, typeof(T))] = fact;

        return fact;
    }
}
