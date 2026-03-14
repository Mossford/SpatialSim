using System.Collections;

namespace SpatialSim.Engine.Core
{
    /// <summary>
    /// List with stable ids for deletion and addition
    ///
    /// For loops need to be done with ValueCount and Get to work properly as the index will not match the data or even crash
    /// </summary>
    public class StableList<T>
    {
        List<T> data;
        /// <summary>
        /// Stores the dataId value inline with the data list
        /// </summary>
        List<int> dataIdNonStable;
        List<int> dataIds;
        Queue<int> freeIds;

        /// <summary>
        /// Gives how much elements were added
        /// </summary>
        public int Count => dataIds.Count;
        /// <summary>
        /// Gives how much elements are currently stored
        /// </summary>
        public int ValueCount => data.Count;

        public StableList()
        {
            data = new List<T>();
            dataIdNonStable = new List<int>();
            dataIds = new List<int>();
            freeIds = new Queue<int>();
        }

        public int Add(T value)
        {
            int id;
            if (freeIds.Count != 0)
            {
                id = freeIds.Dequeue();
                dataIds[id] = data.Count;
            }
            else
            {
                id = dataIds.Count;
                dataIds.Add(data.Count);
            }
            
            data.Add(value);
            dataIdNonStable.Add(id);

            return id;
        }

        public int PeekNextId()
        {
            int id;
            if (freeIds.Count != 0)
            {
                id = freeIds.Peek();
            }
            else
            {
                id = dataIds.Count;
            }

            return id;
        }

        public void Clear()
        {
            data.Clear();
            dataIdNonStable.Clear();
            dataIds.Clear();
            freeIds.Clear();
        }

        public T this[int i]
        {
            get
            {
                if (i < 0 || i >= dataIds.Count)
                    throw new IndexOutOfRangeException();
                int index = dataIds[i];
                if (index == -1)
                    throw new Exception("Tried to access element that does not exist anymore");
                return data[index];
            }
            set
            {
                if (i < 0 || i >= dataIds.Count)
                    throw new IndexOutOfRangeException();
                int index = dataIds[i];
                if (index == -1)
                    throw new Exception("Tried to access element that does not exist anymore");
                data[index] = value;
            }
        }
        
        /// <summary>
        /// Gets the data directly from the stored list
        /// </summary>
        public T Get(int index)
        {
            if (index < 0 || index >= data.Count)
                throw new IndexOutOfRangeException();
            
            return data[index];
        }
        
        public bool TryGet(int id, out T? value)
        {
            value = default;
            if (id < 0 || id >= dataIds.Count)
                return false;
            int index = dataIds[id];
            if (index == -1)
                return false;
            value = data[index];
            return true;
        }

        public void RemoveAt(int id)
        {
            if (id < 0 || id >= dataIds.Count)
                return;

            int objectIndex = dataIds[id];
            if (objectIndex < 0)
                return;

            int lastIndex = data.Count - 1;
            int lastStableId = dataIdNonStable[lastIndex];

            if (objectIndex != lastIndex)
            {
                //swap with last element
                data[objectIndex] = data[lastIndex];
                //swap the id inline with it
                dataIdNonStable[objectIndex] = lastStableId;
                //swap the dataId associated with the data
                dataIds[lastStableId] = objectIndex;
            }
            
            dataIds[id] = -1;
            freeIds.Enqueue(id);
            
            data.RemoveAt(lastIndex);
            dataIdNonStable.RemoveAt(lastIndex);
        }

        public T[] ToArray()
        {
            return data.ToArray();
        }
    }
}