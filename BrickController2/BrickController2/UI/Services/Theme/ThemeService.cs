using BrickController2.UI.Themes;
using System;
using Xamarin.Forms;

namespace BrickController2.UI.Services.Theme
{
    public class ThemeService : IThemeService
    {
        public ThemeType CurrentTheme
        {
            get
            {
                return Application.Current.UserAppTheme switch
                {
                    OSAppTheme.Dark => ThemeType.Dark,
                    OSAppTheme.Light => ThemeType.Light,
                    _ => ThemeType.System
                };
            }

            set
            {
                var osTheme = value switch
                {
                    ThemeType.Light => OSAppTheme.Light,
                    ThemeType.Dark => OSAppTheme.Dark,
                    _ => OSAppTheme.Unspecified
                };

                if (osTheme != Application.Current.UserAppTheme)
                {
                    Application.Current.UserAppTheme = osTheme;
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

                ResourceDictionary selectedTheme = Application.Current.UserAppTheme switch
                {
                    OSAppTheme.Dark => new DarkTheme(),
                    OSAppTheme.Light => new LightTheme(),
                    _ => Application.Current.RequestedTheme switch
                    {
                        OSAppTheme.Dark => new DarkTheme(),
                        OSAppTheme.Light => new LightTheme(),
                        _ => new LightTheme()
                    }
                };

                mergedDictionaries.Add(selectedTheme);
            }
        }
    }
}
