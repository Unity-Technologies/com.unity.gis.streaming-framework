
namespace Unity.Geospatial.Streaming.UnityTerrain
{
    /// <summary>
    /// Top schema item of the UnityTerrain json file.
    /// </summary>
    public class ConfigSchema
    {
        /// <summary>
        /// Define the <see href="https://docs.microsoft.com/en-us/dotnet/api/system.uri">URI</see> formatting.
        /// </summary>
        public ContentSchema Content { get; set; }

        /// <summary>
        /// Coordinates limits.
        /// </summary>
        public ExtentSchema Extent { get; set; }

        /// <summary>
        /// Layout information on how to divide the tiles.
        /// </summary>
        public LimitsSchema SetLimits { get; set; }

        /// <summary>
        /// The version of the encoded config.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// When loading a dataset, start with item first as the root parent.
        /// This item is also used to get the base URI in case sub-items have relative URI.
        /// </summary>
        /// <remarks>This doesn't need to have associated geometry, it can be a holder for a list of top parent items
        /// if the dataset has multiple roots.</remarks>
        /// <returns>The top parent starting the hierarchy.</returns>
        public Tile GetRoot()
        {
            return new Tile(
                -1,
                0,
                0,
                default,
                Content,
                Extent,
                SetLimits);
        }
    }
}
