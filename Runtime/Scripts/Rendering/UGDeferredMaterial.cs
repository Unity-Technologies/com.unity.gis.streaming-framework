using System.Collections.Generic;

using UnityEngine;

namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// UGDeferredMaterial allows material customization at runtime per instance data.
    /// This can be done by retrieving the material properties per instance data, 
    /// then assigning these properties directly to the renderer's underlying Unity Materials.
    /// </summary>
    public class UGDeferredMaterial : UGMaterial
    {
        /// <summary>
        /// Instantiate this <see cref="UGDeferredMaterial"/> when this <see cref="MaterialType"/> is requested.
        /// </summary>
        public MaterialType Type { get; private set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="type">Instantiate this <see cref="UGDeferredMaterial"/> when this <see cref="MaterialType"/> is requested.</param>
        public UGDeferredMaterial(MaterialType type) : base(isComposite: false)
        {
            Type = type;
        }

        /// <inheritdoc cref="UGMaterial.UnityMaterials"/>
        public override List<Material> UnityMaterials { get; } = new List<Material> { null };

        /// <summary>
        /// The <see cref="MaterialProperty">properties</see> to apply to the <see cref="UGMaterial"/> when instantiated.
        /// </summary>
        public Dictionary<MaterialPropertyType, MaterialProperty> Properties { get; private set; } = new Dictionary<MaterialPropertyType, MaterialProperty>();

        /// <inheritdoc cref="UGMaterial.OnAddMaterialProperty"/>
        protected override void OnAddMaterialProperty(MaterialProperty materialProperty)
        {
            Properties.Add(materialProperty.Type, materialProperty);
        }

        /// <inheritdoc cref="UGMaterial.OnRemoveMaterialProperty"/>
        protected override void OnRemoveMaterialProperty(MaterialProperty materialProperty)
        {
            Properties.Remove(materialProperty.Type);
        }

        /// <inheritdoc cref="UGMaterial.OnDispose"/>
        protected override void OnDispose()
        {
            //
            //  Intentionally left blank
            //
        }
    }

}
