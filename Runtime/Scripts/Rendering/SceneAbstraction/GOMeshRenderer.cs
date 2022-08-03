using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// Class responsible for converting <see cref="InstanceID">Instances</see> into <see cref="GameObject">GameObjects</see>
    /// with just a simple static <see cref="MeshRenderer"/>.
    /// </summary>
    public class GOMeshRenderer : GOBaseRenderer
    {
        private sealed class MaterialListener
        {
            public MaterialListener(Material[] setupMaterials, MeshRenderer meshRenderer, int index)
            {
                m_MeshRenderer = meshRenderer;
                m_Index = index;
                m_SetupMaterials = setupMaterials;
            }

            private readonly MeshRenderer m_MeshRenderer;

            private readonly int m_Index;

            private readonly Material[] m_SetupMaterials;

            public void OnMaterialUpdate(UGMaterial ugMaterial)
            {
                Assert.IsFalse(ugMaterial.IsComposite);
                Assert.AreEqual(1, ugMaterial.UnityMaterials.Count);

                m_SetupMaterials[m_Index] = ugMaterial.UnityMaterials[0];
                m_MeshRenderer.sharedMaterials = m_SetupMaterials;
            }


        }

        /// <summary>
        /// Default constructor.
        /// Responsible to create <see href="https://docs.unity3d.com/ScriptReference/MeshFilter.html">MeshFilter</see>
        /// and <see href="https://docs.unity3d.com/ScriptReference/MeshRenderer.html">MeshRenderer</see>
        /// <see href="https://docs.unity3d.com/ScriptReference/Component.html">Components</see>.
        /// </summary>
        public GOMeshRenderer()
        {
            m_MeshFilter = m_GameObject.AddComponent<MeshFilter>();
            m_MeshRenderer = m_GameObject.AddComponent<MeshRenderer>();
        }
        
        private readonly MeshFilter m_MeshFilter;

        private readonly MeshRenderer m_MeshRenderer;

        private UGMaterial[] m_Materials;

        private MaterialListener[] m_MaterialListeners;

        /// <inheritdoc cref="GOBaseRenderer.UnityLayer"/>
        public override int UnityLayer
        {
            set
            {
                m_GameObject.layer = value;
            }
        }

        /// <inheritdoc cref="GOBaseRenderer.Mesh"/>
        public override Mesh Mesh
        {
            set
            {
                m_MeshFilter.sharedMesh = value;
            }
        }

        /// <inheritdoc cref="GOBaseRenderer.Materials"/>
        public override UGMaterial[] Materials
        {
            set
            {
                if (m_Materials != null)
                {
                    Assert.AreEqual(m_Materials.Length, m_MaterialListeners.Length);

                    for(int i = 0; i < m_Materials.Length; i++)
                    {
                        m_Materials[i].RemoveListener(m_MaterialListeners[i].OnMaterialUpdate);
                    }

                    m_Materials = null;
                    m_MaterialListeners = null;
                }

                if (value != null)
                {
                    m_Materials = value;
                    m_MaterialListeners = new MaterialListener[value.Length];
                    Material[] setupMaterials = new Material[value.Length];
                    
                    for (int i = 0; i < m_Materials.Length; i++)
                    {
                        m_MaterialListeners[i] = new MaterialListener(setupMaterials, m_MeshRenderer, i);
                        m_Materials[i].AddListener(m_MaterialListeners[i].OnMaterialUpdate);
                        m_MaterialListeners[i].OnMaterialUpdate(m_Materials[i]);
                    }
                }
            }
        }
    }
}
