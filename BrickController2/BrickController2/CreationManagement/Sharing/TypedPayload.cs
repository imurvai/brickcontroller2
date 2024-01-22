namespace BrickController2.CreationManagement.Sharing;

internal class TypedPayload<TModel> where TModel : class, IShareable
{
    public string PayloadType { get; init; }
    public TModel Payload { get; init; }
}
