using System;

namespace BrickController2.UI.Services.Theme
{
    public interface IThemeService
    {
        ThemeType CurrentTheme { get; set; }
        void ApplyCurrentTheme();
    }
}
