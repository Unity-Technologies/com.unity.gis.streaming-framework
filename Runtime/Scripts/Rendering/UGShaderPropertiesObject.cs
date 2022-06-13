using System.Collections.Generic;
using UnityEngine;

namespace Unity.Geospatial.Streaming
{
    [CreateAssetMenu(fileName = "Shader Properties", menuName = "Geospatial/Rendering/Shader Properties", order = 102)]
    public class UGShaderPropertiesObject : ScriptableObject
    {
        public List<UGShaderProperty> PropertyList;
    }
}
