using UnityEngine;

namespace Unity.Geospatial.Streaming
{
    public class GONodeRenderer : GOBaseRenderer
    {
        public override int UnityLayer
        {
            set
            {
                m_GameObject.layer = value;
            }
        }

        public override Mesh Mesh
        {
            set{}
        }
        public override UGMaterial[] Materials
        {
            set{}
        }
    }
}
