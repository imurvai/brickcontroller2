using BrickController2.UI.ViewModels;
using System;
using System.Collections.Generic;

namespace BrickController2.UI.DI
{
    public delegate ViewModelBase ViewModelFactory(Type type, IDictionary<string, object> parameters);
}
