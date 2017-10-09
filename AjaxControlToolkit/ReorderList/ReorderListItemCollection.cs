// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.


using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;

namespace AjaxControlToolkit
{
    /// <summary>
    /// Basica collection of ReorderListItems.  This just calls through to the parent's list.
    /// </summary>
    public class ReorderListItemCollection : IList<ReorderListItem>
    {
        private ReorderList _parent;

        public ReorderListItemCollection(ReorderList parent)
        {
            _parent = parent;
        }

        private ControlCollection ChildList
        {
            get
            {
                return _parent.ChildList.Controls;
            }
        }

        #region IList<ReorderListItem> Members

        public int IndexOf(ReorderListItem item)
        {
            return ChildList.IndexOf(item);
        }

        public void Insert(int index, ReorderListItem item)
        {
            throw new NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            ChildList.RemoveAt(index);
        }

        public ReorderListItem this[int index]
        {
            get
            {
                return (ReorderListItem)ChildList[index];
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        #endregion

        #region ICollection<ReorderListItem> Members

        public void Add(ReorderListItem item)
        {
            ChildList.Add(item);
        }

        public void Clear()
        {
            ChildList.Clear();
        }

        public bool Contains(ReorderListItem item)
        {
            return ChildList.Contains(item);
        }

        public void CopyTo(ReorderListItem[] array, int arrayIndex)
        {
            ChildList.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get
            {
                return ChildList.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return ChildList.IsReadOnly;
            }
        }

        public bool Remove(ReorderListItem item)
        {
            ChildList.Remove(item);
            return true;
        }

        #endregion

        #region IEnumerable<ReorderListItem> Members

        public IEnumerator<ReorderListItem> GetEnumerator()
        {
            return new ReorderListItemEnumerator(ChildList.GetEnumerator());
        }

        private class ReorderListItemEnumerator : IEnumerator<ReorderListItem>
        {
            private IEnumerator _controlEnum;

            public ReorderListItemEnumerator(IEnumerator baseEnum)
            {
                _controlEnum = baseEnum;
            }

            #region IEnumerator<ReorderListItem> Members

            public ReorderListItem Current
            {
                get
                {
                    return (ReorderListItem)_controlEnum.Current;
                }
            }

            #endregion

            #region IDisposable Members

            public void Dispose()
            {
                _controlEnum = null;
            }

            #endregion

            #region IEnumerator Members

            object IEnumerator.Current
            {
                get { return (ReorderListItem)_controlEnum.Current; }
            }

            public bool MoveNext()
            {
                return _controlEnum.MoveNext();
            }

            public void Reset()
            {
                _controlEnum.Reset();
            }

            #endregion
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ChildList.GetEnumerator();
        }

        #endregion
    }
}
