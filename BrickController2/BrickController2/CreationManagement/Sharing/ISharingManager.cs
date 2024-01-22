namespace BrickController2.CreationManagement.Sharing;

public interface ISharingManager
{
    /// <summary>
    /// Export the specified <paramref name="model"/> as JSON.
    /// </summary>
    /// <typeparam name="TItem">Type of sharable item</typeparam>
    /// <returns>JSON payload that represents the specified model</returns>
    Task<string> ShareAsync<TModel>(TModel model) where TModel : class, IShareable;

    Task ShareToClipboardAsync<TModel>(TModel model) where TModel : class, IShareable;

    Task<TModel> ImportFromClipboardAsync<TModel>() where TModel : class, IShareable;
}
