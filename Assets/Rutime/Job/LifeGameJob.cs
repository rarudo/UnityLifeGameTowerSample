using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace LifeGame3D.Job
{
    /// <summary>
    /// ライフゲームのロジック
    /// </summary>
    [BurstCompile]
    public struct LifeGameJob : IJobParallelFor
    {
        [ReadOnly, NativeDisableContainerSafetyRestriction]
        public DataChunk input;
        
        [WriteOnly, NativeDisableContainerSafetyRestriction]
        public DataChunk output;

        public void Execute(int i)
        {
            input.GetIndex(i, out int x, out _, out int z);
            
            if (x - 1 < 0 || z - 1 < 0 ||
                x + 1 >= input.xLength || z + 1 >= input.zLength) return;
            
            int sum = 0;
            // 周囲8ピクセルの探索
            for (int xx = -1; xx <= 1; xx++)
            {
                for (int zz = -1; zz <= 1; zz++)
                {
                    if (xx == 0 && zz == 0)
                    {
                        continue;
                    }

                    var offsetX = x + xx;
                    var offsetZ = z + zz;

                    if (input.IsFlag(offsetX, 0, offsetZ, LifeGameFlags.Alive))
                    {
                        sum++;
                    }
                }
            }

            var pastArrival = input.IsFlag(i, LifeGameFlags.Alive);

            if (pastArrival)
            {
                if (sum == 2 || sum == 3)
                {
                    output.AddFlag( i, LifeGameFlags.Alive);
                }
            }
            else 
            {
                if (sum == 3)
                {
                    output.AddFlag( i, LifeGameFlags.Alive);
                }
            }
        }

        public void Execute()
        {
            for (int i = 0; i < input.Length; i++)
            {
                Execute(i);
            }
        }
    }
}
