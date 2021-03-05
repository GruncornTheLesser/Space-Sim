using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace Graphics
{
    /* THING TO DO:
     * I have been a fool.
     */

    /// <summary>
    /// the list of objects that get rendered. The order of render is determined by the objects z index.
    /// </summary>
    class RenderList : IEnumerable<RenderObject2D>
    {
        private List<RenderObject2D> ObjectPool = new List<RenderObject2D>();
        public int Count => ObjectPool.Count;

        // adds object to render list.
        public void Add(RenderObject2D item)
        {
            ObjectPool.Add(item);
            item.Set_Z_Index += QuickSort;
            QuickSort(item.Z_index);

        }
        // removes object from render list.
        public void Remove(RenderObject2D item)
        {
            item.Set_Z_Index -= QuickSort;
            ObjectPool.Remove(item);
        }

        #region QuickSort
        /// <summary>
        /// used for Z index changed calls
        /// </summary>
        /// <param name="NewZ"></param>
        private void QuickSort(int NewZ) => QuickSort(0, Count - 1);

        /// <summary>
        /// quick sort with last index's value as pivot.
        /// </summary>
        /// <param name="Left">The left bound index(inclusive)</param>
        /// <param name="Right">The right bound (exclusive)</param>
        private void QuickSort(int Left, int Right)
        {
            if (Left >= Right) return;
            else 
            {
                // if Left < Right
                int P = QuickSortPartition(Left, Right); // finds partitions
                QuickSort(Left, P - 1); // sorts each partition
                QuickSort(P + 1, Right);

            }
        }
        /// <summary>
        /// moves all the values less than the pivot before and vice versa. 
        /// </summary>
        /// <param name="Left">The left bound index(inclusive).</param>
        /// <param name="Right">The right bound (exclusive).</param>
        /// <returns></returns>
        private int QuickSortPartition(int Left, int Right)
        {
            int pivot = ObjectPool[Right].Z_index; // take pivot point

            int i = Left - 1; // iterator

            //2. Reorder the collection.
            for (int j = Left; j < Right; j++)
            {
                if (ObjectPool[j].Z_index <= pivot)
                {
                    i++;
                    Swap(i, j);
                }
            }

            RenderObject2D temp1 = ObjectPool[i + 1];
            ObjectPool[i + 1] = ObjectPool[Right];
            ObjectPool[Right] = temp1;

            return i + 1;
        }
        /// <summary>
        /// Swaps value at index a with value at index b
        /// </summary>
        /// <param name="a">1st index to swap.</param>
        /// <param name="b">2nd index to swap.</param>
        private void Swap(int a, int b)
        {
            RenderObject2D tempb = ObjectPool[a];
            ObjectPool[a] = ObjectPool[b];
            ObjectPool[b] = tempb;
        }
        #endregion

        #region Enumerators
        public IEnumerator<RenderObject2D> GetEnumerator()
        {
            // 'yield return' returns the data in packets whereas 'return' returns it as one packet
            foreach(RenderObject2D Object in ObjectPool) yield return Object;
            // IEnumerator requires an instance of the object so must do 'RO in new RenderList()'
        }

        IEnumerator IEnumerable.GetEnumerator() => ObjectPool.GetEnumerator();
        #endregion
    }
}