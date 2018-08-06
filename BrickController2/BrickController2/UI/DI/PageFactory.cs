using BrickController2.UI.Pages;
using BrickController2.UI.ViewModels;
using System;

namespace BrickController2.UI.DI
{
    public delegate PageBase PageFactory(Type type, ViewModelBase vm);
}
