using BrickController2.UI.Commands;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Services.Navigation;
using BrickController2.UI.Services.Preferences;
using BrickController2.UI.Services.Theme;
using BrickController2.UI.Services.Translation;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BrickController2.UI.ViewModels
{
    public class SettingsPageViewModel : PageViewModelBase
    {
        private readonly IPreferencesService _preferencesService;
        private readonly IThemeService _themeService;
        private readonly IDialogService _dialogService;

        private CancellationTokenSource _disappearingTokenSource;

        public SettingsPageViewModel(
            INavigationService navigationService,
            ITranslationService translationService,
            IPreferencesService preferencesService,
            IDialogService dialogService,
            IThemeService themeService) : 
            base(navigationService, translationService)
        {
            _preferencesService = preferencesService;
            _themeService = themeService;
            _dialogService = dialogService;

            SelectThemeCommand = new SafeCommand(async () => await SelectThemeAsync());
        }

        public ThemeType CurrentTheme
        {
            get => _preferencesService.Get("Theme", ThemeType.System);
            set
            {
                if (CurrentTheme != value)
                {
                    _preferencesService.Set("Theme", value);
                    _themeService.ApplyTheme(value);
                    RaisePropertyChanged();
                }
            }
        }

        public ICommand SelectThemeCommand { get; }

        public override void OnAppearing()
        {
            _disappearingTokenSource?.Cancel();
            _disappearingTokenSource = new CancellationTokenSource();
        }

        public override void OnDisappearing()
        {
            _disappearingTokenSource.Cancel();
        }

        private async Task SelectThemeAsync()
        {
            var result = await _dialogService.ShowSelectionDialogAsync(
                Enum.GetNames(typeof(ThemeType)),
                Translate("Theme"),
                Translate("Cancel"),
                _disappearingTokenSource.Token);

            if (result.IsOk)
            {
                CurrentTheme = (ThemeType)Enum.Parse(typeof(ThemeType), result.SelectedItem);
            }
        }
    }
}
