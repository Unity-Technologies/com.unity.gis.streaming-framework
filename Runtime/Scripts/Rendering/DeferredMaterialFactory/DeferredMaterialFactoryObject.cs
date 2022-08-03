
using UnityEngine;

namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// <see cref="UGMaterialFactoryObject"/> implementation where <see href="https://docs.unity3d.com/ScriptReference/Material.html">materials</see>
    /// get instantiated later by the <see cref="UGSystem"/> allowing dynamic creation based on the current state
    /// of the instance.
    /// </summary>
    [CreateAssetMenu(fileName = "Deferred Material Factory", menuName = "Geospatial/Rendering/Deferred Material Factory", order = k_AssetMenuOrder)]
    public class DeferredMaterialFactoryObject : UGMaterialFactoryObject
    {
        /// <inheritdoc cref="UGMaterialFactoryObject.Instantiate()"/>
        public override UGMaterialFactory Instantiate()
        {
            return new DeferredMaterialFactory();
        }
    }
}
