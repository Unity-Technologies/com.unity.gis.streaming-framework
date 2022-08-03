
namespace Unity.Geospatial.Streaming
{ 
    /// <summary>
    /// Base class to create <see cref="UGMaterial"/> instances with its attached <see cref="MaterialProperty">Properties</see>.
    /// </summary>
    public abstract class UGMaterialFactory
    {
        /// <summary>
        /// Create a new <see cref="UGMaterial"/> instance based on the given <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Determine which <see cref="UGMaterial"/> to instantiate.</param>
        /// <returns>The new material instance.</returns>
        public abstract UGMaterial InstantiateMaterial(MaterialType type);
    }
}
