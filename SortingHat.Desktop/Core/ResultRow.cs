using System;
using System.IO;
using Apophis.CLI;
using Funcky.Monads;
using SortingHat.API.Models;

namespace SortingHat.Desktop.Core;

/// <summary>A found file as a row of the result list.</summary>
/// <param name="File">The file at the path it was found at.</param>
/// <param name="Name">The file name of the path.</param>
/// <param name="Folder">The folder of the path.</param>
/// <param name="Size">The size, such as "12 KB".</param>
/// <param name="CreatedAt">When the content was first seen, in UTC; the view shows it in local time.</param>
internal sealed record ResultRow(FileAt File, string Name, string Folder, string Size, DateTime CreatedAt)
{
    public static ResultRow From(FileAt file)
        => new(file, Path.GetFileName(file.Path), Option.FromNullable(Path.GetDirectoryName(file.Path)).GetOrElse(string.Empty), file.File.Size.HumanSize().TrimEnd(), file.File.CreatedAt);
}
