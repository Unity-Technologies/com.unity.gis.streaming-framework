using UnityEngine;

namespace Unity.Geospatial.Streaming
{
    public abstract class UGSceneObserverBehaviour : MonoBehaviour
    {
        public abstract UGSceneObserver Instantiate(UGSystemBehaviour ugSystem);
    }
}
