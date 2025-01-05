using System;

namespace BrickController2.Settings;

public static class NamedSettingExtensions
{
    public static TValue GetValue<TValue>(this NamedSetting? setting, TValue defaultValue)
    {
        if (setting == null)
            return defaultValue;

        // special handling of enums
        if (typeof(TValue).IsEnum)
        {
            var safeValue = Convert.ChangeType(setting.Value, Enum.GetUnderlyingType(typeof(TValue)));
            if (Enum.IsDefined(typeof(TValue), safeValue))
            {
                return (TValue)Enum.ToObject(typeof(TValue), safeValue);
            }
            return defaultValue;
        }

        if (setting.Value is TValue typedValue)
            return typedValue;

        return (TValue)Convert.ChangeType(setting.Value, typeof(TValue));
    }
}
