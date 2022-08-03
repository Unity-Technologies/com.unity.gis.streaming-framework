using System;

using Newtonsoft.Json.Linq;
using Unity.Geospatial.Streaming.UniversalDecoder;
using Unity.Mathematics;

namespace Unity.Geospatial.Streaming.Ogc3dTiles
{
    /// <summary>
    /// Data source information required by the <see cref="OgcTilesetContentLoader{TTileset,TTile}"/> to decoder its content.
    /// </summary>
    public class Ogc3dTilesDataSource : UniversalDecoderDataSource
    {
        /// <summary>
        /// Address of the tileset.json file to decoder.
        /// </summary>
        public string Uri { get; private set; }

        /// <summary>
        /// Lighting type to apply to the shading.
        /// </summary>
        public UGLighting Lighting { get; private set; }

        /// <summary>
        /// Texture options to apply for each imported texture.
        /// </summary>
        public UGTextureSettings TextureSettings { get; private set; }

        /// <summary>
        /// Multiply the <see cref="NodeData.GeometricError"/> with this value allowing resolution
        /// control per <see cref="UGDataSource"/>.
        /// </summary>
        public float DetailMultiplier { get; private set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="uri">The URI of the 3D Tiles dataset</param>
        /// <param name="lighting">Lighting override, to force lit or unlit as required.</param>
        /// <param name="textureSettings">Texture settings to override texture filtering.</param>
        /// <param name="detailMultiplier">Detail multiplier, which adjusts the level of details of the specific data source.</param>
        /// <param name="dataSourceId">The id of the corresponding datasource</param>
        public Ogc3dTilesDataSource(string uri, UGLighting lighting, UGTextureSettings textureSettings, float detailMultiplier, UGDataSourceID dataSourceId) :
            base(dataSourceId)
        {
            Uri = uri;
            Lighting = lighting;
            TextureSettings = textureSettings;
            DetailMultiplier = detailMultiplier;
        }

        /// <inheritdoc cref="UniversalDecoderDataSource.InitializerDecoder"/>
        public override void InitializerDecoder(NodeContentManager contentManager)
        {
            Uri dataSourceUri = PathUtility.StringToUri(Uri);
            ContentType contentType = contentManager.ContentTypeGenerator.Generate();

            OgcTilesetContentLoader<TilesetSchema, TileSchema> tilesetLoader = new OgcTilesetContentLoader<TilesetSchema, TileSchema>(contentManager, contentType);
            contentManager.RegisterLoader(contentType, tilesetLoader);

            GltfUriLoader gltfLoader = new GltfUriLoader(contentManager, DataSourceID, Lighting, TextureSettings, GltfUriLoader.ZMinusForwardMatrix);
            tilesetLoader.RegisterUriLoader(gltfLoader);

            OgcB3dmUriLoader<B3dmTableSchema, JToken> b3dmLoader = new OgcB3dmUriLoader<B3dmTableSchema, JToken>(
                contentManager, DataSourceID, Lighting, TextureSettings, GltfUriLoader.ZMinusForwardMatrix);
            tilesetLoader.RegisterUriLoader(b3dmLoader);

            tilesetLoader.AddTopLevelNode(
                contentManager,
                contentType,
                DataSourceID,
                double4x4.identity,
                DetailMultiplier,
                dataSourceUri,
                RefinementMode.Add);
        }
    }
}
