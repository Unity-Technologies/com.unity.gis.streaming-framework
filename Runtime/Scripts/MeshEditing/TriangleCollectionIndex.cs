
namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// Mesh triangles collection indirection allowing giving collections by index instead of the complete array.
    /// </summary>
    public readonly struct TriangleCollectionIndex
    {
        /// <summary>
        /// Unique index this collection refers to.
        /// </summary>
        public readonly int Index;
        
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="index">Unique index the collection will refer to.</param>
        public TriangleCollectionIndex(int index)
        {
            Index = index;
        }
        
        /// <summary>
        /// Get if the collection does not refers to a <see langword="null"/> collection.
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return Index != -1;
        }

        /// <summary>
        /// Get a <see langword="null"/> collection where the instance has no triangles.
        /// </summary>
        public static readonly TriangleCollectionIndex Null = new TriangleCollectionIndex(-1);
    }
}
