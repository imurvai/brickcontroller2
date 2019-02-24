using BrickController2.PlatformServices.Preferences;
using Foundation;
using System;

namespace BrickController2.iOS.PlatformServices.Preferences
{
    public class Preferences : IPreferences
    {
        private readonly object _lock = new object();

        public bool ContainsKey(string key, string section = null)
        {
            lock (_lock)
            {
                var defaults = GetDefaults(section);
                return defaults[key] != null; 
            }
        }

        public T Get<T>(string key, string section = null)
        {
            lock (_lock)
            {
                var defaults = GetDefaults(section);

                if (defaults[key] == null)
                {
                    throw new ArgumentException(nameof(key));
                }

                return Get<T>(key);
            }
        }

        public T Get<T>(string key, T defaultValue, string section = null)
        {
            lock (_lock)
            {
                var defaults = GetDefaults(section);

                if (defaults[key] == null)
                {
                    return defaultValue;
                }

                return Get<T>(key);
            }
        }

        public void Set<T>(string key, T value, string section = null)
        {
            lock (_lock)
            {
                var defaults = GetDefaults(section);

                switch (value)
                {
                    case bool b:
                        defaults.SetBool(b, key);
                        break;

                    case int i:
                        defaults.SetInt(i, key);
                        break;

                    case float f:
                        defaults.SetFloat(f, key);
                        break;

                    case string s:
                        defaults.SetString(s, key);
                        break;

                    default:
                        throw new NotSupportedException($"{typeof(T)} is not supported.");
                }
            }
        }

        private NSUserDefaults GetDefaults(string section)
        {
            if (!string.IsNullOrEmpty(section))
            {
                return new NSUserDefaults(section);
            }
            else
            {
                return NSUserDefaults.StandardUserDefaults;
            }
        }

        private T Get<T>(string key, NSUserDefaults defaults)
        {
            object result = null;

            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    result = (bool)defaults.BoolForKey(key);
                    break;

                case TypeCode.Int32:
                    result = (int)defaults.IntForKey(key);
                    break;

                case TypeCode.Single:
                    result = (float)defaults.FloatForKey(key);
                    break;

                case TypeCode.String:
                    result = (string)defaults.StringForKey(key);
                    break;

                default:
                    throw new NotSupportedException($"{typeof(T)} is not supported.");
            }

            return (T)result;
        }
    }
}