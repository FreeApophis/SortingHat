using System.Collections.Generic;
using Funcky.Monads;

namespace SortingHat.API.DI;

/// <summary>Finds the files named on the command line.</summary>
public interface IFilePathExtractor
{
    /// <summary>
    /// The full paths of the files the <paramref name="filePatterns"/> name, in the case of the file system and each once,
    /// or one problem for every argument that names no file. A pattern such as 'C:\scans\*.pdf' has its wildcards in the file name.
    /// A folder means every file in it and needs <paramref name="recursive"/>, which also searches the subfolders of a pattern.
    /// </summary>
    Either<IReadOnlyList<string>, IReadOnlyList<string>> FromFilePatterns(IEnumerable<string> filePatterns, bool recursive);
}
