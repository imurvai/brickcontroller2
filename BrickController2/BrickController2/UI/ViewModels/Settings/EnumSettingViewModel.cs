using BrickController2.Settings;
using BrickController2.UI.Commands;
using BrickController2.UI.Services.Translation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BrickController2.UI.ViewModels.Settings;

public class EnumSettingViewModel : SettingViewModelBase<string>
{
    public EnumSettingViewModel(NamedSetting setting,
        SettingsPageViewModelBase parent,
        ITranslationService translationService)
    : base(setting, parent, translationService)
    {
        SelectItemCommand = new SafeCommand(SelectItemAsync);
    }

    public IEnumerable<string> Items => Enum.GetNames(Setting.Type);
    public ICommand SelectItemCommand { get; }

    public override string Value
    {
        get => Enum.GetName(Setting.Type, SettingValue)!;
        set
        {
            var enumValue = Enum.Parse(Setting.Type, value);
            SettingValue = enumValue;
        }
    }

    public override void ResetToDefault()
    {
        Value = Enum.GetName(Setting.Type, Setting.DefaultValue)!;
    }

    private async Task SelectItemAsync()
    {
        var result = await ShowSelectionDialogAsync(Items);
        if (result.IsOk)
        {
            Value = result.SelectedItem;
        }
    }
}
