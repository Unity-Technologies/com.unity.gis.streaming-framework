using System;


namespace Unity.Geospatial.Streaming.UniversalDecoder
{
    /// <summary>
    /// Base class that defines a layer of data available to be loaded via a <see cref="NodeContentManager"/>.
    /// </summary>
    public abstract class UniversalDecoderDataSource : UGDataSource
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="dataSourceId">Scriptable object associated with this instance.</param>
        protected UniversalDecoderDataSource(UGDataSourceID dataSourceId) :
            base(dataSourceId)
        { 
        }

        /// <inheritdoc cref="UGDataSource.GetDecoderType"/>
        public override Type GetDecoderType()
        {
            return typeof(UGUniversalDecoder);
        }

        /// <inheritdoc cref="UGDataSource.InstantiateDecoder"/>
        public override UGDataSourceDecoder InstantiateDecoder(UUIDGenerator idGenerator, UGMaterialFactory materialFactory, UGDataSource[] dataSources, int maximumSimultaneousContentRequests)
        {
            return new UGUniversalDecoder(materialFactory, dataSources, maximumSimultaneousContentRequests);
        }

        /// <summary>
        /// Initialize decoder by adding relevant nodes to the hierarchy and by initializing the content manager
        /// with the appropriately configured loaders.
        /// </summary>
        /// <param name="contentManager">The content manager to be initialized with this datasource</param>
        public abstract void InitializerDecoder(NodeContentManager contentManager);

    }
}
