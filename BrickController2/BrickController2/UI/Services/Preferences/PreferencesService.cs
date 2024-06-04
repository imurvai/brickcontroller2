using System;

namespace BrickController2.UI.Services.Preferences
{
    public class PreferencesService : IPreferencesService
    {
        private readonly object _lock = new object();

        public bool ContainsKey(string key, string? section = null)
        {
            lock (_lock)
            {
                return Microsoft.Maui.Storage.Preferences.ContainsKey(key, section);
            }
        }

        public T Get<T>(string key, string? section = null) where T : notnull
        {
            lock (_lock)
            {
                object? result = null;

                if (typeof(T).IsEnum)
                {
                    var enumString = Microsoft.Maui.Storage.Preferences.Get(key, string.Empty, section);
                    result = Enum.Parse(typeof(T), enumString);
                }
                else
                {
                    result = (Type.GetTypeCode(typeof(T))) switch
                    {
                        TypeCode.Boolean => Microsoft.Maui.Storage.Preferences.Get(key, false, section),
                        TypeCode.Int32 => Microsoft.Maui.Storage.Preferences.Get(key, 0, section),
                        TypeCode.Single => Microsoft.Maui.Storage.Preferences.Get(key, 0F, section),
                        TypeCode.String => Microsoft.Maui.Storage.Preferences.Get(key, string.Empty, section),
                        _ => throw new NotSupportedException($"{typeof(T)} is not supported."),
                    };
                }

                return (T)result;
            }
        }

        public T Get<T>(string key, T defaultValue, string? section = null) where T : notnull
        {
            lock (_lock)
            {
                if (!ContainsKey(key, section))
                {
                    return defaultValue;
                }

                return Get<T>(key, section);
            }
        }

        public void Set<T>(string key, T value, string? section = null) where T : notnull
        {
            lock (_lock)
            {
                if (typeof(T).IsEnum)
                {
                    var stringValue = value.ToString();
                    Microsoft.Maui.Storage.Preferences.Set(key, stringValue, section);
                }
                else
                {
                    switch (value)
                    {
                        case bool b:
                            Microsoft.Maui.Storage.Preferences.Set(key, b, section);
                            break;

                        case int i:
                            Microsoft.Maui.Storage.Preferences.Set(key, i, section);
                            break;

                        case float f:
                            Microsoft.Maui.Storage.Preferences.Set(key, f, section);
                            break;

                        case string s:
                            Microsoft.Maui.Storage.Preferences.Set(key, s, section);
                            break;

                        default:
                            throw new NotSupportedException($"{typeof(T)} is not supported.");
                    }
                }
            }
        }
    }
}
