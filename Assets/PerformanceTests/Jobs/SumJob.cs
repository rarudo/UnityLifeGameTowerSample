using System.Threading;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace LifeGame3D.ExampleCode.Jobs
{
    [BurstCompile]
    public struct SumJob : IJob
    {
        [ReadOnly]
        public NativeArray<LifeGameStruct> input;
        
        public NativeReference<float3> result;
        
        public void Execute()
        {
            foreach (var lifeGameStruct in input)
            {
                result.Value += lifeGameStruct.position;
            }
        }
    }
}
