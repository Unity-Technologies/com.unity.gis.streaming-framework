using System.Collections.Generic;

using Unity.Geospatial.HighPrecision;
using Unity.Geospatial.Streaming.UniversalDecoder;

namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// This class is designed to contain a geospatial object's metadata. By default,
    /// this metadata is comprised of a string dictionary which can contain arbitrary
    /// values. However, should a particular use-case require metadata of a different
    /// form, such as a hierarchy of elements, this class can be derived and the
    /// derived class can be assigned to instances by the decoder.
    /// </summary>
    public class UGMetadata
    {
        /// <summary>
        /// Construct a new metadata object
        /// </summary>
        /// <param name="properties">Dictionary of strings containing metadata. If null, 
        /// a new dictionary will be instantiated and properties can be assigned post
        /// construction.</param>
        public UGMetadata(Dictionary<string, object> properties = null)
        {
            Properties = properties ?? new Dictionary<string, object>();
        }

        /// <summary>
        /// Construct a new metadata object with each individual values specified.
        /// </summary>
        /// <param name="nodeId">The id of the node this metadata instance is linked with.</param>
        /// <param name="bounds">Define the limits of the content allowing to calculate the screen space error.</param>
        /// <param name="geometricError">The geometric error of the content.
        /// If the evaluated screen space error is higher than this value, the node will not be expanded.</param>
        public UGMetadata(NodeId nodeId, DoubleBounds bounds, float geometricError) :
            this()
        {
            SetNodeId(nodeId);
            SetBounds(bounds);
            SetGeometricError(geometricError);
        }

        /// <summary>
        /// Construct a new metadata object and fill it with values based on the given <paramref name="data"/> values.
        /// </summary>
        /// <param name="nodeId">Id of the node to create the metadata instance for.</param>
        /// <param name="getter">Instance to get the values from.</param>
        public UGMetadata(NodeId nodeId, IGetNodeData getter) :
            this(nodeId, getter.GetBounds(nodeId), getter.GetGeometricError(nodeId)) { }

        /// <summary>
        /// The metadata properties
        /// </summary>
        public Dictionary<string, object> Properties { get; private set; }

        /// <summary>
        /// Get all the data that was loaded for the <see cref="InstanceID"/> linked with this <see cref="UGMetadata"/> instance.
        /// </summary>
        public InstanceData InstanceData { get; set; }

        /// <summary>
        /// Update the <see cref="NodeId"/> value part.
        /// </summary>
        /// <param name="nodeId">The id of the node this metadata instance is linked with.</param>
        private void SetNodeId(NodeId nodeId)
        {
            Properties[MetadataKeys.NodeId] = nodeId;
        }

        /// <summary>
        /// Update the <see cref="DoubleBounds"/> value part.
        /// </summary>
        /// <param name="bounds">Define the limits of the content allowing to calculate the screen space error.</param>
        private void SetBounds(DoubleBounds bounds)
        {
            Properties[MetadataKeys.Bounds] = (SerializableDoubleBounds)bounds;
        }

        /// <summary>
        /// Update the GeometricError value part.
        /// </summary>
        /// <param name="geometricError">The geometric error of the content.
        /// If the evaluated screen space error is higher than this value, the node will not be expanded.</param>
        private void SetGeometricError(float geometricError)
        {
            Properties[MetadataKeys.GeometricError] = geometricError;
        }
    }
}
