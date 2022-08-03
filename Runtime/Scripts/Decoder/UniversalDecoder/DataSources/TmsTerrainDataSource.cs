using System;

using Unity.Geospatial.Streaming.UniversalDecoder;
using Unity.Mathematics;

namespace Unity.Geospatial.Streaming.TmsTerrain
{
    /// <summary>
    /// Data source information required by the <see cref="TmsTerrainContentLoader"/> to decoder its content.
    /// </summary>
    public class TmsTerrainDataSource : UniversalDecoderDataSource
    {
        /// <summary>
        /// Address of the terrain.json file to decoder.
        /// </summary>
        public string Uri { get; private set; }

        /// <summary>
        /// Lighting type to apply to the shading.
        /// </summary>
        public UGLighting Lighting { get; private set; }

        /// <summary>
        /// Multiply the <see cref="NodeData.GeometricError"/> with this value allowing resolution
        /// control per <see cref="UGDataSource"/>.
        /// </summary>
        public float DetailMultiplier { get; private set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="uri">Address of the .gltf / .glb file to decoder.</param>
        /// <param name="lighting">Lighting type to apply to the shading.</param>
        /// <param name="detailMultiplier">Multiply the <see cref="NodeData.GeometricError"/>
        /// with this value allowing resolution control per <see cref="UGDataSource"/>.</param>
        /// <param name="dataSourceId">Scriptable object associated with this instance.</param>
        public TmsTerrainDataSource(string uri, UGLighting lighting, float detailMultiplier, UGDataSourceID dataSourceId) : base(dataSourceId)
        {
            Uri = uri;
            Lighting = lighting;
            DetailMultiplier = detailMultiplier;
        }

        /// <inheritdoc cref="UniversalDecoderDataSource.InitializerDecoder"/>
        public override void InitializerDecoder(NodeContentManager contentManager)
        {
            Uri dataSourceUri = PathUtility.StringToUri(Uri);
            ContentType contentType = contentManager.ContentTypeGenerator.Generate();

            //
            //  Register content loader
            //
            TmsTerrainContentLoader tilesetLoader = new TmsTerrainContentLoader(contentManager, contentType);
            contentManager.RegisterLoader(contentType, tilesetLoader);

            TmsTerrainUriLoader utrLoader = new TmsTerrainUriLoader(contentManager, DataSourceID, Lighting, DetailMultiplier);
            tilesetLoader.RegisterUriLoader(utrLoader);

            tilesetLoader.AddTopLevelNode(
                contentManager,
                contentType,
                DataSourceID,
                double4x4.identity,
                DetailMultiplier,
                dataSourceUri,
                RefinementMode.Replace);
        }
    }
}
