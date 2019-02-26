using System;
using Android.Content;
using Android.Preferences;
using BrickController2.PlatformServices.Preferences;

namespace BrickController2.Droid.PlatformServices.Preferences
{
    public class Preferences : IPreferences
    {
        private readonly object _lock = new object();
        private readonly Context _context;

        public Preferences(Context context)
        {
            _context = context;
        }

        public bool ContainsKey(string key, string section = null)
        {
            lock (_lock)
            {
                using (var prefs = GetSharedPreferences(section))
                {
                    return prefs.Contains(key);
                }
            }
        }

        public T Get<T>(string key, string section = null)
        {
            lock (_lock)
            {
                using (var prefs = GetSharedPreferences(section))
                {
                    if (!prefs.Contains(key))
                    {
                        throw new ArgumentException(nameof(key));
                    }

                    return Get<T>(key, prefs);
                }
            }
        }

        public T Get<T>(string key, T defaultValue, string section = null)
        {
            lock (_lock)
            {
                using (var prefs = GetSharedPreferences(section))
                {
                    if (!prefs.Contains(key))
                    {
                        return defaultValue;
                    }

                    return Get<T>(key, prefs);
                }
            }
        }

        public void Set<T>(string key, T value, string section = null)
        {
            lock (_lock)
            {
                using (var prefs = GetSharedPreferences(section))
                using (var editor = prefs.Edit())
                {
                    switch (value)
                    {
                        case bool b:
                            editor.PutBoolean(key, b);
                            break;

                        case int i:
                            editor.PutInt(key, i);
                            break;

                        case float f:
                            editor.PutFloat(key, f);
                            break;

                        case string s:
                            editor.PutString(key, s);
                            break;

                        default:
                            throw new NotSupportedException($"{typeof(T)} is not supported.");
                    }

                    editor.Commit();
                }
            }
        }

        private ISharedPreferences GetSharedPreferences(string section)
        {
            if (!string.IsNullOrEmpty(section))
            {
                return _context.GetSharedPreferences(section, FileCreationMode.Private);
            }
            else
            {
                return PreferenceManager.GetDefaultSharedPreferences(_context);
            }
        }

        private T Get<T>(string key, ISharedPreferences prefs)
        {
            object result = null;

            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    result = (bool)prefs.GetBoolean(key, false);
                    break;

                case TypeCode.Int32:
                    result = (int)prefs.GetInt(key, 0);
                    break;

                case TypeCode.Single:
                    result = (float)prefs.GetFloat(key, 0);
                    break;

                case TypeCode.String:
                    result = (string)prefs.GetString(key, string.Empty);
                    break;

                default:
                    throw new NotSupportedException($"{typeof(T)} is not supported.");
            }

            return (T)result;
        }
    }
}