using System.Collections;
using System.Collections.Generic;

namespace Nat.Web.Controls
{
    public class ListTableItems<T> : List<T>, IList where T : TableItem
    {
        private readonly ISessionWorkerContainer sessionWorkerContainer;

        public ListTableItems(IEnumerable<T> collection, ISessionWorkerContainer sessionWorkerContainer) : base(collection)
        {
            this.sessionWorkerContainer = sessionWorkerContainer;
        }

        public ListTableItems(ISessionWorkerContainer sessionWorkerContainer)
        {
            this.sessionWorkerContainer = sessionWorkerContainer;
        }

        #region IList Members

        int IList.Add(object item)
        {
            Add(item as T);
            return IndexOf(item as T);
        }

        #endregion

        public new void Add(T item)
        {
            if (item != null) item.SessionWorkerContainer = sessionWorkerContainer;
            base.Add(item);
        }

        public new void Insert(int index, T item)
        {
            if (item != null) item.SessionWorkerContainer = sessionWorkerContainer;
            base.Insert(index, item);
        }
    }
}