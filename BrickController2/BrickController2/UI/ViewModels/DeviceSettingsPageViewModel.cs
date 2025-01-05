using BrickController2.DeviceManagement;
using BrickController2.UI.Commands;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Translation;
using BrickController2.UI.ViewModels.Settings;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BrickController2.UI.ViewModels;

public class DeviceSettingsPageViewModel : SettingsPageViewModelBase
{
    public DeviceSettingsPageViewModel(
        INavigationService navigationService,
        ITranslationService translationService,
        IDialogService dialogService,
        NavigationParameters parameters)
        : this(navigationService, translationService, dialogService, parameters.Get<Device>("device"))
    {
    }

    private DeviceSettingsPageViewModel(
        INavigationService navigationService,
        ITranslationService translationService,
        IDialogService dialogService,
        Device device) : base(navigationService, translationService, dialogService, device.CurrentSettings)
    {
        Device = device;
        SaveSettingsCommand = new SafeCommand(ApplyChanges, () => AllSettings.Any(x => x.HasChanged));
    }

    public ICommand SaveSettingsCommand { get; }

    public Device Device { get; }

    protected override void OnSettingChanged()
    {
        base.OnSettingChanged();
        SaveSettingsCommand.RaiseCanExecuteChanged();
    }

    private async Task ApplyChanges()
    {
        try
        {
            await DialogService.ShowProgressDialogAsync(
                false,
                async (progressDialog, token) =>
                {
                    var changedSettings = AllSettings
                        .Where(s => s.HasChanged)
                        .Select(s => s.Setting)
                        .ToArray();

                    await Device.UpdateDeviceSettingsAsync(changedSettings);
                },
                Translate("Saving"),
                token: DisappearingToken);

            await NavigationService.NavigateBackAsync();
        }
        catch (OperationCanceledException)
        {
        }
    }
}
