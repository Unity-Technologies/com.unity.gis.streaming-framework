
namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// <see cref="UGMaterialFactory"/> implementation where <see href="https://docs.unity3d.com/ScriptReference/Material.html">materials</see>
    /// get instantiated later by the <see cref="UGSystem"/> allowing dynamic creation based on the current state
    /// of the instance.
    /// </summary>
    public class DeferredMaterialFactory : UGMaterialFactory
    {
        /// <inheritdoc cref="UGMaterialFactory.InstantiateMaterial(MaterialType)"/>
        public override UGMaterial InstantiateMaterial(MaterialType type)
        {
            return new UGDeferredMaterial(type);
        }
    }
}
