using BrickController2.UI.Services;
using UIKit;

namespace BrickController2.iOS.UI.Services
{
    public class ProgressImpl : IProgress
    {
        private readonly UIProgressView _progressView;
        private readonly int _maxValue;
        private UIAlertController _alertController;

        public ProgressImpl(UIAlertController alertController, UIProgressView progressView, int maxValue)
        {
            _alertController = alertController;
            _progressView = progressView;
            _maxValue = maxValue;
        }

        public string Title
        {
            set => _alertController.Title = value;
        }

        public string Message
        {
            set => _alertController.Message = value;
        }

        public int Progress
        {
            get => (int)(_progressView.Progress * _maxValue);
            set
            {
                if (_maxValue <= 0)
                {
                    return;
                }

                if (_maxValue <= value)
                {
                    _progressView.Progress = 1;
                }

                _progressView.Progress = (float)value / _maxValue;
            }
        }

        public async void Dispose()
        {
            if (_alertController != null)
            {
                await _alertController.DismissViewControllerAsync(true);
                _alertController = null;
            }
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