
namespace Unity.Geospatial.Streaming.UniversalDecoder
{
    /// <summary>
    /// <see cref="NodeContent"/> associated with a specific <see cref="UriLoader"/> and children information
    /// allowing to generate the hierarchy on demand.
    /// </summary>
    public interface IExpandingNodeContent
    {
        /// <summary>
        /// Item available to be loaded by a <see cref="UriLoader"/>.
        /// </summary>
        ILeaf Leaf { get; }

        /// <summary>
        /// The parent refine value in case the refine value on this item is not set.
        /// </summary>
        RefinementMode InheritedRefineMode { get; }
    }
}
