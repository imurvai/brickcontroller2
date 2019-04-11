namespace BrickController2.PlatformServices.Preferences
{
    public interface IPreferences
    {
        bool ContainsKey(string key, string section = null);
        T Get<T>(string key, string section = null);
        T Get<T>(string key, T defaultValue, string section = null);
        void Set<T>(string key, T value, string section = null);
    }
}
