using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace BrickController2.Helpers
{
    /// <summary>
    /// Observable generic collection allowing builk addition of items
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObservableCollectionExt<T> : ObservableCollection<T>
    {
        public ObservableCollectionExt() : base()
        {
        } 

        public ObservableCollectionExt(IEnumerable<T> collection) : base(collection)
        {
        }

        public ObservableCollectionExt(List<T> list) : base(list)
        {
        }

        /// <summary> 
        /// Adds the elements of the specified collection to the end of the ObservableCollection(Of T). 
        /// </summary> 
        /// <remarks>No check on item duplicity during addition</remarks>
        public void AddRange(IList<T> collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            foreach (var i in collection)
            {
                Items.Add(i);
            }

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, (System.Collections.IList)collection));
        }
    }
}