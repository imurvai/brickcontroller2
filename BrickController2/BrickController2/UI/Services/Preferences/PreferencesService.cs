using System;

namespace BrickController2.UI.Services.Preferences
{
    public class PreferencesService : IPreferencesService
    {
        private readonly object _lock = new object();

        public bool ContainsKey(string key, string section = null)
        {
            lock (_lock)
            {
                return Xamarin.Essentials.Preferences.ContainsKey(key, section);
            }
        }

        public T Get<T>(string key, string section = null)
        {
            lock (_lock)
            {
                object result = null;

                switch (Type.GetTypeCode(typeof(T)))
                {
                    case TypeCode.Boolean:
                        result = Xamarin.Essentials.Preferences.Get(key, false, section);
                        break;

                    case TypeCode.Int32:
                        result = Xamarin.Essentials.Preferences.Get(key, 0, section);
                        break;

                    case TypeCode.Single:
                        result = Xamarin.Essentials.Preferences.Get(key, 0F, section);
                        break;

                    case TypeCode.String:
                        result = Xamarin.Essentials.Preferences.Get(key, string.Empty, section);
                        break;

                    default:
                        throw new NotSupportedException($"{typeof(T)} is not supported.");
                }

                return (T)result;
            }
        }

        public T Get<T>(string key, T defaultValue, string section = null)
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

        public void Set<T>(string key, T value, string section = null)
        {
            lock (_lock)
            {
                switch (value)
                {
                    case bool b:
                        Xamarin.Essentials.Preferences.Set(key, b, section);
                        break;

                    case int i:
                        Xamarin.Essentials.Preferences.Set(key, i, section);
                        break;

                    case float f:
                        Xamarin.Essentials.Preferences.Set(key, f, section);
                        break;

                    case string s:
                        Xamarin.Essentials.Preferences.Set(key, s, section);
                        break;

                    default:
                        throw new NotSupportedException($"{typeof(T)} is not supported.");
                }
            }
        }
    }
}
