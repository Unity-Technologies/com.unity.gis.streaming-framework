using System.Collections.Generic;

using UnityEngine;

namespace Unity.Geospatial.Streaming
{
    public class UGDeferredMaterial : UGMaterial
    {
        public MaterialType Type { get; private set; }

        public UGDeferredMaterial(MaterialType type) : base(isComposite: false)
        {
            Type = type;
        }

        public override List<Material> UnityMaterials { get; } = new List<Material> { null };

        public Dictionary<MaterialPropertyType, MaterialProperty> Properties { get; private set; } = new Dictionary<MaterialPropertyType, MaterialProperty>();

        protected override void OnAddMaterialProperty(MaterialProperty materialProperty)
        {
            Properties.Add(materialProperty.Type, materialProperty);
        }

        protected override void OnRemoveMaterialProperty(MaterialProperty materialProperty)
        {
            Properties.Remove(materialProperty.Type);
        }

        protected override void OnDispose()
        {
            //
            //  Intentionally left blank
            //
        }
    }

}
