using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.Geospatial.Streaming
{
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

        public GOMeshRenderer()
        {
            m_MeshFilter = m_GameObject.AddComponent<MeshFilter>();
            m_MeshRenderer = m_GameObject.AddComponent<MeshRenderer>();
        }


        private readonly MeshFilter m_MeshFilter;

        private readonly MeshRenderer m_MeshRenderer;

        private UGMaterial[] m_Materials;

        private MaterialListener[] m_MaterialListeners;

        public override int UnityLayer
        {
            set
            {
                m_GameObject.layer = value;
            }
        }

        public override Mesh Mesh
        {
            set
            {
                m_MeshFilter.sharedMesh = value;
            }
        }

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
