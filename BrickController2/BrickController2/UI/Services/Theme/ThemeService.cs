using BrickController2.UI.Services.Preferences;
using BrickController2.UI.Themes;

namespace BrickController2.UI.Services.Theme
{
    public class ThemeService : IThemeService
    {
        private readonly IPreferencesService _preferencesService;

        public ThemeService(IPreferencesService preferencesService)
        {
            _preferencesService = preferencesService;
        }

        public ThemeType CurrentTheme
        {
            get => _preferencesService.Get("Theme", ThemeType.System);

            set
            {
                if (CurrentTheme != value)
                {
                    _preferencesService.Set("Theme", value);
                    ApplyCurrentTheme();
                }
            }
        }

        public void ApplyCurrentTheme()
        {
            var mergedDictionaries = Application.Current.Resources.MergedDictionaries;
            if (mergedDictionaries != null)
            {
                mergedDictionaries.Clear();

                ResourceDictionary selectedTheme = CurrentTheme switch
                {
                    ThemeType.Dark => new DarkTheme(),
                    ThemeType.Light => new LightTheme(),
                    _ => Application.Current.RequestedTheme switch
                    {
                        AppTheme.Dark => new DarkTheme(),
                        AppTheme.Light => new LightTheme(),
                        _ => new LightTheme()
                    }
                };

                mergedDictionaries.Add(selectedTheme);
            }
        }
    }
}
