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
    class RenderList
    {
        private List<RenderObject2D> _List = new List<RenderObject2D>();
        private int Min
        {
            get
            {
                if (Count > 0) return _List.Min(RO => RO.Z_index);
                else return int.MaxValue;
            }
        }
        private int Max
        {
            get
            {
                if (Count > 0) return _List.Max(RO => RO.Z_index);
                else return int.MinValue;
            }
        }
        public int Count => _List.Count;

        public RenderObject2D this[int index]
        {
            get => _List[index];
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public RenderList() { }
        public void Add(string Key, RenderObject2D item)
        {
            item.Set_Z_Index += Update_Index;
            if (Min >= item.Z_index) // if greater than biggest
            {
                _List.Insert(0, item); // add to start
            }
            else if (Max <= item.Z_index) // if less than smallest
            {
                _List.Add(item); // add to end
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
                    if (item.Z_index > _List[Mid].Z_index)
                    {
                        tail = Mid;
                    }
                    else
                    {
                        head = Mid;
                    }
                }
                _List.Insert(++tail, item);
            }
        }

        /// <summary>
        /// When Z index is updated, one object will be out of place so this can loop through and reposition it.
        /// </summary>
        private void Update_Index(int value) => _List = _List.OrderBy(RO => RO.Z_index).ToList();
        public IEnumerator<RenderObject2D> GetEnumerator() => _List.GetEnumerator();
    }
}