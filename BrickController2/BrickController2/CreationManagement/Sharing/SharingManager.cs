using Newtonsoft.Json;

namespace BrickController2.CreationManagement.Sharing;

public class SharingManager : ISharingManager
{
    /// <summary>
    /// Export the specified <paramref name="item"/> as JSON.
    /// </summary>
    /// <typeparam name="TItem">Type of sharable item</typeparam>
    /// <returns>JSON payload that represents the specified model</returns>
    public Task<string> ShareAsync<TModel>(TModel model) where TModel : class, IShareable
    {
        var json = JsonConvert.SerializeObject(model);
        return Task.FromResult(json);
    }

    public Task ShareToClipboardAsync<TModel>(TModel model) where TModel : class, IShareable
    {
        var json = GetJson(new TypedPayload<TModel> { PayloadType = "t", Payload = model });
        return MainThread.InvokeOnMainThreadAsync(() =>
        {
            Clipboard.Default.SetTextAsync(json);
        });
    }

    public async Task<TModel> ImportFromClipboardAsync<TModel>() where TModel : class, IShareable
    {
        var json = await MainThread.InvokeOnMainThreadAsync(Clipboard.Default.GetTextAsync);
        return FromJson<TModel>(json);
    }

    private static TModel FromJson<TModel>(string json) where TModel : class, IShareable
        => JsonConvert.DeserializeObject<TModel>(json);

    private static string GetJson(object model)
        => JsonConvert.SerializeObject(model);
}
