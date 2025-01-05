using BrickController2.UI.Services.Translation;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace BrickController2.UI.ViewModels.Settings;

public class SettingGroupViewModel : ObservableCollection<SettingViewModelBase>
{
    private const string DefaultGroupName = "_defaultGroup";

    private readonly string _groupName;
    private readonly ITranslationService _translationService;
    private bool _changed;
    private bool _nonDefaultValue;

    public SettingGroupViewModel(string groupName,
        ICollection<SettingViewModelBase> settings,
        ITranslationService translationService)
        : base(settings)
    {
        _groupName = !string.IsNullOrEmpty(groupName) ? groupName : DefaultGroupName;
        _translationService = translationService;
        _nonDefaultValue = settings.Any(x => x.HasNonDefaultValue);
        // subscribe to changes - collection is expected to be immutable
        foreach (var setting in settings)
        {
            setting.PropertyChanged += Setting_PropertyChanged;
        }
    }

    public bool HasChanged
    {
        get => _changed;
        protected set
        {
            if (_changed != value)
            {
                _changed = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(HasChanged)));
            }
        }
    }

    public bool HasNonDefaultValue
    {
        get => _nonDefaultValue;
        protected set
        {
            if (_nonDefaultValue != value)
            {
                _nonDefaultValue = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(HasNonDefaultValue)));
            }
        }
    }

    public string GroupName => _translationService.Translate(_groupName);

    private void Setting_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // notify group due to possible change 
        if (e.PropertyName == nameof(SettingViewModelBase.HasChanged))
        {
            HasChanged = this.Any(x => x.HasChanged);
        }
        else if (e.PropertyName == nameof(SettingViewModelBase.HasNonDefaultValue))
        {
            HasNonDefaultValue = this.Any(x => x.HasNonDefaultValue);
        }
    }
}
