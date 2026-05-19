using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using Composer.Models;
using Composer.Views;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Composer.Views.Controls;

/// <summary>
/// Right rail (280px). Lists the nine files (eight per-layer + synthesized
/// prompt-context.md) with status dot and badge. Brief 03 layers on the
/// pulse animation for Writing.
/// </summary>
public sealed partial class FilesRail : UserControl
{
    private INotifyPropertyChanged? _vm;

    public FilesRail()
    {
        this.InitializeComponent();
        this.DataContextChanged += (_, args) => Attach(args.NewValue);
        this.Unloaded            += (_, _)    => Detach();
    }

    private void Attach(object? dc)
    {
        Detach();
        if (dc is INotifyPropertyChanged inpc)
        {
            _vm = inpc;
            _vm.PropertyChanged += OnVmPropertyChanged;
        }
        Render();
    }

    private void Detach()
    {
        if (_vm is null) return;
        _vm.PropertyChanged -= OnVmPropertyChanged;
        _vm = null;
    }

    private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "FileStatuses")
            DispatcherQueue?.TryEnqueue(Render);
    }

    private void Render()
    {
        var statuses = ReadFileStatuses(DataContext);
        var total = Composer.Models.Layers.All.Length;
        var rows = ImmutableArray.CreateBuilder<FileRow>(total + 2);
        var lockedCount = 0;
        for (var i = 0; i < total; i++)
        {
            var def = Composer.Models.Layers.All[i];
            var status = statuses.TryGetValue(def.Kind, out var s) ? s : FileStatus.Planned;
            if (status == FileStatus.Drafted) lockedCount++;
            rows.Add(FileRow.For(def.File, status));
        }
        // Synthesized companion files — emit with the Scaffold layer.
        rows.Add(FileRow.For(Composer.Models.Layers.ReadmeFileName,        FileStatus.Planned));
        rows.Add(FileRow.For(Composer.Models.Layers.PromptContextFileName, FileStatus.Planned));
        FileRows.ItemsSource = rows.MoveToImmutable();
        LockedSummary.Text = $"{lockedCount} OF {total} LOCKED";
    }

    private static IDictionary<LayerKind, FileStatus> ReadFileStatuses(object? dc)
    {
        if (dc is null) return new Dictionary<LayerKind, FileStatus>();
        var prop = dc.GetType().GetProperty("FileStatuses")?.GetValue(dc);
        if (prop is IDictionary<LayerKind, FileStatus> dict) return dict;
        if (prop is IReadOnlyDictionary<LayerKind, FileStatus> rdict)
        {
            var copy = new Dictionary<LayerKind, FileStatus>();
            foreach (var kv in rdict) copy[kv.Key] = kv.Value;
            return copy;
        }
        return new Dictionary<LayerKind, FileStatus>();
    }
}

public sealed record FileRow(
    string FileName,
    string StatusBadge,
    Brush StatusBadgeBrush,
    Brush DotFill,
    Brush DotStroke,
    double Opacity)
{
    public static FileRow For(string fileName, FileStatus status) => status switch
    {
        FileStatus.Drafted => new FileRow(fileName,
            StatusBadge:      "DRAFTED",
            StatusBadgeBrush: AppBrushes.Amber,
            DotFill:          AppBrushes.Amber,
            DotStroke:        AppBrushes.Amber,
            Opacity:          1.0),
        FileStatus.Writing => new FileRow(fileName,
            StatusBadge:      "WRITING",
            StatusBadgeBrush: AppBrushes.Indigo,
            DotFill:          AppBrushes.Indigo,
            DotStroke:        AppBrushes.Indigo,
            Opacity:          1.0),
        _ => new FileRow(fileName,
            StatusBadge:      "PLANNED",
            StatusBadgeBrush: AppBrushes.Ink4,
            DotFill:          AppBrushes.Transparent,
            DotStroke:        AppBrushes.Ink4,
            Opacity:          0.42),
    };
}
