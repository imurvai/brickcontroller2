using BrickController2.Settings;
using BrickController2.UI.Commands;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Input;

namespace BrickController2.UI.ViewModels.Settings;

public abstract class SettingsPageViewModelBase : PageViewModelBase
{
    private CancellationTokenSource? _disappearingTokenSource;

    protected SettingsPageViewModelBase(
        INavigationService navigationService,
        ITranslationService translationService,
        IDialogService dialogService,
        IEnumerable<NamedSetting> settings) : base(navigationService, translationService)
    {
        DialogService = dialogService;
        // prepare grouping
        Groups = new(settings
            .OrderBy(x => x.Name)
            .GroupBy(x => x.Group)
            .Select(x => new SettingGroupViewModel(x.Key, x.Select(ToViewModel).ToArray(), translationService)));
        AllSettings = new(Groups.SelectMany(x => x));
        // detect grouping
        IsGrouped = settings.Any(x => !string.IsNullOrEmpty(x.Group));
        // subscribe to change coming from via groups 
        foreach (INotifyPropertyChanged group in Groups)
        {
            group.PropertyChanged += Group_PropertyChanged;
        }

        ResetToDefaultsCommand = new SafeCommand(ResetToDefaults, () => AllSettings.Any(x => x.HasNonDefaultValue));
        ResetGroupToDefaultCommand = new SafeCommand<SettingGroupViewModel>(ResetGroupToDefaults,
            (o) => o is SettingGroupViewModel group && group.HasNonDefaultValue);
    }

    public ICommand ResetToDefaultsCommand { get; }
    public ICommand ResetGroupToDefaultCommand { get; }

    public bool IsGrouped { get; }
    public IEnumerable<INotifyPropertyChanged> Settings => IsGrouped ? Groups : AllSettings;
    public CancellationToken DisappearingToken => _disappearingTokenSource?.Token ?? default;

    protected ObservableCollection<SettingViewModelBase> AllSettings { get; }
    protected ObservableCollection<SettingGroupViewModel> Groups { get; }
    public IDialogService DialogService { get; }

    public override void OnAppearing()
    {
        _disappearingTokenSource?.Cancel();
        _disappearingTokenSource = new CancellationTokenSource();
    }

    public override void OnDisappearing()
    {
        _disappearingTokenSource?.Cancel();
    }

    protected virtual void OnSettingChanged()
    {
    }

    protected virtual void OnDefaultValueChanged()
    {
        ResetToDefaultsCommand.RaiseCanExecuteChanged();
        ResetGroupToDefaultCommand.RaiseCanExecuteChanged();
    }

    private void Group_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SettingGroupViewModel.HasChanged))
        {
            OnSettingChanged();
        }
        else if (e.PropertyName == nameof(SettingGroupViewModel.HasNonDefaultValue))
        {
            OnDefaultValueChanged();
        }
    }

    private SettingViewModelBase ToViewModel(NamedSetting setting)
    {
        if (setting.IsBoolType)
        {
            return new BoolSettingViewModel(setting, this, TranslationService);
        }
        if (setting.IsEnumType)
        {
            return new EnumSettingViewModel(setting, this, TranslationService);
        }

        throw new InvalidOperationException($"The specified type {setting.Type} is not supported.");
    }

    private void ResetToDefaults() => ResetToDefaults(AllSettings);

    private static void ResetGroupToDefaults(SettingGroupViewModel group) => ResetToDefaults(group);

    private static void ResetToDefaults(ICollection<SettingViewModelBase> viewModels)
    {
        foreach (var setting in viewModels.Where(s => s.HasNonDefaultValue))
        {
            setting.ResetToDefault();
        }
    }
}
