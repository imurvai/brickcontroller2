using System;
using System.Collections.Generic;

namespace BrickController2.UI.ViewModels
{
    public delegate ViewModelBase ViewModelFactory(Type viewModelType, IDictionary<string, object> parameters);
}
