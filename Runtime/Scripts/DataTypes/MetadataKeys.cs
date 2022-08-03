namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// Keys to be used when storing values in the <see cref="UGMetadata"/> as
    /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.keyvaluepair-2">KeyValuePair</see>.
    /// </summary>
    public static class MetadataKeys
    {
        /// <summary>
        /// Key used when storing a DoubleBounds value in a <see cref="UGMetadata"/> instance.
        /// </summary>
        public const string Bounds = "com.unity.geospatial.bounds";
        
        /// <summary>
        /// Key used when storing a <see cref="Unity.Geospatial.Streaming.UniversalDecoder.NodeId"/>
        /// in a <see cref="UGMetadata"/> instance.
        /// </summary>
        public const string NodeId = "com.unity.geospatial.nodeid";
        
        /// <summary>
        /// Key used when storing a <see cref="Unity.Geospatial.Streaming.UniversalDecoder.NodeData.GeometricError"/> value
        /// in a <see cref="UGMetadata"/> instance.
        /// </summary>
        public const string GeometricError = "com.unity.geospatial.geometricerror";
    }
}
