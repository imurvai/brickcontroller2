using System;
using System.Collections.Generic;

namespace BrickController2.UI.Services.Navigation
{
    public class NavigationParameters
    {
        private readonly IDictionary<string, object> _parameters = new Dictionary<string, object>();

        public NavigationParameters(params (string Key, object Value)[] parameters)
        {
            foreach (var entry in parameters)
            {
                _parameters[entry.Key] = entry.Value;
            }
        }

        public bool Contains(string key)
        {
            return _parameters.ContainsKey(key);
        }

        public T Get<T>(string key)
        {
            if (!_parameters.ContainsKey(key))
            {
                throw new ArgumentException($"No parameter for key '{key}'.");
            }

            var value = _parameters[key];
            if (!(value is T))
            {
                throw new ArgumentException($"Parameter for '{key}' is not type of '{typeof(T).Name}'.");
            }

            return (T)value;
        }

        public T Get<T>(string key, T defaultValue)
        {
            if (!_parameters.ContainsKey(key))
            {
                return defaultValue;
            }

            var value = _parameters[key];
            if (!(value is T))
            {
                return defaultValue;
            }

            return (T)value;
        }
    }
}
