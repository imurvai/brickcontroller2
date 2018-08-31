namespace BrickController2.UI.Services.Dialog
{
    public class InputDialogResult
    {
        public InputDialogResult(bool isOk, string result)
        {
            IsOk = isOk;
            Result = result;
        }

        public bool IsOk { get; }
        public string Result { get; }
    }
}
