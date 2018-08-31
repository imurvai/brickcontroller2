using BrickController2.UI.Services.Dialog;
using UIKit;

namespace BrickController2.iOS.UI.Services
{
    public class ProgressDialog : IProgressDialog
    {
        private readonly UIProgressView _progressView;
        private readonly UIAlertController _alertController;

        public ProgressDialog(UIAlertController alertController, UIProgressView progressView)
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
            get => (int)(_progressView?.Progress * 100 ?? 0);
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
    }
}