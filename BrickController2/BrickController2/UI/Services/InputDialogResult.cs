namespace BrickController2.UI.Services
{
    public class InputDialogResult
    {
        public InputDialogResult(bool isOk, string result)
        {
            IsOk = IsOk;
            Result = result;
        }

        public bool IsOk { get; }
        public string Result { get; }
    }
}
