using System;
using System.Threading;
using Android.App;
using Android.Widget;
using BrickController2.UI.Services.Dialog;

namespace BrickController2.Droid.UI.Services
{
    public class ProgressDialog : IProgressDialog
    {
        private readonly ProgressBar _progressBar;
        private readonly AlertDialog _progressDialog;
        
        public ProgressDialog(AlertDialog progressDialog, ProgressBar progressBar)
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

        public int Percent
        {
            get => _progressBar.Progress;
            set => _progressBar.Progress = Math.Min(_progressBar.Max, Math.Max(0, value));
        }
    }
}