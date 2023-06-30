using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine.Rendering;

namespace LifeGame3D.Job
{
    [BurstCompile]
    public unsafe struct DrawCommandsJob : IJobParallelFor
    {
        [ReadOnly] public NativeList<BatchMeshID> m_MeshIDs;
        public BatchMaterialID m_MaterialID;
        public BatchID m_BatchID;
        public uint kNumInstances;

        [NativeDisableUnsafePtrRestriction] 
        public BatchCullingOutputDrawCommands* drawCommands;

        public void Execute(int index)
        {
            drawCommands->drawCommands[index].visibleOffset = 0;
            drawCommands->drawCommands[index].visibleCount = kNumInstances;
            drawCommands->drawCommands[index].batchID = m_BatchID;
            drawCommands->drawCommands[index].materialID = m_MaterialID;
            drawCommands->drawCommands[index].meshID = m_MeshIDs[index];
            drawCommands->drawCommands[index].submeshIndex = 0;
            drawCommands->drawCommands[index].splitVisibilityMask = 0xff;
            drawCommands->drawCommands[index].flags = 0;
            drawCommands->drawCommands[index].sortingPosition = 0;
        }
    }
}
