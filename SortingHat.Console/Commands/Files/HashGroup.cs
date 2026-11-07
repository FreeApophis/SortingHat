using System.Collections.Generic;
using SortingHat.API.Models;

namespace SortingHat.CLI.Commands.Files;

/// <summary>The paths of one content.</summary>
/// <param name="Hash">The hash of the content.</param>
/// <param name="Files">The content at each of its paths.</param>
internal sealed record HashGroup(Hash Hash, IReadOnlyList<FileAt> Files);
