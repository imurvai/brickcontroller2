using System.Threading.Tasks;
using BrickController2.CreationManagement;
using BrickController2.UI.Services.Navigation;
using System.Windows.Input;
using BrickController2.UI.Services.Dialog;
using BrickController2.UI.Commands;
using System.Threading;
using System;
using BrickController2.UI.Services.Translation;
using BrickController2.BusinessLogic;
using BrickController2.Helpers;
using System.IO;
using BrickController2.PlatformServices.SharedFileStorage;

namespace BrickController2.UI.ViewModels
{
    public class CreationPageViewModel : PageViewModelBase
    {
        private readonly ICreationManager _creationManager;
        private readonly IDialogService _dialogService;
        private readonly ISharedFileStorageService _sharedFileStorageService;
        private readonly IPlayLogic _playLogic;

        private CancellationTokenSource _disappearingTokenSource;

        public CreationPageViewModel(
            INavigationService navigationService,
            ITranslationService translationService,
            ICreationManager creationManager,
            IDialogService dialogService,
            ISharedFileStorageService sharedFileStorageService,
            IPlayLogic playLogic,
            NavigationParameters parameters)
            : base(navigationService, translationService)
        {
            _creationManager = creationManager;
            _dialogService = dialogService;
            _sharedFileStorageService = sharedFileStorageService;
            _playLogic = playLogic;

            Creation = parameters.Get<Creation>("creation");

            ImportControllerProfileCommand = new SafeCommand(async () => await ImportControllerProfileAsync(), () => _sharedFileStorageService.IsSharedStorageAvailable);
            ExportCreationCommand = new SafeCommand(async () => await ExportCreationAsync(), () => _sharedFileStorageService.IsSharedStorageAvailable);
            RenameCreationCommand = new SafeCommand(async () => await RenameCreationAsync());
            PlayCommand = new SafeCommand(async () => await PlayAsync());
            AddControllerProfileCommand = new SafeCommand(async () => await AddControllerProfileAsync());
            ControllerProfileTappedCommand = new SafeCommand<ControllerProfile>(async controllerProfile => await NavigationService.NavigateToAsync<ControllerProfilePageViewModel>(new NavigationParameters(("controllerprofile", controllerProfile))));
            DeleteControllerProfileCommand = new SafeCommand<ControllerProfile>(async controllerProfile => await DeleteControllerProfileAsync(controllerProfile));
        }

        public Creation Creation { get; }

        public ICommand ImportControllerProfileCommand { get; }
        public ICommand ExportCreationCommand { get; }
        public ICommand RenameCreationCommand { get; }
        public ICommand PlayCommand { get; }
        public ICommand AddControllerProfileCommand { get; }
        public ICommand ControllerProfileTappedCommand { get; }
        public ICommand DeleteControllerProfileCommand { get; }

        public override void OnAppearing()
        {
            _disappearingTokenSource?.Cancel();
            _disappearingTokenSource = new CancellationTokenSource();
        }

        public override void OnDisappearing()
        {
            _disappearingTokenSource.Cancel();
        }

        private async Task RenameCreationAsync()
        {
            try
            {
                var result = await _dialogService.ShowInputDialogAsync(
                    Creation.Name,
                    Translate("CreationName"),
                    Translate("Rename"),
                    Translate("Cancel"),
                    KeyboardType.Text,
                    (creationName) => !string.IsNullOrEmpty(creationName),
                    _disappearingTokenSource.Token);
                if (result.IsOk)
                {
                    if (string.IsNullOrWhiteSpace(result.Result))
                    {
                        await _dialogService.ShowMessageBoxAsync(
                            Translate("Warning"),
                            Translate("CreationNameCanNotBeEmpty"),
                            Translate("Ok"),
                            _disappearingTokenSource.Token);
                        return;
                    }

                    await _dialogService.ShowProgressDialogAsync(
                        false,
                        async (progressDialog, token) => await _creationManager.RenameCreationAsync(Creation, result.Result),
                        Translate("Renaming"),
                        token: _disappearingTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task PlayAsync()
        {
            try
            {
                var validationResult = _playLogic.ValidateCreation(Creation);

                string warning = null;
                switch (validationResult)
                {
                    case CreationValidationResult.MissingControllerAction:
                        warning = Translate("NoControllerActions");
                        break;

                    case CreationValidationResult.MissingDevice:
                        warning = Translate("MissingDevices");
                        break;

                    case CreationValidationResult.MissingSequence:
                        warning = Translate("MissingSequence");
                        break;
                }

                if (validationResult == CreationValidationResult.Ok)
                {
                    await NavigationService.NavigateToAsync<PlayerPageViewModel>(new NavigationParameters(("creation", Creation)));
                }
                else
                {
                    await _dialogService.ShowMessageBoxAsync(
                        Translate("Warning"),
                        warning,
                        Translate("Ok"),
                        _disappearingTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task AddControllerProfileAsync()
        {
            try
            {
                var result = await _dialogService.ShowInputDialogAsync(
                    null,
                    Translate("ProfileName"),
                    Translate("Create"),
                    Translate("Cancel"),
                    KeyboardType.Text,
                    (profileName) => !string.IsNullOrEmpty(profileName),
                    _disappearingTokenSource.Token);

                if (result.IsOk)
                {
                    if (string.IsNullOrWhiteSpace(result.Result))
                    {
                        await _dialogService.ShowMessageBoxAsync(
                            Translate("Warning"),
                            Translate("ProfileNameCanNotBeEmpty"),
                            Translate("Ok"),
                            _disappearingTokenSource.Token);

                        return;
                    }

                    ControllerProfile controllerProfile = null;
                    await _dialogService.ShowProgressDialogAsync(
                        false,
                        async (progressDialog, token) => controllerProfile = await _creationManager.AddControllerProfileAsync(Creation, result.Result),
                        Translate("Creating"),
                        token: _disappearingTokenSource.Token);

                    await NavigationService.NavigateToAsync<ControllerProfilePageViewModel>(new NavigationParameters(("controllerprofile", controllerProfile)));
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task DeleteControllerProfileAsync(ControllerProfile controllerProfile)
        {
            try
            {
                if (await _dialogService.ShowQuestionDialogAsync(
                    Translate("Confirm"),
                    $"{Translate("AreYouSureToDeleteProfile")} '{controllerProfile.Name}'?",
                    Translate("Yes"),
                    Translate("No"),
                    _disappearingTokenSource.Token))
                {
                    await _dialogService.ShowProgressDialogAsync(
                        false,
                        async (progressDialog, token) => await _creationManager.DeleteControllerProfileAsync(controllerProfile),
                        Translate("Deleting"),
                        token: _disappearingTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task ImportControllerProfileAsync()
        {
            await _dialogService.ShowMessageBoxAsync(
                "Pukk",
                "Majd",
                "Ok",
                _disappearingTokenSource.Token);
        }

        private async Task ExportCreationAsync()
        {
            try
            {
                var filename = Creation.Name;
                var done = false;

                do
                {
                    var result = await _dialogService.ShowInputDialogAsync(
                        filename,
                        Translate("CreationName"),
                        Translate("Ok"),
                        Translate("Cancel"),
                        KeyboardType.Text,
                        fn => FileHelper.FilenameValidator(fn),
                        _disappearingTokenSource.Token);

                    if (!result.IsOk)
                    {
                        return;
                    }

                    filename = result.Result;
                    var filePath = Path.Combine(_sharedFileStorageService.SharedStorageDirectory, $"{filename}.{FileHelper.CreationFileExtension}");

                    if (!File.Exists(filePath) || 
                        await _dialogService.ShowQuestionDialogAsync(
                            Translate("FileAlreadyExists"),
                            Translate("DoYouWantToOverWrite"),
                            Translate("Yes"),
                            Translate("No"),
                            _disappearingTokenSource.Token))
                    {
                        try
                        {
                            await _creationManager.ExportCreationAsync(Creation, filePath);
                            done = true;

                            await _dialogService.ShowMessageBoxAsync(
                                Translate("ExportSuccessful"),
                                filePath,
                                Translate("Ok"),
                                _disappearingTokenSource.Token);
                        }
                        catch (Exception)
                        {
                            await _dialogService.ShowMessageBoxAsync(
                                Translate("Error"),
                                Translate("FailedToExportCreation"),
                                Translate("Ok"),
                                _disappearingTokenSource.Token);
                            
                            return;
                        }
                    }
                }
                while (!done);
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}
