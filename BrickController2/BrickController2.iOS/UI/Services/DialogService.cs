using System;
using System.Threading;
using System.Threading.Tasks;
using BrickController2.HardwareServices;
using BrickController2.UI.Services;

namespace BrickController2.iOS.UI.Services
{
    public class DialogService : IDialogService
    {
        public Task<IInputDialogResult> ShowInputDialogAsync(string title, string message, string initialValue, string positiveButtonText, string negativeButtonText)
        {
            throw new NotImplementedException();
        }

        public IProgress ShowProgressDialogAsync(string title, string message, string cancelButtonText, CancellationTokenSource tokenSource, int minValue, int maxValue)
        {
            throw new NotImplementedException();
        }

        public Task<IGameControllerEventDialogResult> ShowGameControllerEventDialogAsync(string title, string message, string cancelButtonText)
        {
            throw new NotImplementedException();
        }


        private class InputDialogResult : IInputDialogResult
        {
            public bool IsPositive { get; set; }
            public string Result { get; set; }
        }

        private class ProgressImpl : IProgress
        {
            public string Title
            {
                get => throw new NotImplementedException();
                set => throw new NotImplementedException();
            }

            public string Message
            {
                get => throw new NotImplementedException();
                set => throw new NotImplementedException();
            }

            public int Progress
            {
                get => throw new NotImplementedException();
                set => throw new NotImplementedException();
            }

            public void Dispose()
            {
            }
        }

        private class GameControllerDialogResult : IGameControllerEventDialogResult
        {
            public bool IsOk { get; set; }
            public GameControllerEventType EventType { get; set; }
            public string EventCode { get; set; }
        }
    }
}