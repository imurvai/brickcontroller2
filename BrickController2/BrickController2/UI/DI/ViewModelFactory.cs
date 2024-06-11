using BrickController2.UI.Services.Navigation;
using BrickController2.UI.ViewModels;
using System;

namespace BrickController2.UI.DI
{
    public delegate PageViewModelBase ViewModelFactory(Type type, NavigationParameters? parameters);
}
