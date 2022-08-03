
using UnityEngine;

namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// Base factory asset to be instantiated into a scene as a <see cref="UGMaterialFactory"/>.
    /// </summary>
    public abstract class UGMaterialFactoryObject : ScriptableObject
    {
        internal const int k_AssetMenuOrder = 101;

        /// <summary>
        /// Create a new <see cref="UGMaterialFactory"/> based on this instance values.
        /// </summary>
        /// <returns>The new <see cref="UGMaterialFactory"/> instance.</returns>
        public abstract UGMaterialFactory Instantiate();
    }
}
