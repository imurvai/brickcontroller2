using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BrickController2.Helpers
{
    public static class ObservableExtensions
    {
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> source)
        {
            return new ObservableCollection<T>(source);
        }
    }
}
