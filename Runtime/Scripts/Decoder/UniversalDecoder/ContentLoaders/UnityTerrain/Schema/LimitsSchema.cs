
using Unity.Geospatial.Streaming.UniversalDecoder;

namespace Unity.Geospatial.Streaming.UnityTerrain
{
    /// <summary>
    /// Layout information on how to divide the tiles.
    /// </summary>
    public class LimitsSchema
    {
        /// <summary>
        /// UTR layout used to divide the globe.
        /// </summary>
        public uint Layout { get; set; }

        /// <summary>
        /// Minimum <see cref="Node{T}.Level"/> in the hierarchy.
        /// Lower the number, closer to the root the <see cref="Node{T}"/> will be;
        /// Higher the number, deeper in the hierarchy it is.
        /// </summary>
        public int MinLevel { get; set; }

        /// <summary>
        /// Divide the tileset up to this <see cref="Node{T}.Level"/>.
        /// Higher zoom level are not part of this dataset.
        /// </summary>
        public uint MaxLevel { get; set; }

        /// <summary>
        /// The lower column value part of the dataset when dividing for the <see cref="MinLevel"/>.
        /// </summary>
        public uint MinCol { get; set; }

        /// <summary>
        /// The lower row value part of the dataset when dividing for the <see cref="MinLevel"/>.
        /// </summary>
        public uint MinRow { get; set; }

        /// <summary>
        /// The higher column value part of the dataset when dividing for the <see cref="MinLevel"/>.
        /// </summary>
        public uint MaxCol { get; set; }

        /// <summary>
        /// The higher row value part of the dataset when dividing for the <see cref="MinLevel"/>.
        /// </summary>
        public uint MaxRow { get; set; }
    }
}
