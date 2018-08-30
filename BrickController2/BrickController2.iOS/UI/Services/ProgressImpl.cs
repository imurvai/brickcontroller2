using BrickController2.UI.Services;
using UIKit;

namespace BrickController2.iOS.UI.Services
{
    public class ProgressImpl : IProgress
    {
        private readonly UIProgressView _progressView;
        private UIAlertController _alertController;

        public ProgressImpl(UIAlertController alertController, UIProgressView progressView)
        {
            _alertController = alertController;
            _progressView = progressView;
        }

        public string Title
        {
            set => _alertController.Title = value;
        }

        public string Message
        {
            set => _alertController.Message = value;
        }

        public int Percent
        {
            get => (int)(_progressView != null ? _progressView.Progress * 100 : 0);
            set
            {
                if (_progressView == null)
                {
                    return;
                }

                if (value >= 100)
                {
                    _progressView.SetProgress(1F, true);
                }

                _progressView.SetProgress((float)value / 100, true);
            }
        }

        public void Dispose()
        {
            if (_alertController != null)
            {
                // TODO: somehow wait for the completion of the dismiss
                _alertController.DismissViewController(true, null);
                _alertController = null;
            }
        }
    }
}