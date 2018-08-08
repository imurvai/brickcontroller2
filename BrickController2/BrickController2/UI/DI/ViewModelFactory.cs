using BrickController2.UI.Navigation;
using BrickController2.UI.ViewModels;
using System;

namespace BrickController2.UI.DI
{
    public delegate ViewModelBase ViewModelFactory(Type type, NavigationParameters parameters);
}
