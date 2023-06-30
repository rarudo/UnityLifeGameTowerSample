using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace LifeGame3D.ExampleCode.Jobs
{
    [BurstCompile]
    public unsafe struct CreateJob : IJobParallelFor
    {
        [WriteOnly] public NativeArray<LifeGameStruct> output;

        public static readonly Vector3 Addictive = Vector3.one;

        public void Execute(int index)
        {
            var ptr = (LifeGameStruct*)output.GetUnsafePtr();
            ptr[index].position = Addictive;
        }
    }
}
