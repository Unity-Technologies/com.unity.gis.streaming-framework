Shader "Geospatial/Builtin/UnlitTerrain"
{
    Properties
    {
        _DefaultColor ("DefaultColor", Color) = (0,0,0,1)

        _ATexture ("ATexture", 2D) = "black" {}
        _AMask ("AMask", Vector) = (0,0,1,1)

        _BTexture("BTexture", 2D) = "black" {}
        _BMask("BMask", Vector) = (0,0,1,1)

        _CTexture("CTexture", 2D) = "black" {}
        _CMask("CMask", Vector) = (0,0,1,1)

        _DTexture("DTexture", 2D) = "black" {}
        _DMask("DMask", Vector) = (0,0,1,1)

        _DepthBias("Depth Bias", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 uva : TEXCOORD1;
                float2 uvb : TEXCOORD2;
                float2 uvc : TEXCOORD3;
                float2 uvd : TEXCOORD4;
                UNITY_FOG_COORDS(5)
                float4 vertex : SV_POSITION;
            };

            fixed4 _DefaultColor;

            sampler2D _ATexture;
            float4 _AMask;
            float4 _ATexture_ST;

            sampler2D _BTexture;
            float4 _BMask;
            float4 _BTexture_ST;

            sampler2D _CTexture;
            float4 _CMask;
            float4 _CTexture_ST;

            sampler2D _DTexture;
            float4 _DMask;
            float4 _DTexture_ST;

            float _DepthBias;

            v2f vert (appdata v)
            {
                v2f o;
                float3 viewPos = mul(UNITY_MATRIX_MV, v.vertex); //Intentionnaly using matrix to improve reduce artifacts on large scale worlds
                o.vertex = mul(UNITY_MATRIX_P, float4(viewPos * _DepthBias, 1));

                o.uv = v.uv;
                o.uva = TRANSFORM_TEX(v.uv, _ATexture);
                o.uvb = TRANSFORM_TEX(v.uv, _BTexture);
                o.uvc = TRANSFORM_TEX(v.uv, _CTexture);
                o.uvd = TRANSFORM_TEX(v.uv, _DTexture);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            
            float mask(float2 uv, float4 mask)
            {
                return (1-step(uv.x, mask.x)) * step(uv.x, mask.x+mask.z)
                     * (1-step(uv.y, mask.y))* step(uv.y, mask.y+mask.w);
            }

            fixed4 blend(fixed4 bg, fixed4 fg)
            {
                return bg * (1.0 - fg.a) + fg * fg.a;
            }

            fixed4 frag(v2f i) : SV_Target
            {

                // sample the texture
                fixed4 bg = tex2D(_ATexture, i.uva);
                fixed4 aCol = mask(i.uv, _AMask) * tex2D(_ATexture, i.uva);
                fixed4 bCol = mask(i.uv, _BMask) * tex2D(_BTexture, i.uvb);
                fixed4 cCol = mask(i.uv, _CMask) * tex2D(_CTexture, i.uvc);
                fixed4 dCol = mask(i.uv, _DMask) * tex2D(_DTexture, i.uvd);

                fixed4 col = _DefaultColor;
                col = blend(col, aCol);
                col = blend(col, bCol);
                col = blend(col, cCol);
                col = blend(col, dCol);
                
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
