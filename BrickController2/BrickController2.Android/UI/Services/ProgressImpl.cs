using System;
using Android.App;
using Android.Widget;
using BrickController2.UI.Services;

namespace BrickController2.Droid.UI.Services
{
    public class ProgressImpl : IProgress
    {
        private readonly ProgressBar _progressBar;
        private AlertDialog _progressDialog;

        public ProgressImpl(AlertDialog progressDialog, ProgressBar progressBar)
        {
            _progressDialog = progressDialog;
            _progressBar = progressBar;
        }

        public string Title
        {
            set => _progressDialog.SetTitle(value);
        }

        public string Message
        {
            set => _progressDialog.SetMessage(value);
        }

        public int Progress
        {
            get => _progressBar.Progress;
            set => _progressBar.Progress = Math.Min(_progressBar.Max, Math.Max(0, value));
        }

        public void Dispose()
        {
            if (_progressDialog != null)
            {
                _progressDialog.Dismiss();
                _progressDialog = null;
            }
        }
    }
}