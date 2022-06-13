
namespace Unity.Geospatial.Streaming.UnityTerrain
{
    /// <summary>
    /// Define how to format the <see href="https://docs.microsoft.com/en-us/dotnet/api/system.uri">URI</see> part of this dataset.
    /// </summary>
    public class ContentSchema
    {
        /// <summary>
        /// File extension of the geometry files without the dot.
        /// </summary>
        public string TerrainFormat { get; set; }

        /// <summary>
        /// Base <see href="https://docs.microsoft.com/en-us/dotnet/api/system.uri">URI</see>
        /// each time a terrain geometry file is requested.
        /// </summary>
        public string TerrainUri { get; set; }

        /// <summary>
        /// File extension of the texture files without the dot.
        /// </summary>
        public string ImageryFormat { get; set; }

        /// <summary>
        /// Base <see href="https://docs.microsoft.com/en-us/dotnet/api/system.uri">URI</see>
        /// each time a terrain texture file is requested.
        /// </summary>
        public string ImageryUri { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="terrainUri">Base <see href="https://docs.microsoft.com/en-us/dotnet/api/system.uri">URI</see>
        /// each time a terrain geometry file is requested.</param>
        /// <param name="terrainFormat">File extension of the geometry files without the dot.</param>
        /// <param name="imageryUri">Base <see href="https://docs.microsoft.com/en-us/dotnet/api/system.uri">URI</see>
        /// each time a terrain texture file is requested.</param>
        /// <param name="imageryFormat">File extension of the texture files without the dot.</param>
        public ContentSchema(string terrainUri, string terrainFormat, string imageryUri, string imageryFormat)
        {
            ImageryUri = imageryUri;
            ImageryFormat = imageryFormat;
            TerrainUri = terrainUri;
            TerrainFormat = terrainFormat;
        }
    }
    
}
