using UnityEngine;

namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// Empty renderer where the <see cref="Mesh"/> and <see cref="Materials"/> instantiation is skipped.
    /// This can be useful when creating a hierarchy of nodes that has no geometry to be rendered.
    /// </summary>
    public class GONodeRenderer : GOBaseRenderer
    {
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
            set{}
        }
        
        /// <inheritdoc cref="GOBaseRenderer.Materials"/>
        public override UGMaterial[] Materials
        {
            set{}
        }
    }
}
