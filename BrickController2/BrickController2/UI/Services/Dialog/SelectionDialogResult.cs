namespace BrickController2.UI.Services.Dialog
{
    public class SelectionDialogResult<T> where T : notnull
    {
        public SelectionDialogResult(bool isOk, T selectedValue)
        {
            IsOk = isOk;
            SelectedItem = selectedValue;
        }

        public bool IsOk { get; }
        public T SelectedItem { get; }
    }
}
