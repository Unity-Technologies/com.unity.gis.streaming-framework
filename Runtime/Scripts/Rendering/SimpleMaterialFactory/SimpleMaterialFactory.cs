using System.Collections.Generic;

using UnityEngine;

namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// Class to create a single <see cref="UGMaterial"/> instances with its attached <see cref="MaterialProperty">Properties</see>.
    /// </summary>
    public class SimpleMaterialFactory : UGMaterialFactory
    {
        /// <summary>
        /// Specify which <see href="https://docs.unity3d.com/ScriptReference/Material.html">material</see> to use
        /// for the given <see cref="Type"/>.
        /// </summary>
        public struct DefaultMaterial
        {
            /// <summary>
            /// When this type is requested, <see cref="SimpleMaterialFactory"/> will instantiate <see cref="Material"/>.
            /// </summary>
            public MaterialType Type;
            
            /// <summary>
            /// <see href="https://docs.unity3d.com/ScriptReference/Material.html">Material</see> to instantiate
            /// when <see cref="Type"/> is required.
            /// </summary>
            public readonly Material Material;
            
            /// <summary>
            /// Create those <see cref="UGShaderProperty">properties</see> when instantiating the <see cref="Material"/>.
            /// </summary>
            public readonly IReadOnlyList<UGShaderProperty> PropertyList;

            /// <summary>
            /// Default constructor.
            /// </summary>
            /// <param name="materialType">
            /// When this type is requested, <see cref="SimpleMaterialFactory"/> will instantiate <see cref="Material"/>.
            /// </param>
            /// <param name="material">
            /// <see href="https://docs.unity3d.com/ScriptReference/Material.html">Material</see> to instantiate
            /// when <see cref="Type"/> is required.
            /// </param>
            /// <param name="propertyList">
            /// Create those <see cref="UGShaderProperty">properties</see> when instantiating the <see cref="Material"/>.
            /// </param>
            public DefaultMaterial(MaterialType materialType,
                                   Material material,
                                   IReadOnlyList<UGShaderProperty> propertyList)
            {
                Type = materialType;
                Material = material;
                PropertyList = propertyList;
            }
        }

        /// <summary>
        /// Mapping where the key is the <see cref="MaterialType"/> requested by <see cref="InstantiateMaterial"/>
        /// and the value is the <see href="https://docs.unity3d.com/ScriptReference/Material.html">material</see>
        /// and its <see cref="UGShaderProperty">properties</see>.
        /// </summary>
        private readonly Dictionary<MaterialType, DefaultMaterial> m_DefaultMaterialDict;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="defaultMaterialList">Directives telling which
        /// <see href="https://docs.unity3d.com/ScriptReference/Material.html">material</see> to instantiate
        /// by matching its <see cref="DefaultMaterial.Type">Type</see>.</param>
        public SimpleMaterialFactory(List<DefaultMaterial> defaultMaterialList)
        {
            m_DefaultMaterialDict = new Dictionary<MaterialType, DefaultMaterial>();

            foreach (DefaultMaterial defaultMaterial in defaultMaterialList)
            {
                m_DefaultMaterialDict.Add(defaultMaterial.Type, defaultMaterial);
            }
        }

        /// <inheritdoc cref="UGMaterialFactory.InstantiateMaterial(MaterialType)"/>
        public override UGMaterial InstantiateMaterial(MaterialType type)
        {
            DefaultMaterial defaultMaterial = m_DefaultMaterialDict[type];
            return new UGSimpleMaterial(defaultMaterial.Material, defaultMaterial.PropertyList);
        }
    }
}
