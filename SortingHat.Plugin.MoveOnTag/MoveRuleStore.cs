using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Funcky.Extensions;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API.Models;
using SortingHat.API.Plugin;

namespace SortingHat.Plugin.MoveOnTag;

/// <summary>Keeps the move rules as JSON in the settings of the active project: rules name tags, and tags belong to a project.</summary>
[UsedImplicitly]
internal sealed class MoveRuleStore(IPluginSettings<MoveOnTagModule> settings)
{
    private const string Key = "Rules";

    public MoveRules Load()
        => settings.Project.Get(Key).Select(Parse).GetOrElse(MoveRules.Empty);

    public void Save(MoveRules rules)
        => settings.Project.Set(Key, JsonSerializer.Serialize(rules.Rules.Select(rule => new StoredRule(rule.Tag.FullName, rule.Folder))));

    /// <summary>A stored tag that is no tag any more is left out.</summary>
    private static MoveRules Parse(string json)
        => new([.. Stored(json).WhereSelect(stored => Tag.Parse(stored.Tag).RightOrNone().Select(tag => new MoveRule(tag, stored.Folder)))]);

    private static IEnumerable<StoredRule> Stored(string json)
        => Option.FromNullable(JsonSerializer.Deserialize<List<StoredRule>>(json)).GetOrElse([]);

    private sealed record StoredRule(string Tag, string Folder);
}
