using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWE3.Demo
{
    /// <summary>This class implements a lazy loading wrapper for framework object lists.</summary>
    /// <typeparam name="T">Type.</typeparam>
    public class LazyList<T>: IList<T>
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // protected members                                                                                                //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Primary keys.</summary>
        protected object[] _Pks;


        /// <summary>List values.</summary>
        protected List<T> _InternalItems = null;


        /// <summary>Table implementing a many to many relationship.</summary>
        protected string _Table = null;



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // protected properties                                                                                             //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        /// <summary>Gets the list values.</summary>
        protected List<T> _Items
        {
            get
            {
                if(_InternalItems == null)
                {
                    // TODO: implement!
                }
                
                return _InternalItems;
            }
        }



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // [interface] IList<T>                                                                                             //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Gets or sets an item by its index.</summary>
        /// <param name="index">Index.</param>
        /// <returns>Item.</returns>
        public T this[int index]
        {
            get { return _Items[index]; }
            set { _Items[index] = value; }
        }


        /// <summary>Gets the number of items in this list.</summary>
        public int Count
        {
            get { return _Items.Count; }
        }


        /// <summary>Gets if the list is read-only.</summary>
        bool ICollection<T>.IsReadOnly
        {
            get { return ((IList<T>) _Items).IsReadOnly; }
        }


        /// <summary>Adds an item to the list.</summary>
        /// <param name="item">Item.</param>
        public void Add(T item)
        {
            _Items.Add(item);
        }


        /// <summary>Clears the list.</summary>
        public void Clear()
        {
            _Items.Clear();
        }


        /// <summary>Returns if the list contains an item.</summary>
        /// <param name="item">Item.</param>
        /// <returns>Returns TRUE if the list contains the item, otherwise returns FALSE.</returns>
        public bool Contains(T item)
        {
            return _Items.Contains(item);
        }


        /// <summary>Copies the list to an array.</summary>
        /// <param name="array">Array.</param>
        /// <param name="arrayIndex">Starting index.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            _Items.CopyTo(array, arrayIndex);
        }


        /// <summary>Returns an enumerator for this list.</summary>
        /// <returns>Enumerator.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return _Items.GetEnumerator();
        }


        /// <summary>Returns the index of an item.</summary>
        /// <param name="item">Item.</param>
        /// <returns>Index.</returns>
        public int IndexOf(T item)
        {
            return _Items.IndexOf(item);
        }


        /// <summary>Inserts an item into the list.</summary>
        /// <param name="index">Index.</param>
        /// <param name="item">Item.</param>
        public void Insert(int index, T item)
        {
            _Items.Insert(index, item);
        }


        /// <summary>Removes an item from the list.</summary>
        /// <param name="item">Item.</param>
        /// <returns>Returns TRUE if successful, otherwise returns FALSE.</returns>
        public bool Remove(T item)
        {
            return _Items.Remove(item);
        }


        /// <summary>Removes an item with a specific index from the list.</summary>
        /// <param name="index">Index.</param>
        public void RemoveAt(int index)
        {
            _Items.RemoveAt(index);
        }


        /// <summary>Returns an enumerator for this list.</summary>
        /// <returns>Enumerator.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _Items.GetEnumerator();
        }
    }
}
