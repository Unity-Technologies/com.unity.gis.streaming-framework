
namespace Unity.Geospatial.Streaming.UniversalDecoder
{
    /// <summary>
    /// <see cref="NodeContent"/> associated with a specific <see cref="UriLoader"/> and children information
    /// allowing to generate the hierarchy on demand.
    /// </summary>
    public interface IExpandingNodeContent
    {
        ILeaf Leaf { get; }

        RefinementMode InheritedRefineMode { get; }
    }
}
