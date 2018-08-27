using System;
using System.Threading;
using System.Threading.Tasks;
using BrickController2.UI.Services;

namespace BrickController2.iOS.UI.Services
{
    public class DialogService : IDialogService
    {
        public Task<IInputDialogResult> ShowInputDialogAsync(string message, string initialValue, string positiveButtonText, string negativeButtonText)
        {
            throw new NotImplementedException();
        }

        public Task<IDisposable> ShowProgressDialogAsync(string message, string cancelButtonText, CancellationTokenSource tokenSource)
        {
            throw new NotImplementedException();
        }

        public Task<IDeterministicProgressDialog> ShowProgressDialogAsync(string message, string cancelButtonText, CancellationTokenSource tokenSource, int minValue, int maxValue)
        {
            throw new NotImplementedException();
        }

        public Task<IGameControllerEventDialogResult> ShowGameControllerEventDialogAsync(string message, string cancelButtonText)
        {
            throw new NotImplementedException();
        }
    }
}