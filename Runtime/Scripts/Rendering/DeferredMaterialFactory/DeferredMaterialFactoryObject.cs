
using UnityEngine;

namespace Unity.Geospatial.Streaming
{
    [CreateAssetMenu(fileName = "Deferred Material Factory", menuName = "Geospatial/Rendering/Deferred Material Factory", order = k_AssetMenuOrder)]
    public class DeferredMaterialFactoryObject : UGMaterialFactoryObject
    {
        public override UGMaterialFactory Instantiate()
        {
            return new DeferredMaterialFactory();
        }
    }
}
