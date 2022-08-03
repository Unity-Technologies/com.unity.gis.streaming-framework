using System;

using Unity.Geospatial.Streaming.UniversalDecoder;

namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// Base class that defines a layer of data available to be loaded. This can be a single file, a collection of
    /// files, a stream, a connection to a server data set or any other ways to access data.
    /// </summary>
    public abstract class UGDataSource
    {
        /// <summary>
        /// Unique identifier allowing to do indirect assignment.
        /// </summary>
        public readonly UGDataSourceID DataSourceID;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="dataSourceId">Scriptable object associated with this instance.</param>
        protected UGDataSource(UGDataSourceID dataSourceId)
        {
            DataSourceID = dataSourceId;
        }

        /// <summary>
        /// Type of <see cref="UGDataSourceDecoder"/> allowing to define with which other <see cref="UGDataSource"/> this instance can be used with.
        /// </summary>
        /// <returns>The type of decoder associated with this instance.</returns>
        public abstract Type GetDecoderType();

        /// <summary>
        /// Create a new <see cref="UGDataSourceDecoder"/> instance allowing to decode / read from this source.
        /// </summary>
        /// <param name="idGenerator">Class instance to use to create unique identifier.</param>
        /// <param name="materialFactory">Which material factory to be used by the renderer.</param>
        /// <param name="dataSources">Array of <see cref="UGDataSource"/> instance available to be loaded.</param>
        /// <param name="maximumSimultaneousContentRequests">The <see cref="UGUniversalDecoder.MaximumSimultaneousContentRequests"/> will be set to this value.</param>
        /// <returns></returns>
        public abstract UGDataSourceDecoder InstantiateDecoder(UUIDGenerator idGenerator, UGMaterialFactory materialFactory, UGDataSource[] dataSources, int maximumSimultaneousContentRequests);
    }
}
