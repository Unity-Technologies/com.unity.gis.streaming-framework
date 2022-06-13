
using UnityEngine;

namespace Unity.Geospatial.Streaming
{
    public abstract class UGMaterialFactoryObject : ScriptableObject
    {
        internal const int k_AssetMenuOrder = 101;

        public abstract UGMaterialFactory Instantiate();
    }
}
