using System.Collections.Generic;

using UnityEngine;

namespace Unity.Geospatial.Streaming
{
    public class SimpleMaterialFactory : UGMaterialFactory
    {
        public struct DefaultMaterial
        {
            public MaterialType Type;
            public readonly Material Material;
            public readonly IReadOnlyList<UGShaderProperty> PropertyList;

            public DefaultMaterial(MaterialType materialType,
                                   Material material,
                                   IReadOnlyList<UGShaderProperty> propertyList)
            {
                Type = materialType;
                Material = material;
                PropertyList = propertyList;
            }
        }

        private readonly Dictionary<MaterialType, DefaultMaterial> m_DefaultMaterialDict;

        public SimpleMaterialFactory(List<DefaultMaterial> defaultMaterialList)
        {
            m_DefaultMaterialDict = new Dictionary<MaterialType, DefaultMaterial>();

            foreach (DefaultMaterial defaultMaterial in defaultMaterialList)
            {
                m_DefaultMaterialDict.Add(defaultMaterial.Type, defaultMaterial);
            }
        }

        public override UGMaterial InstantiateMaterial(MaterialType type)
        {
            DefaultMaterial defaultMaterial = m_DefaultMaterialDict[type];
            return new UGSimpleMaterial(defaultMaterial.Material, defaultMaterial.PropertyList);
        }
    }
}
