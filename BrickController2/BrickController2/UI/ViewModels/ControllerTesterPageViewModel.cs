using BrickController2.HardwareServices;
using BrickController2.UI.Navigation;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace BrickController2.UI.ViewModels
{
    public class ControllerTesterPageViewModel : PageViewModelBase
    {
        private readonly IGameControllerService _gameControllerService;

        public ControllerTesterPageViewModel(
            INavigationService navigationService,
            IGameControllerService gameControllerService)
            : base(navigationService)
        {
            _gameControllerService = gameControllerService;
        }

        public override void OnAppearing()
        {
            _gameControllerService.GameControllerEvent += GameControllerEventHandler;
            base.OnAppearing();
        }

        public override void OnDisappearing()
        {
            _gameControllerService.GameControllerEvent -= GameControllerEventHandler;
            base.OnDisappearing();
        }

        public ObservableCollection<ControllerEventViewModel> ControllerEventList { get; } = new ObservableCollection<ControllerEventViewModel>(); 

        private void GameControllerEventHandler(object sender, GameControllerEventArgs args)
        {
            foreach (var controllerEvent in args.ControllerEvents)
            {
                var controllerEventViewModel = ControllerEventList.FirstOrDefault(ce => ce.EventType == controllerEvent.Key.EventType && ce.EventCode == controllerEvent.Key.EventCode);
                if (0.1F < Math.Abs(controllerEvent.Value))
                {
                    if (controllerEventViewModel != null)
                    {
                        controllerEventViewModel.Value = controllerEvent.Value;
                    }
                    else
                    {
                        ControllerEventList.Add(new ControllerEventViewModel(controllerEvent.Key.EventType, controllerEvent.Key.EventCode, controllerEvent.Value));
                    }
                }
                else
                {
                    if (controllerEventViewModel != null)
                    {
                        ControllerEventList.Remove(controllerEventViewModel);
                    }
                }
            }
        }
    }
}
