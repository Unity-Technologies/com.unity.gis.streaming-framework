using System.Collections.Generic;


namespace Unity.Geospatial.Streaming.UniversalDecoder
{
    /// <summary>
    /// A priority queue for the specified object type.
    /// </summary>
    /// <typeparam name="T">The type to be stored within the priority queue.</typeparam>
    public class PriorityQueue<T>
    {
        /// <summary>
        /// Private structure used to store the item type alongside its priority
        /// </summary>
        private readonly struct Item
        {
            /// <summary>
            /// The priority of a given item.
            /// </summary>
            public readonly double Priority;

            /// <summary>
            /// The generic item associated to the given priority.
            /// </summary>
            public readonly T Value;

            /// <summary>
            /// Constructor for an item.
            /// </summary>
            /// <param name="priority">The priority of the given item.</param>
            /// <param name="value">The item to be associated with the given priority.</param>
            public Item(double priority, T value)
            {
                Priority = priority;
                Value = value;
            }
        }

        /// <summary>
        /// The queue in which the items are stored
        /// </summary>
        private readonly List<Item> m_Queue = new List<Item>();

        /// <summary>
        /// Comparer used to compare Items to one another for the search algorithm. It is cached
        /// here to avoid multiple allocations.
        /// </summary>
        private readonly Comparer<Item> m_Comparer;

        /// <summary>
        /// Default constructor for the PriorityQueue
        /// </summary>
        public PriorityQueue()
        {
            m_Comparer = Comparer<Item>.Create((x, y) => y.Priority.CompareTo(x.Priority));
        }

        /// <summary>
        /// The number of items contained within the priority queue.
        /// </summary>
        public int Count { get { return m_Queue.Count; } }

        public void Clear()
        {
            m_Queue.Clear();
        }

        /// <summary>
        /// Enqueue and order based on priority. Lower values will come out first. Equal priorities will
        /// are garanteed to come out in FIFO order.
        /// </summary>
        /// <param name="priority">Priority value - lower values have priority over higher values.</param>
        /// <param name="item">The item to be enqueued within the priority queue.</param>
        public void Enqueue(double priority, T item)
        {
            int location = FindLocation(priority);
            m_Queue.Insert(location, new Item(priority, item));
        }

        /// <summary>
        /// Internal method to find the location in which a given priority should be placed in the list.
        /// </summary>
        /// <param name="priority">The priority of the item to be inserted.</param>
        /// <returns>The index at which the item should be inserted.</returns>
        private int FindLocation(double priority)
        {
            var item = new Item(priority, default(T));
            int location = m_Queue.BinarySearch(item, m_Comparer);
            if (location < 0)
            {
                location = ~location;
            }
            return location;
        }

        /// <summary>
        /// Remove the item with priority from the queue.
        /// </summary>
        /// <returns>The item with priority</returns>
        public T Dequeue()
        {
            var result = m_Queue[m_Queue.Count - 1];
            m_Queue.RemoveAt(m_Queue.Count - 1);
            return result.Value;
        }
    }
}
