using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Text;
using Graphics;
using System.Collections;

namespace Graphics
{
    //sorts render objects by z index
    class RenderObjectList<Vertex> : ICollection<RenderObject2D<Vertex>> where Vertex : unmanaged
    {
        private readonly List<RenderObject2D<Vertex>> _List = new List<RenderObject2D<Vertex>>();

        public int Count => _List.Count;
        public bool IsReadOnly => false;

        public RenderObject2D<Vertex> this[int index] 
        {
            get => _List[index];
            set => _List[index] = value; 
        }

        public void Add(RenderObject2D<Vertex> item)
        {
            item.Set_Z_Index += Update_Index;

            // linear search for Z index
            // continues displacing all 1s after to the next space
            // the adds last to the end
            for (int i = 0; i < Count; i++)
            {
                if (item.Z_index < _List[i].Z_index) // smallest numbers first
                {
                    RenderObject2D<Vertex> temp = _List[i];
                    _List[i] = item;
                    item = temp;
                }
            }
            _List.Add(item);
        }
        private void Update_Index(int Z)
        {

            for (int i = 1; i < Count; i++)
            {
                if (_List[i].Z_index < _List[i - 1].Z_index)
                {
                    RenderObject2D<Vertex> temp = _List[i - 1];
                    _List[i - 1] = _List[i];
                    _List[i] = temp;
                }
            }
        }
        public void Clear() => _List.Clear();
        public bool Contains(RenderObject2D<Vertex> item) => _List.Contains(item);
        public void CopyTo(RenderObject2D<Vertex>[] array, int arrayIndex) => _List.CopyTo(array, arrayIndex);
        public bool Remove(RenderObject2D<Vertex> item) => _List.Remove(item);
        public void RemoveAt(int index) => _List.RemoveAt(index);
        public IEnumerator<RenderObject2D<Vertex>> GetEnumerator() => _List.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_List).GetEnumerator();

        

    }
}
