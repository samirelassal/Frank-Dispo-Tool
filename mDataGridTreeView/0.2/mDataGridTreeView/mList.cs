using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace WpfApplication1
{
    public delegate void mListChangedEventHandler(object sender, object item);

    public class mList<T> : List<T>
    {
        public event mListChangedEventHandler OnAdd;
        public event mListChangedEventHandler OnRemove;
        public event mListChangedEventHandler OnMove;


        public new void Add(T item) 
        {
            base.Add(item);
            if (OnAdd != null)
                OnAdd(this, item);
        }

        public new void Remove(T item) 
        {
            base.Remove(item);
            if (OnRemove != null) 
            {
                OnRemove(this, item);
            }
        }

        /// <summary>
        /// moves item to new index
        /// </summary>
        /// <param name="item"></param>
        /// <param name="newIndex"></param>
        public void Move(T item, int newIndex) 
        {
            List<T> range = base.GetRange(newIndex, Count + 1 - newIndex);
            base.RemoveRange(newIndex, Count + 1 - newIndex);
            base.Add(item);
            base.AddRange(range);
            if (OnMove != null)
                OnMove(this, item);
        }
    }
}
