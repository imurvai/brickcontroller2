namespace BrickController2.UI.Services.Preferences
{
    public interface IPreferencesService
    {
        bool ContainsKey(string key, string? section = null);
        T Get<T>(string key, string? section = null) where T : notnull;
        T Get<T>(string key, T defaultValue, string? section = null) where T : notnull;
        void Set<T>(string key, T value, string? section = null) where T : notnull;
    }
}
