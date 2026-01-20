using IconPacks.Avalonia.Material;
using ReQuantum.Assets.I18n;
using ReQuantum.Infrastructure.Abstractions;
using ReQuantum.Infrastructure.Entities;
using ReQuantum.Infrastructure.Services;
using ReQuantum.Modules.Common.Attributes;
using ReQuantum.Modules.Menu.Abstractions;
using ReQuantum.Services;
using ReQuantum.Views;
using System;
using System.Diagnostics.CodeAnalysis;

namespace ReQuantum.ViewModels;

[AutoInject(Lifetime.Singleton, RegisterTypes = [typeof(MyNotesViewModel), typeof(IMenuItemProvider)])]
public partial class MyNotesViewModel : ViewModelBase<MyNotesView>, IMenuItemProvider
{

    #region MenuItemProvider APIs
    public MenuItem MenuItem { get; }
    public uint Order => 0;

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
    public Type ViewModelType => typeof(MyNotesViewModel);
    #endregion


    private readonly ILocalizer _localizer;


    public MyNotesViewModel(ILocalizer localizer)
    {
        MenuItem = new MenuItem
        {
            Title = new LocalizedText { Key = nameof(UIText.NotesWall) },
            IconKind = PackIconMaterialKind.BookOpenPageVariant,
            OnSelected = () => Navigator.NavigateTo<MyNotesViewModel>()
        };
        _localizer = localizer;
    }

    public void AddInputAt(double x, double y)
    {

    }
}