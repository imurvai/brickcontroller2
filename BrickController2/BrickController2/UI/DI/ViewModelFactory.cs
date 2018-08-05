using System;
using System.Collections.Generic;

namespace BrickController2.UI.DI
{
    public delegate ViewModelBase ViewModelFactory(Type viewModelType, IDictionary<string, object> parameters);
}
