using System.Collections.Generic;
using System.ComponentModel;
using Funcky.Extensions;
using Funcky.Monads;
using JetBrains.Annotations;
using SortingHat.Desktop.Core;
using SortingHat.Desktop.Shell;
using static Funcky.Functional;

namespace SortingHat.Desktop.Views;

/// <summary>The main window's view of the state: read-only projections for the bindings, and the user's actions as messages.</summary>
[UsedImplicitly]
internal sealed class MainViewModel : INotifyPropertyChanged
{
    private static readonly IReadOnlyList<string> Projections =
    [
        nameof(Messages), nameof(HasMessages), nameof(Query), nameof(QueryProblem), nameof(HasQueryProblem), nameof(IsQueryIncomplete),
        nameof(Completions), nameof(HasCompletions), nameof(Results), nameof(Summary), nameof(HasDetails), nameof(DetailTags),
        nameof(DetailNames), nameof(DetailPaths), nameof(DetailHash), nameof(DetailSize), nameof(TagTree), nameof(HasTagEdit),
        nameof(TagEditPrompt), nameof(TagEditText), nameof(TagEditHasText), nameof(TagEditAction), nameof(TagEditProblem), nameof(HasTagEditProblem),
    ];

    private readonly Store _store;

    public MainViewModel(Store store)
    {
        _store = store;
        store.Subscribe(_ => Projections.ForEach(name => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name))));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public IReadOnlyList<string> Messages => _store.State.Messages;

    public bool HasMessages => Messages.Count > 0;

    /// <summary>Two-way: what the user types becomes a <see cref="Message.QueryChanged"/>.</summary>
    public string Query
    {
        get => Search.Query;
        set => ChangeQuery(value);
    }

    public string QueryProblem => Search.Problem.Match(none: string.Empty, some: problem => problem.Reason);

    public bool HasQueryProblem => Search.Problem.Match(none: false, some: True);

    public bool IsQueryIncomplete => Search.Problem.Match(none: false, some: problem => problem.IsIncomplete);

    public IReadOnlyList<string> Completions => Search.Completions;

    public bool HasCompletions => Completions.Count > 0;

    public IReadOnlyList<ResultRow> Results => Search.Results;

    public string Summary => Search.Summary;

    public bool HasDetails => Search.Details.Match(none: false, some: True);

    public IReadOnlyList<string> DetailTags => Search.Details.Match(none: [], some: details => details.Tags);

    public IReadOnlyList<string> DetailNames => Search.Details.Match(none: [], some: details => details.Names);

    public IReadOnlyList<string> DetailPaths => Search.Details.Match(none: [], some: details => details.Paths);

    public string DetailHash => Search.Details.Match(none: string.Empty, some: details => details.Hash.ToString());

    public string DetailSize => Search.Details.Match(none: string.Empty, some: details => $"{details.Size:N0} bytes, first seen {details.CreatedAt.ToLocalTime():g}");

    public IReadOnlyList<TagTreeNode> TagTree => Tags.Tree;

    public bool HasTagEdit => Tags.Edit.Match(none: false, some: True);

    public string TagEditPrompt => Tags.Edit.Match(none: string.Empty, some: edit => edit.Prompt);

    /// <summary>Two-way: the new name or the destination the user types.</summary>
    public string TagEditText
    {
        get => Tags.Edit.Match(none: string.Empty, some: EditText);
        set => ChangeTagEditText(value);
    }

    public bool TagEditHasText => Tags.Edit.Match(none: false, some: edit => edit is not TagEdit.Removing);

    public string TagEditAction => Tags.Edit.Match(none: string.Empty, some: edit => edit.Action);

    public string TagEditProblem => Tags.Problem.GetOrElse(string.Empty);

    public bool HasTagEditProblem => Tags.Problem.Match(none: false, some: True);

    private SearchState Search => _store.State.Search;

    private TagsState Tags => _store.State.Tags;

    public void Start() => _store.Start();

    public void SearchNow() => _store.Dispatch(new Message.SearchRequested());

    public void Complete(string completion) => _store.Dispatch(new Message.QueryChanged(completion));

    public void Select(Option<ResultRow> row) => _store.Dispatch(new Message.ResultSelected(row.Select(selected => selected.File)));

    public void OpenSelected() => Search.Selected.AndThen(file => _store.Dispatch(new Message.OpenRequested(file)));

    public void Open(ResultRow row) => _store.Dispatch(new Message.OpenRequested(row.File));

    public void ChooseTag(TagTreeNode node) => _store.Dispatch(new Message.TagChosen(node.Tag));

    public void StartRename(TagTreeNode node) => _store.Dispatch(new Message.TagEditStarted(new TagEdit.Renaming(node.Tag, node.Name)));

    public void StartMove(TagTreeNode node) => _store.Dispatch(new Message.TagEditStarted(new TagEdit.Moving(node.Tag, node.Tag.Parent.Match(none: ":", some: parent => parent.FullName))));

    public void StartRemove(TagTreeNode node) => _store.Dispatch(new Message.TagEditStarted(new TagEdit.Removing(node.Tag)));

    public void ConfirmTagEdit() => _store.Dispatch(new Message.TagEditConfirmed());

    public void CancelTagEdit() => _store.Dispatch(new Message.TagEditCancelled());

    private static string EditText(TagEdit edit)
        => edit switch
        {
            TagEdit.Renaming renaming => renaming.Text,
            TagEdit.Moving moving => moving.Text,
            TagEdit.Removing => string.Empty,
        };

    /// <summary>The binding writes the query back after every change; only a new query is a message.</summary>
    private void ChangeQuery(string query)
    {
        if (query != Search.Query)
        {
            _store.Dispatch(new Message.QueryChanged(query));
        }
    }

    private void ChangeTagEditText(string text)
    {
        if (text != TagEditText)
        {
            _store.Dispatch(new Message.TagEditTextChanged(text));
        }
    }
}
