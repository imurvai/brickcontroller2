using BrickController2.UI.Services.Background;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.ViewModels;

namespace BrickController2.UI.Pages
{
    public abstract class PageBase : ContentPage
    {
        private readonly IBackgroundService _backgroundService;
        private readonly IDialogServerHost _dialogServerHost;

        private IDialogServer _dialogServer;
        private bool _appeared;

        public PageBase(IBackgroundService backgroundService, IDialogServerHost dialogServerHost)
        {
            _backgroundService = backgroundService;
            _dialogServerHost = dialogServerHost;

            // On iOS hide the back button title
            NavigationPage.SetBackButtonTitle(this, string.Empty);
        }

        protected void AfterInitialize(IPageViewModel vm)
        {
            BindingContext = vm;
            _dialogServer = FindDialogServer();
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            _dialogServerHost.RegisterDialogServer(_dialogServer);

            _backgroundService.ApplicationSleepEvent += OnApplicationSleep;
            _backgroundService.ApplicationResumeEvent += OnApplicationResume;

            OnAppearingInternal();

        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            OnDisappearingInternal();

            _dialogServerHost.UnregisterDialogServer(_dialogServer);

            _backgroundService.ApplicationSleepEvent -= OnApplicationSleep;
            _backgroundService.ApplicationResumeEvent -= OnApplicationResume;
        }

        protected override bool OnBackButtonPressed()
        {
            var result = ((BindingContext as IPageViewModel)?.OnBackButtonPressed()) ?? true;
            return result && base.OnBackButtonPressed();
        }

        private void OnApplicationSleep(object sender, EventArgs args)
        {
            OnDisappearingInternal();
        }

        private void OnApplicationResume(object sender, EventArgs args)
        {
            OnAppearingInternal();
        }

        private void OnAppearingInternal()
        {
            if (!_appeared)
            {
                (BindingContext as IPageViewModel)?.OnAppearing();
            }

            _appeared = true;
        }

        private void OnDisappearingInternal()
        {
            if (_appeared)
            {
                (BindingContext as IPageViewModel)?.OnDisappearing();
            }

            _appeared = false;
        }

        private IDialogServer FindDialogServer()
        {
            if (Content is Layout layout)
            {
                var dialogServer = layout.Children.FirstOrDefault(c => c is IDialogServer) as IDialogServer;
                if (dialogServer != null)
                {
                    return dialogServer;
                }
            }

            throw new NotImplementedException("No dialog server");
        }
    }
}
