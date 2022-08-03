using UnityEngine;

namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// Base class to <see cref="Instantiate"/> a <see cref="UGSceneObserver"/>.
    /// Scene observer tells the <see cref="UGSystem"/> which geometry must be loaded at which resolution by calculating
    /// the <see cref="DetailObserverData.GetErrorSpecification">error specification</see> based on the space it takes in the field of view of the
    /// <see cref="UGSceneObserver"/>.
    /// </summary>
    public abstract class UGSceneObserverBehaviour : MonoBehaviour
    {
        /// <summary>
        /// Create a new <see cref="UGSceneObserver"/> of this camera and link it with the given <paramref name="ugSystem">system </paramref>.
        /// </summary>
        /// <param name="ugSystem">Create the instance under this parent node.</param>
        /// <returns>The instantiated observer.</returns>
        public abstract UGSceneObserver Instantiate(UGSystemBehaviour ugSystem);
    }
}
