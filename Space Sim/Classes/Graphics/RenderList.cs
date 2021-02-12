using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Text;
using Graphics;
using System.Collections;
using OpenTK.Mathematics;
using System.Linq;
using DeepCopy;
namespace Graphics
{
    /* THING TO DO:
     * Could Make it a deepcopy so it can be changed in the window class -> requires a constructor with no parameters for a default value
     */

    /// <summary>
    /// Sorts Render Objects by Z index
    /// </summary>
    /// <typeparam name="Vertex"></typeparam>
    class RenderList : IEnumerable<RenderObject2D>
    {
        private List<string> _List = new List<string>(); // list of keys
        private Dictionary<string, Func<RenderObject2D>> _Dict = new Dictionary<string, Func<RenderObject2D>>(); // hash table for key to the render object get function
        private int Min
        {
            get
            {
                if (Count > 0) return _List.Min(Key => _Dict[Key]().Z_index);
                else return int.MaxValue;
            }
        }
        private int Max
        {
            get
            {
                if (Count > 0) return _List.Max(Key => _Dict[Key]().Z_index);
                else return int.MinValue;
            }
        }
        public int Count => _List.Count;

        public RenderList() { }
        public void Add(string Key, Func<RenderObject2D> get_item)
        {
            get_item().Set_Z_Index += Update_Index;
            _Dict.Add(Key, get_item);

            if (Min >= get_item().Z_index) // if less than smallest
            {
                _List.Insert(0, Key); // add to start
            }
            else if (Max <= get_item().Z_index) // if greater than biggest
            {
                _List.Add(Key); // add to end
            }
            else
            {
                // binary search for where to add new object
                int tail = 0;
                int head = Count - 1;
                
                // while there are indexes between head and tail
                while (head - tail > 1)
                {
                    int Mid = (head + tail) / 2;
                    if (get_item().Z_index > _Dict[_List[Mid]]().Z_index)
                    {
                        tail = Mid;
                    }
                    else
                    {
                        head = Mid;
                    }
                }
                _List.Insert(++tail, Key);
            }
        }

        /// <summary>
        /// When Z index is updated, one object will be out of place so this can loop through and reposition it.
        /// </summary>
        private void Update_Index(int value) => _List = _List.OrderBy(Key => _Dict[Key]().Z_index).ToList(); // re-sort using Z index
        

        public IEnumerator<RenderObject2D> GetEnumerator()
        {
            // 'yield return' returns the data in packets whereas 'return' returns it as one packet
            foreach(string Key in _List) yield return _Dict[Key]();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}