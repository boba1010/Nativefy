using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Nativefy.Models;

public partial class SidebarGroup : ObservableObject
{
    public string HeaderText { get; set; } = string.Empty;

    public string GroupTag { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ChevronGlyph))]
    public partial bool IsExpanded { get; set; } = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasCompactMode))]
    public partial string? CompactGlyph { get; set; }

    public ICommand? CompactCommand { get; set; }
    
    public bool HasCompactMode => !string.IsNullOrEmpty(CompactGlyph) && CompactCommand is not null;
    
    public string ChevronGlyph => IsExpanded ? "\uE70D" : "\uE70E";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasAction))]
    public partial string? ActionGlyph { get; set; }

    public ICommand? ActionCommand { get; set; }

    public bool HasAction => !string.IsNullOrEmpty(ActionGlyph) && ActionCommand is not null;

    public ObservableCollection<SidebarMenuItem> Items { get; set; } = [];
}