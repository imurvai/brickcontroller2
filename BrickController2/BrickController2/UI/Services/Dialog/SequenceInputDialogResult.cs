namespace BrickController2.UI.Services.Dialog
{
    public class SequenceInputDialogResult
    {
        public SequenceInputDialogResult(bool isOk, float value, int durationMs)
        {
            IsOk = isOk;
            Value = value;
            DurationMs = durationMs;
        }

        public bool IsOk { get; }
        public float Value { get; }
        public int DurationMs { get; }
    }
}
