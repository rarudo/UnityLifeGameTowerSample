using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace LifeGame3D.Job
{
    [BurstCompile]
    public struct LifeGameConvertJob : IJobParallelFor
    {
        public DataChunk input;

        [WriteOnly] public NativeList<uint>.ParallelWriter output;

        [ReadOnly] public uint addictive;

        public void Execute(int index)
        {
            if (input.IsFlag(index, LifeGameFlags.Alive))
            {
                output.AddNoResize((uint)index + addictive);
            }
        }
    }
}
