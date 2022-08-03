using UnityEngine;

namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// Base class that defines an <see href="https://docs.unity3d.com/ScriptReference/ScriptableObject.html">ScriptableObject</see>
    /// available to be loaded. This can be a single file, a collection of files, a stream, a connection to a server
    /// data set or any other ways to access data.
    /// </summary>
    public abstract class UGDataSourceObject : ScriptableObject
    {
        /// <summary>
        /// The position of the menu item within the Assets/Create menu.
        /// </summary>
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
