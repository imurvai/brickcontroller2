using BrickController2.Helpers;
using BrickController2.Settings;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Translation;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrickController2.UI.ViewModels.Settings;

public abstract class SettingViewModelBase : NotifyPropertyChangedSource
{
    protected readonly object OriginalValue;

    private bool _changed;
    private bool _hasDefaultValue;

    protected SettingViewModelBase(NamedSetting setting)
    {
        OriginalValue = setting.Value;
        // make editable copy
        Setting = setting with { };
        // initialize flag(s)
        _hasDefaultValue = !setting.Value.Equals(Setting.DefaultValue);
    }

    public bool HasChanged
    {
        get => _changed;
        protected set
        {
            if (_changed != value)
            {
                _changed = value;
                RaisePropertyChanged();
            }
        }
    }

    public bool HasNonDefaultValue
    {
        get => _hasDefaultValue;
        protected set
        {
            if (_hasDefaultValue != value)
            {
                _hasDefaultValue = value;
                RaisePropertyChanged();
            }
        }
    }

    public NamedSetting Setting { get; }

    protected object SettingValue
    {
        get => Setting.Value;
        set
        {
            if (!Setting.Value.Equals(value))
            {
                Setting.Value = value!;
                RaisePropertyChanged(nameof(SettingViewModelBase<int>.Value));
                // update additional properties
                HasChanged = !value.Equals(OriginalValue);
                HasNonDefaultValue = !value.Equals(Setting.DefaultValue);
            }
        }
    }

    public virtual void ResetToDefault()
    {
        SettingValue = Setting.DefaultValue;
    }
}

public abstract class SettingViewModelBase<TValue> : SettingViewModelBase
{
    protected readonly SettingsPageViewModelBase Parent;
    protected readonly ITranslationService TranslationService;

    protected SettingViewModelBase(NamedSetting setting,
        SettingsPageViewModelBase parent,
        ITranslationService translationService) : base(setting)
    {
        Parent = parent;
        TranslationService = translationService;
    }

    public string DisplayName => TranslationService.Translate(Setting.Name);

    public abstract TValue Value { get; set; }

    protected Task<SelectionDialogResult<T>> ShowSelectionDialogAsync<T>(IEnumerable<T> items) where T : notnull
        => Parent.DialogService.ShowSelectionDialogAsync(
            items,
            TranslationService.Translate(Setting.Name),
            TranslationService.Translate("Cancel"),
            Parent.DisappearingToken);
}
