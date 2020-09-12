using BrickController2.UI.Themes;
using System;
using Xamarin.Forms;

namespace BrickController2.UI.Services.Theme
{
    public class ThemeService : IThemeService
    {
        public void ApplyTheme(ThemeType theme)
        {
            var mergedDictionaries = Application.Current.Resources.MergedDictionaries;
            if (mergedDictionaries != null)
            {
                mergedDictionaries.Clear();

                ResourceDictionary selectedTheme = theme switch
                {
                    ThemeType.Dark => new DarkTheme(),
                    ThemeType.Light => new LightTheme(),
                    ThemeType.System => Application.Current.RequestedTheme switch
                    {
                        OSAppTheme.Dark => new DarkTheme(),
                        OSAppTheme.Light => new LightTheme(),
                        _ => new LightTheme()
                    },
                    _ => throw new ArgumentException(nameof(theme))
                };

                mergedDictionaries.Add(selectedTheme);
            }
        }
    }
}
