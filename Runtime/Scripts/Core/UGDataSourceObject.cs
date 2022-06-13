using UnityEngine;

namespace Unity.Geospatial.Streaming
{
    public abstract class UGDataSourceObject : ScriptableObject
    {
        internal const int AssetMenuOrder = 100;

        /// <summary>
        /// Scriptable object associated with this instance.
        /// </summary>
        public abstract UGDataSourceID DataSourceID { get; }

        /// <summary>
        /// Instantiates the underlying UGDataSource object. For most use-cases, this should
        /// only be called by the UGSystemBehaviour.
        /// </summary>
        /// <param name="system">Instantiate the <see cref="UGDataSource"/> as child of this <see cref="UGSystemBehaviour"/>.</param>
        /// <returns>The newly instantiated <see cref="UGDataSource"/>.</returns>
        public abstract UGDataSource Instantiate(UGSystemBehaviour system);

    }

}
