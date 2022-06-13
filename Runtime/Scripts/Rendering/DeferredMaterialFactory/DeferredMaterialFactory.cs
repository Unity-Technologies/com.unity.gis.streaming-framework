
namespace Unity.Geospatial.Streaming
{
    public class DeferredMaterialFactory : UGMaterialFactory
    {
        public override UGMaterial InstantiateMaterial(MaterialType type)
        {
            return new UGDeferredMaterial(type);
        }
    }
}
