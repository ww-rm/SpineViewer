using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SpineViewer.Extensions
{
    public class ObservableCollectionWithLock<T> : ObservableCollection<T>
    {
        private readonly object _lock = new();

        public ObservableCollectionWithLock()
        {
            BindingOperations.EnableCollectionSynchronization(this, _lock);
        }

        /// <summary>
        /// 锁对象, 任何对集合的操作需要锁住该对象
        /// </summary>
        public object Lock => _lock;
    }
}
