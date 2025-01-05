using BrickController2.Settings;
using BrickController2.UI.Services.Translation;

namespace BrickController2.UI.ViewModels.Settings;

public class BoolSettingViewModel : SettingViewModelBase<bool>
{
    public BoolSettingViewModel(NamedSetting setting,
        SettingsPageViewModelBase parent,
        ITranslationService translationService)
        : base(setting, parent, translationService)
    {
    }

    public override bool Value
    {
        get => (bool)SettingValue;
        set => SettingValue = value!;
    }
}
