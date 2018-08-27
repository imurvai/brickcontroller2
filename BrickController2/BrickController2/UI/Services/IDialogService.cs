using System;
using System.Threading;
using System.Threading.Tasks;

namespace BrickController2.UI.Services
{
    public interface IDialogService
    {
        Task<IInputDialogResult> ShowInputDialogAsync(string message, string initialValue, string positiveButtonText, string negativeButtonText);

        Task<IDisposable> ShowProgressDialogAsync(string message, string cancelButtonText, CancellationTokenSource tokenSource);

        Task<IDeterministicProgressDialog> ShowProgressDialogAsync(string message, string cancelButtonText, CancellationTokenSource tokenSource, int minValue, int maxValue);
    }
}
