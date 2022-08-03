using System.Collections.Generic;
using UnityEngine;

namespace Unity.Geospatial.Streaming
{
    /// <summary>
    /// <see href="https://docs.unity3d.com/ScriptReference/ScriptableObject.html">ScriptableObject</see> for
    /// <see cref="UGShaderProperty"/> to be applied on <see href="https://docs.unity3d.com/ScriptReference/Material.html">materials</see>.
    /// </summary>
    [CreateAssetMenu(fileName = "Shader Properties", menuName = "Geospatial/Rendering/Shader Properties", order = 102)]
    public class UGShaderPropertiesObject : ScriptableObject
    {
        /// <summary>
        /// List of <see cref="UGShaderProperty"/> to be applied.
        /// </summary>
        public List<UGShaderProperty> PropertyList;
    }
}
