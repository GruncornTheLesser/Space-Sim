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
        private static List<RenderObject2D> ObjectPool = new List<RenderObject2D>();
        public static int Count => ObjectPool.Count;

        // adds object to render list.
        public static void Add(RenderObject2D item)
        {
            ObjectPool.Add(item);
            item.Set_Z_Index += QuickSort;
            QuickSort(item.Z_index);
            
            Display();

        }
        // removes object from render list.
        public static void Remove(RenderObject2D item)
        {
            item.Set_Z_Index -= QuickSort;
            ObjectPool.Remove(item);
            Display();
        }

        #region QuickSort
        /// <summary>
        /// used for Z index changed calls
        /// </summary>
        /// <param name="NewZ"></param>
        private static void QuickSort(int NewZ) => QuickSort(0, Count);

        /// <summary>
        /// quick sort with last index's value as pivot.
        /// </summary>
        /// <param name="Left">The left bound index(inclusive)</param>
        /// <param name="Right">The right bound (exclusive)</param>
        private static void QuickSort(int Left, int Right)
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
        /// <param name="Pivot">The value its being reorganised with.</param>
        /// <returns></returns>
        private static int QuickSortPartition(int Left, int Right)
        {
            int Pivot = ObjectPool[Left].Z_index;
            int i = Left + 1; // runs left to right
            int j = Right; // runs right to left
            while (true)
            {
                while (i < Right && ObjectPool[i++].Z_index < Pivot); // Adds to left pointer until value less than pivot
                while (ObjectPool[--j].Z_index > Pivot); // subtracts from right pointer until value less than pivot

                if (i >= j) break; // if pointers havent passed each other
                else swap(i++, j--); // swap the values that it got stuck on
            }
            swap(j, Left); // swap j index with Pivot index(Left)
            return j;
        }
        // Swaps value at index a with value at index b
        private static void swap(int a, int b)
        {
            RenderObject2D tempb = ObjectPool[a];
            ObjectPool[a] = ObjectPool[b];
            ObjectPool[b] = tempb;
        }
        /// <summary>
        /// Creates array for testing. 
        /// </summary>
        public static void Display()
        {
            int[] arr = new int[Count];
            for (int i = 0; i < Count; i++) arr[i] = ObjectPool[i].Z_index;
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