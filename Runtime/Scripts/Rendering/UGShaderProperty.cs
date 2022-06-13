using System;

namespace Unity.Geospatial.Streaming
{
    [Serializable]
    public class UGShaderProperty
    {
        public MaterialPropertyType Type;
        public string Name;
        public string TextureScaleTiling;

        public UGShaderProperty(MaterialPropertyType type, string name)
        {
            Type = type;
            Name = name;
            TextureScaleTiling = null;
        }

        public UGShaderProperty(MaterialPropertyType type, string name, string st)
        {
            Type = type;
            Name = name;
            TextureScaleTiling = st;
        }
    }
}
