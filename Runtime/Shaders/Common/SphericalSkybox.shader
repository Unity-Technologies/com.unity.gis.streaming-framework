Shader "Geospatial/URP/SphericalSkybox"
{
    Properties
    {
        _UpperSkyColor("Upper Sky Color", Color) = (0,0,1,1)
        _LowerSkyColor("Lower Sky Color", Color) = (0,0,1,1)
        [HDR] _HorizonColor("Horizon Color", Color) = (1,1,1,1)
        _GroundColor("Ground Color", Color) = (0,0,0,1)
        _GroundTransition("Ground Transition", Float) = 1.0
        _AtmosphereThickness("Atmosphere Thickness", Float) = 1.0
        _SkyGradient("Sky Gradient", Float) = 1.0
        _PlanetRadius("Planet Radius", Float) = 1000.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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
                float4 vertex : SV_POSITION;
                float4 position : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _UpperSkyColor;
            float4 _LowerSkyColor;
            float4 _HorizonColor;
            float4 _GroundColor;
            float _PlanetRadius;
            float _AtmosphereThickness;
            float _GroundTransition;
            float _SkyGradient;
            float3 _EcefCenter;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.position = mul(unity_ObjectToWorld, v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float linstep(float min, float max, float value)
            {
                return clamp((value - min) / (max - min),0,1);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 ecefPosition = _WorldSpaceCameraPos - _EcefCenter;
                float sinAngle = _PlanetRadius / length(ecefPosition);
                float cosAngle = sqrt(1 - sinAngle * sinAngle);
                float3 down = normalize(-ecefPosition);
                float3 ray = normalize(-i.position.xyz);
                float dotProduct = dot(down, ray);
                float c1 = linstep(cosAngle, cosAngle - 0.1 * _AtmosphereThickness * (1 - cosAngle), -dotProduct);
                float c2 = linstep(cosAngle, -0.7, -dotProduct);
                float c3 = smoothstep(cosAngle + 0.01 * _GroundTransition * (1 - cosAngle), cosAngle, -dotProduct);
                c1 = 1 - c1;
                c1 *= c1 * c1;
                c1 = 1 - c1;
                //float c = smoothstep(cosAngle, cosAngle - 0.1*_AtmosphereThickness*(1-cosAngle), -dotProduct);
                //float c = step(cosAngle, -dotProduct);

                fixed4 col = c2 * _UpperSkyColor + (1 - c2) * _LowerSkyColor;
                col = c1 * col + (1 - c1) * _HorizonColor;//tex2D(_MainTex, i.uv);
                col = c3 * col + (1 - c3) * _GroundColor;

                return col;
            }
            ENDCG
        }
    }
}
