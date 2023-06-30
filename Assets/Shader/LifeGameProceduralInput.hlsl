#ifndef LIFE_GAME_PROCEDURAL_INPUT
#define LIFE_GAME_PROCEDURAL_INPUT

#include "Packages/com.unity.render-pipelines.universal/Shaders/SimpleLitInput.hlsl"

StructuredBuffer<uint> _LifeGameBuffer;

float4 _Size;
float4x4 _LocalToWorld;

float3 GetPosition(float3 positionOS, uint instanceID)
{
    const uint index = _LifeGameBuffer[instanceID];
    const int sizeX = _Size.x;
    const int sizeY = _Size.y;
    const int sizeZ = _Size.z;

    int x = index % sizeX;
    int z = index / sizeX % sizeZ;
    int y = index / sizeX / sizeZ % sizeY;
    
    float3 offsetOS = float3(x, y, z);

    float3 positionWS = mul(_LocalToWorld, positionOS + offsetOS);
    positionWS.x += _LocalToWorld[0][3];
    positionWS.y += _LocalToWorld[1][3];
    positionWS.z += _LocalToWorld[2][3];

    return positionWS;
}

#endif
