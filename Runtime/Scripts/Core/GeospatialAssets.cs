
namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// List of GUIDs allowing to load assets without hardcoded paths by using
    /// <see href="https://docs.unity3d.com/ScriptReference/AssetDatabase.GUIDToAssetPath.html">AssetDatabase.GUIDToAssetPath</see>
    /// </summary>
    public static class GeospatialAssets
    {
        /// <summary>
        /// Round atmospheric skybox used by <see cref="UGSkyboxBehaviour"/>.
        /// </summary>
        public const string SphericalSkybox = "3ad26f7af37d0f74bb9b2a63f714e443";

        /// <summary>
        /// High Definition Render Pipeline material factory to be set at <see cref="UGSystem.Configuration.MaterialFactory"/>.
        /// </summary>
        public const string HDRPMaterialFactory = "6e3cedafdf5610f49a8941c05be86b07";
        
        /// <summary>
        /// Universal Render Pipeline material factory to be set at <see cref="UGSystem.Configuration.MaterialFactory"/>.
        /// </summary>
        public const string URPMaterialFactory = "7f8b4dc2d6e1edd4daeb0ed88318ee79";
        
        /// <summary>
        /// Built-in material factory to be set at <see cref="UGSystem.Configuration.MaterialFactory"/>.
        /// </summary>
        public const string BuiltinMaterialFactory = "d995d7c41773bd8458b3aa5d4c27c300";
        
        /// <summary>
        /// Deffered material factory to be set at <see cref="UGSystem.Configuration.MaterialFactory"/>.
        /// </summary>
        public const string DeferredMaterialFactory = "83d0458cf8d11424f9ee5aa4850c11d7";
    }

}
