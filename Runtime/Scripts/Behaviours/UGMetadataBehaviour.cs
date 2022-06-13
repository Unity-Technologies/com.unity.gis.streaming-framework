using UnityEngine;

namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// The UGMetadataBehaviour is a class designed to wrap the UGMetadata
    /// object and expose it to the GameObject workflow. 
    /// </summary>
    public class UGMetadataBehaviour : MonoBehaviour
    {
        /// <summary>
        /// The Metadata of the given GameObject. If no metadata is present,
        /// this value can be null.
        /// </summary>
        public UGMetadata Metadata { get; set; }
    }
}
