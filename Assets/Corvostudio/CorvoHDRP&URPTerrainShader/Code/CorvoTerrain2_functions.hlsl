//URP&HDRP Terrain Shader
//Marco Amadei (Corvostudio di Amadei Marco) 
//Software is protected by EU licensing

void ValueCombiner_float(float4 splat0,float4 splat1,float4 splat2,float4 splat3,float4 splat_control, out float4 Out)
{
    float4 col;
    col  = splat_control.r * splat0;
    col += splat_control.g * splat1;
    col += splat_control.b * splat2;
    col += splat_control.a * splat3;

    Out = col;
}
void ValueCombinerNormal_float(float4 splat0,float4 splat1,float4 splat2,float4 splat3,float4 splat_control, out float4 Out)
{
    Out  = splat_control.r * splat0;
    Out += splat_control.g * splat1;
    Out += splat_control.b * splat2;
    Out += splat_control.a * splat3;
    //Out.r = -Out.r;//<--this one inverted is similar ground color normal influence, but still it's WRONG
    Out.g = -Out.g;//<--this one inverted is similar light direction
    //Out.b = -Out.b;
}

void NormalUnpack_float(float4 normal_tex, out float3 Out)
{
    Out = UnpackNormal(normal_tex);
}

void UnpackNormalFromArray_float(float4 normal_tex, out float3 Out)
{
    Out = UnpackNormal(normal_tex)*2-1;
}


float2 ParallaxMapping(float2 texCoords, float4 depthMap, float3 viewDir, float numLayers, float Aplitude)
{ 
    float layerDepth = 1.0f / numLayers;
    // depth of current layer
    float currentLayerDepth = 0.0f;
    // the amount to shift the texture coordinates per layer (from vector P)

    viewDir=normalize(viewDir);
    float2 P = viewDir.xy * Aplitude; 
    float2 deltaTexCoords = P / numLayers;

    // get initial values
    float2 currentTexCoords = texCoords;
    float currentDepthMapValue = depthMap.r;
    
    while(currentLayerDepth < currentDepthMapValue)
    {
        // shift texture coordinates along direction of P
        currentTexCoords -= deltaTexCoords;
        // get depthmap value at current texture coordinates
        currentDepthMapValue = depthMap.r;  
        // get depth of next layer
        currentLayerDepth += layerDepth;  
    }
    return currentTexCoords;    
}

void GetParallaxUVsLayered_float(float4 Heightmap,float2 UVs, float3 ViewDirTangent,float Amplitude,float NumLayers, out float2 Out)
{
    Out = ParallaxMapping(UVs, Heightmap, ViewDirTangent, NumLayers, Amplitude);
}

void GetParallaxUVs_float(half Heightmap, float2 UVs, half3 ViewDirTangent, half Amplitude, out float2 Out)
{
    Heightmap = Heightmap * Amplitude - Amplitude / 2.0;
    float3 v = normalize(ViewDirTangent);
    v.z += 0.42;
    Out = UVs + Heightmap * (v.xy / v.z);
}

/*void SumUVsToMatrixUVs_float(float4x4 MatrixUVs,float2 SumUVs, out float4x4 Out)
{
    Out = float4x4( MatrixUVs[0].r+SumUVs.x, MatrixUVs[0].g+SumUVs.y, MatrixUVs[0].b+SumUVs.x, MatrixUVs[0].a+SumUVs.y,
                    MatrixUVs[1].r+SumUVs.x, MatrixUVs[1].g+SumUVs.y, MatrixUVs[1].b+SumUVs.x, MatrixUVs[1].a+SumUVs.y,
                    MatrixUVs[2].r+SumUVs.x, MatrixUVs[2].g+SumUVs.y, MatrixUVs[2].b+SumUVs.x, MatrixUVs[2].a+SumUVs.y,
                    MatrixUVs[3].r+SumUVs.x, MatrixUVs[3].g+SumUVs.y, MatrixUVs[3].b+SumUVs.x, MatrixUVs[3].a+SumUVs.y);
                    
    //Out = float4x4( MatrixUVs[0].r+SumUVs.x, MatrixUVs[1].r+SumUVs.y, MatrixUVs[2].r+SumUVs.x, MatrixUVs[3].r+SumUVs.y,
    //                MatrixUVs[0].g+SumUVs.x, MatrixUVs[1].g+SumUVs.y, MatrixUVs[2].g+SumUVs.x, MatrixUVs[3].g+SumUVs.y,
    //                MatrixUVs[0].b+SumUVs.x, MatrixUVs[1].b+SumUVs.y, MatrixUVs[2].b+SumUVs.x, MatrixUVs[3].b+SumUVs.y,
    //                MatrixUVs[0].a+SumUVs.x, MatrixUVs[1].a+SumUVs.y, MatrixUVs[2].a+SumUVs.x, MatrixUVs[3].a+SumUVs.y);
}*/