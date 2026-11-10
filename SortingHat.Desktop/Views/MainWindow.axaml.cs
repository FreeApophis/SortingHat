using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Funcky.Monads;
using SortingHat.Desktop.Core;

namespace SortingHat.Desktop.Views;

/// <summary>Only turns what the user does into calls of the <see cref="MainViewModel"/>; the bindings show the state.</summary>
internal sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnQueryKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            WithViewModel(viewModel => viewModel.SearchNow());
            e.Handled = true;
        }
    }

    private void OnCompletionTapped(object? sender, TappedEventArgs e)
    {
        if (Completions.SelectedItem is string completion)
        {
            WithViewModel(viewModel => viewModel.Complete(completion));
            QueryBox.Focus();
            QueryBox.CaretIndex = completion.Length;
        }
    }

    private void OnResultSelectionChanged(object? sender, SelectionChangedEventArgs e)
        => WithViewModel(viewModel => viewModel.Select(Results.SelectedItem is ResultRow row ? Option.Some(row) : Option<ResultRow>.None));

    private void OnResultDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (Results.SelectedItem is ResultRow row)
        {
            WithViewModel(viewModel => viewModel.Open(row));
        }
    }

    private void OnOpenClick(object? sender, RoutedEventArgs e)
        => WithViewModel(viewModel => viewModel.OpenSelected());

    private void OnTagSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (TagTree.SelectedItem is TagTreeNode node)
        {
            WithViewModel(viewModel => viewModel.ChooseTag(node));
        }
    }

    private void OnRenameTagClick(object? sender, RoutedEventArgs e)
        => StartTagEdit(sender, (viewModel, node) => viewModel.StartRename(node));

    private void OnMoveTagClick(object? sender, RoutedEventArgs e)
        => StartTagEdit(sender, (viewModel, node) => viewModel.StartMove(node));

    private void OnRemoveTagClick(object? sender, RoutedEventArgs e)
        => StartTagEdit(sender, (viewModel, node) => viewModel.StartRemove(node));

    private void OnTagEditKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key is Key.Enter or Key.Escape)
        {
            WithViewModel(viewModel => (e.Key == Key.Enter ? (Action)viewModel.ConfirmTagEdit : viewModel.CancelTagEdit)());
            e.Handled = true;
        }
    }

    private void OnTagEditConfirmClick(object? sender, RoutedEventArgs e)
        => WithViewModel(viewModel => viewModel.ConfirmTagEdit());

    private void OnTagEditCancelClick(object? sender, RoutedEventArgs e)
        => WithViewModel(viewModel => viewModel.CancelTagEdit());

    /// <summary>The menu item belongs to the tag it was opened on; the edit bar gets the focus.</summary>
    private void StartTagEdit(object? sender, Action<MainViewModel, TagTreeNode> start)
    {
        if (sender is MenuItem { DataContext: TagTreeNode node })
        {
            WithViewModel(viewModel => start(viewModel, node));
            TagEditText.Focus();
            TagEditText.SelectAll();
        }
    }

    private void WithViewModel(Action<MainViewModel> act)
    {
        if (DataContext is MainViewModel viewModel)
        {
            act(viewModel);
        }
    }
}
