namespace BrickController2.UI.Services.Dialog
{
    public class ProgressDialogResult
    {
        public ProgressDialogResult(bool isCancelled)
        {
            IsCancelled = isCancelled;
        }

        public bool IsCancelled { get; }
    }
}
