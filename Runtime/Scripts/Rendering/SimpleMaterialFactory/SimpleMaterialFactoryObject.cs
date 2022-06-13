using System;
using System.Collections.Generic;

using UnityEngine;


namespace Unity.Geospatial.Streaming
{
    [CreateAssetMenu(fileName = "Simple Material Factory", menuName = "Geospatial/Rendering/Simple Material Factory", order = k_AssetMenuOrder)]
    public class SimpleMaterialFactoryObject : UGMaterialFactoryObject
    {
        [Serializable]
        public struct DefaultMaterialObject
        {
            public SerializableMaterialType MaterialType;
            public Material Material;
            public UGShaderPropertiesObject PropertyObject;
        }

        public List<DefaultMaterialObject> DefaultMaterialObjectList;

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
