using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Funcky.Extensions;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.API.DI;
using SortingHat.API.Models;

namespace SortingHat.Plugin.MoveOnTag;

/// <summary>Keeps the move rules as JSON under one key of the settings of hat, so they apply in every project.</summary>
[UsedImplicitly]
internal sealed class MoveRuleStore(ISettings settings)
{
    private const string Key = "MoveOnTag.Rules";

    public MoveRules Load()
        => settings.Get(Key).Select(Parse).GetOrElse(MoveRules.Empty);

    public void Save(MoveRules rules)
        => settings.Set(Key, JsonSerializer.Serialize(rules.Rules.Select(rule => new StoredRule(rule.Tag.FullName, rule.Folder))));

    /// <summary>A stored tag that is no tag any more is left out.</summary>
    private static MoveRules Parse(string json)
        => new([.. Stored(json).WhereSelect(stored => Tag.Parse(stored.Tag).RightOrNone().Select(tag => new MoveRule(tag, stored.Folder)))]);

    private static IEnumerable<StoredRule> Stored(string json)
        => Option.FromNullable(JsonSerializer.Deserialize<List<StoredRule>>(json)).GetOrElse([]);

    private sealed record StoredRule(string Tag, string Folder);
}
