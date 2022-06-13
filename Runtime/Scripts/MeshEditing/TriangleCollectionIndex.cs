
namespace Unity.Geospatial.Streaming
{
    public readonly struct TriangleCollectionIndex
    {
        public readonly int Index;
        
        public TriangleCollectionIndex(int index)
        {
            Index = index;
        }
        
        public bool IsValid()
        {
            return Index != -1;
        }

        public static readonly TriangleCollectionIndex Null = new TriangleCollectionIndex(-1);
    }
}
