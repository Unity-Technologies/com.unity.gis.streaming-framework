//UNITY_SHADER_NO_UPGRADE
#ifndef TERRAIN_MASK_NODE
#define TERRAIN_MASK_NODE

float mask(float2 uv, float4 mask)
{
    return (1-step(uv.x, mask.x)) * step(uv.x, mask.x+mask.z)
            * (1-step(uv.y, mask.y))* step(uv.y, mask.y+mask.w);
}


float4 blend(float4 bg, float4 fg)
{
    return bg * (1.0 - fg.a) + fg * fg.a;
}




void ComputeFragment_float(float2 uv, float4 a, float4 aMask, float4 b, float4 bMask, float4 c, float4 cMask, float4 d, float4 dMask, out float4 result)
{
    float4 aCol = mask(uv, aMask) * a;
    float4 bCol = mask(uv, bMask) * b;
    float4 cCol = mask(uv, cMask) * c;
    float4 dCol = mask(uv, dMask) * d;

    float4 col = float4(0,0,0,0);
    col = blend(col, aCol);
    col = blend(col, bCol);
    col = blend(col, cCol);
    col = blend(col, dCol);
    
    result = col;
}

#endif