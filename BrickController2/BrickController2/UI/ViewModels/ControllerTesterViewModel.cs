using BrickController2.HardwareServices;
using BrickController2.UI.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BrickController2.UI.ViewModels
{
    public class ControllerTesterViewModel : ViewModelBase
    {
        private readonly IGameControllerService _gameControllerService;
        private readonly IDictionary<(GameControllerEventType EventType, string EventCode), float> _controllerEvents = new Dictionary<(GameControllerEventType, string), float>();

        public ControllerTesterViewModel(
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

        public ObservableCollection<string> ControllerEventList { get; } = new ObservableCollection<string>(); 

        private void GameControllerEventHandler(object sender, GameControllerEventArgs args)
        {
            foreach (var controllerEvent in args.ControllerEvents)
            {
                if (Math.Abs(controllerEvent.Value) < 0.1F)
                {
                    if (_controllerEvents.ContainsKey(controllerEvent.Key))
                    {
                        _controllerEvents.Remove(controllerEvent.Key);
                    }
                }
                else
                {
                    _controllerEvents[controllerEvent.Key] = controllerEvent.Value;
                }
            }

            ControllerEventList.Clear();
            foreach (var entry in _controllerEvents)
            {
                var text = entry.Key.EventType == GameControllerEventType.Button ? 
                    $"Button - {entry.Key.EventCode}" : 
                    $"Axis - {entry.Key.EventCode} : ( {entry.Value.ToString("0.00")} )";
                ControllerEventList.Add(text);
            }
        }
    }
}
