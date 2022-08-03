using System.Collections.Generic;

using UnityEngine;

namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// <see cref="UGMaterial"/> instance creator with a single
    /// <see href="https://docs.unity3d.com/ScriptReference/Material.html">material</see>.
    /// </summary>
    public class UGSimpleMaterial : UGMaterial
    {
        private struct ResolvedProperty
        {
            public int ID;
            public int TextureScaleTiling;
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="mat">The material to be instantiated when applied to <see href="https://docs.unity3d.com/ScriptReference/Mesh.html">meshes</see>.</param>
        /// <param name="propertyList">Properties to be evaluated when applied.</param>
        public UGSimpleMaterial(Material mat, IEnumerable<UGShaderProperty> propertyList) : 
            base(isComposite: false)
        {
            m_Materials = new List<Material>();
            Material material = new Material(mat);

            //
            //  TODO - Is this really necessary?
            //
            material.hideFlags = HideFlags.DontSaveInEditor;
            m_Materials.Add(material);

            foreach(UGShaderProperty property in propertyList)
            {
                m_ResolvedProperties.Add(property.Type, new ResolvedProperty
                {
                    ID = Shader.PropertyToID(property.Name),
                    TextureScaleTiling = Shader.PropertyToID(property.TextureScaleTiling)
                });
            }
        }
        
        private readonly List<Material> m_Materials;
        
        private readonly Dictionary<MaterialPropertyType, ResolvedProperty> m_ResolvedProperties = new Dictionary<MaterialPropertyType, ResolvedProperty>();

        private delegate void MaterialWriter(Material material, MaterialProperty materialProperty, ResolvedProperty resolvedProperty);

        private static readonly Dictionary<MaterialPropertyValue, MaterialWriter> k_MaterialWriters = new Dictionary<MaterialPropertyValue, MaterialWriter>()
        {
            { MaterialPropertyValue.Color, SetColor },
            { MaterialPropertyValue.Float, SetFloat },
            { MaterialPropertyValue.Vector4, SetVector },
            { MaterialPropertyValue.Texture, SetTexture },
            { MaterialPropertyValue.TerrainTexture, SetTerrainTexture },
        };

        /// <inheritdoc cref="UGMaterial.UnityMaterials"/>
        public override List<Material> UnityMaterials
        {
            get
            {
                return m_Materials;
            }
        }

        /// <inheritdoc cref="UGMaterial.OnDispose"/>
        protected override void OnDispose()
        {
            foreach (Material material in m_Materials)
            {
                if (Application.isPlaying)
                    Object.Destroy(material);
                else
                    Object.DestroyImmediate(material);
            }
        }

        /// <inheritdoc cref="UGMaterial.OnAddMaterialProperty"/>
        protected override void OnAddMaterialProperty(MaterialProperty materialProperty)
        {
            if (m_ResolvedProperties.TryGetValue(materialProperty.Type, out ResolvedProperty property) &&
                k_MaterialWriters.TryGetValue(materialProperty.ValueType, out MaterialWriter writer))
            {
                writer.Invoke(m_Materials[0], materialProperty, property);
            }
        }

        /// <inheritdoc cref="UGMaterial.OnRemoveMaterialProperty"/>
        protected override void OnRemoveMaterialProperty(MaterialProperty materialProperty)
        {
            if (m_ResolvedProperties.TryGetValue(materialProperty.Type, out ResolvedProperty property) &&
                k_MaterialWriters.TryGetValue(materialProperty.ValueType, out MaterialWriter writer))
            {
                writer.Invoke(m_Materials[0], default, property);
            }
        }

        private static void SetColor(Material material, MaterialProperty materialProperty, ResolvedProperty property)
        {
            material.SetColor(property.ID, materialProperty.ColorValue);
        }

        private static void SetVector(Material material, MaterialProperty materialProperty, ResolvedProperty property)
        {
            material.SetVector(property.ID, materialProperty.Vector4Value);
        }

        private static void SetFloat(Material material, MaterialProperty materialProperty, ResolvedProperty property)
        {
            material.SetFloat(property.ID, materialProperty.FloatValue);
        }

        private static void SetTexture(Material material, MaterialProperty materialProperty, ResolvedProperty property)
        {
            material.SetTexture(property.ID, materialProperty.Texture);
            material.SetVector(property.TextureScaleTiling, materialProperty.Vector4Value);
        }

        private static void SetTerrainTexture(Material material, MaterialProperty materialProperty, ResolvedProperty property)
        {
            Debug.LogError("Simple UG Material class does not provide support for multi-textured terrains.");
        }
    }
}
