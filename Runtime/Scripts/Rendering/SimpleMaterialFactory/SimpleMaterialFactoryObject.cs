using System;
using System.Collections.Generic;

using UnityEngine;


namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// Factory asset to be instantiated into a scene as a <see cref="UGMaterialFactory"/> for a single
    /// <see href="https://docs.unity3d.com/ScriptReference/Material.html">material</see> assignation per
    /// <see href="https://docs.unity3d.com/ScriptReference/Mesh.html">meshes</see>.
    /// </summary>
    [CreateAssetMenu(fileName = "Simple Material Factory", menuName = "Geospatial/Rendering/Simple Material Factory", order = k_AssetMenuOrder)]
    public class SimpleMaterialFactoryObject : UGMaterialFactoryObject
    {
        /// <summary>
        /// Specify which <see href="https://docs.unity3d.com/ScriptReference/Material.html">material</see> to use
        /// for a given <see cref="Unity.Geospatial.Streaming.MaterialType"/>.
        /// </summary>
        [Serializable]
        public struct DefaultMaterialObject
        {
            /// <summary>
            /// When this type is requested, <see cref="SimpleMaterialFactoryObject"/> will instantiate <see cref="Material"/>.
            /// </summary>
            public SerializableMaterialType MaterialType;
            
            /// <summary>
            /// <see href="https://docs.unity3d.com/ScriptReference/Material.html">Material</see> to instantiate
            /// when <see cref="MaterialType"/> is required.
            /// </summary>
            public Material Material;
            
            /// <summary>
            /// Create those <see cref="UGShaderProperty">properties</see> when instantiating the <see cref="Material"/>.
            /// </summary>
            public UGShaderPropertiesObject PropertyObject;
        }

        /// <summary>
        /// The <see href="https://docs.unity3d.com/ScriptReference/Material.html">material</see> creation directives
        /// for each <see cref="MaterialType"/>.
        /// </summary>
        public List<DefaultMaterialObject> DefaultMaterialObjectList;

        /// <inheritdoc cref="UGMaterialFactoryObject.Instantiate()"/>
        public override UGMaterialFactory Instantiate()
        {
            List<SimpleMaterialFactory.DefaultMaterial> m_DefaultMaterialList = new List<SimpleMaterialFactory.DefaultMaterial>();

            foreach (DefaultMaterialObject defaultMaterialObject in DefaultMaterialObjectList)
            {
                MaterialType materialType = (MaterialType)defaultMaterialObject.MaterialType;

                SimpleMaterialFactory.DefaultMaterial defaultMaterial = new SimpleMaterialFactory.DefaultMaterial(
                                                                           materialType,
                                                                           defaultMaterialObject.Material,
                                                                           defaultMaterialObject.PropertyObject.PropertyList
                                                                        );
                m_DefaultMaterialList.Add(defaultMaterial);
            }
            return new SimpleMaterialFactory(m_DefaultMaterialList);
        }
    }
}
