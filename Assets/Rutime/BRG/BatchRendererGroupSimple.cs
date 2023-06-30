using System;
using LifeGame3D.Job;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace LifeGame3D
{
    public class BatchRendererGroupSimple : IDisposable
    {
        // public Mesh mesh;
        // public Material material;

        private BatchRendererGroup m_BRG;

        private GraphicsBuffer m_InstanceData;
        private BatchID m_BatchID;
        private NativeList<BatchMeshID> m_MeshIDs;
        private BatchMaterialID m_MaterialID;
        private int MeshCount => m_MeshIDs.Length;

        // Some helper constants to make calculations more convenient.
        private const int kSizeOfMatrix = sizeof(float) * 4 * 4;
        private const int kSizeOfPackedMatrix = sizeof(float) * 4 * 3;
        private const int kSizeOfFloat4 = sizeof(float) * 4;
        private const int kBytesPerInstance = (kSizeOfPackedMatrix * 2) + kSizeOfFloat4;
        private const int kExtraBytes = kSizeOfMatrix * 2;
        private const int kNumInstances = 1;

        // The PackedMatrix is a convenience type that converts matrices into
        // the format that Unity-provided SRP shaders expect.
        struct PackedMatrix
        {
            public float c0x;
            public float c0y;
            public float c0z;
            public float c1x;
            public float c1y;
            public float c1z;
            public float c2x;
            public float c2y;
            public float c2z;
            public float c3x;
            public float c3y;
            public float c3z;

            public PackedMatrix(Matrix4x4 m)
            {
                c0x = m.m00;
                c0y = m.m10;
                c0z = m.m20;
                c1x = m.m01;
                c1y = m.m11;
                c1z = m.m21;
                c2x = m.m02;
                c2y = m.m12;
                c2z = m.m22;
                c3x = m.m03;
                c3y = m.m13;
                c3z = m.m23;
            }
        }

        public BatchRendererGroupSimple(Material material)
        {
            m_BRG = new BatchRendererGroup(this.OnPerformCulling, IntPtr.Zero);
            m_MeshIDs = new NativeList<BatchMeshID>(Allocator.Persistent);
            m_MaterialID = m_BRG.RegisterMaterial(material);

            AllocateInstanceDateBuffer();
            PopulateInstanceDataBuffer();
        }

        public void AddMesh(Mesh m)
        {
            m_MeshIDs.Add(m_BRG.RegisterMesh(m));
        }

        public void SetGlobalBounds(Bounds bounds)
        {
            m_BRG.SetGlobalBounds(bounds);
        }
        
        private void AllocateInstanceDateBuffer()
        {
            m_InstanceData = new GraphicsBuffer(GraphicsBuffer.Target.Raw,
                BufferCountForInstances(kBytesPerInstance, kNumInstances, kExtraBytes),
                sizeof(int));
        }

        private void PopulateInstanceDataBuffer()
        {
            // Place a zero matrix at the start of the instance data buffer, so loads from address 0 return zero.
            var zero = new Matrix4x4[1] { Matrix4x4.zero };

            // Create transform matrices for three example instances.
            var matrices = new Matrix4x4[kNumInstances]
            {
                Matrix4x4.Translate(new Vector3(0, 0, 0)),
            };

            // Convert the transform matrices into the packed format that the shader expects.
            var objectToWorld = new PackedMatrix[kNumInstances]
            {
                new PackedMatrix(matrices[0]),
            };

            // Also create packed inverse matrices.
            var worldToObject = new PackedMatrix[kNumInstances]
            {
                new PackedMatrix(matrices[0].inverse),
            };

            // Make all instances have unique colors.
            var colors = new Vector4[kNumInstances]
            {
                new Vector4(1, 0, 0, 1),
            };

            // In this simple example, the instance data is placed into the buffer like this:
            // Offset | Description
            //      0 | 64 bytes of zeroes, so loads from address 0 return zeroes
            //     64 | 32 uninitialized bytes to make working with SetData easier, otherwise unnecessary
            //     96 | unity_ObjectToWorld, three packed float3x4 matrices
            //    240 | unity_WorldToObject, three packed float3x4 matrices
            //    384 | _BaseColor, three float4s

            // Calculates start addresses for the different instanced properties. unity_ObjectToWorld starts
            // at address 96 instead of 64, because the computeBufferStartIndex parameter of SetData
            // is expressed as source array elements, so it is easier to work in multiples of sizeof(PackedMatrix).
            uint byteAddressObjectToWorld = kSizeOfPackedMatrix * 2;
            uint byteAddressWorldToObject = byteAddressObjectToWorld + kSizeOfPackedMatrix * kNumInstances;
            uint byteAddressColor = byteAddressWorldToObject + kSizeOfPackedMatrix * kNumInstances;

            // Upload the instance data to the GraphicsBuffer so the shader can load them.
            m_InstanceData.SetData(zero, 0, 0, 1);
            m_InstanceData.SetData(objectToWorld, 0, (int)(byteAddressObjectToWorld / kSizeOfPackedMatrix),
                objectToWorld.Length);
            m_InstanceData.SetData(worldToObject, 0, (int)(byteAddressWorldToObject / kSizeOfPackedMatrix),
                worldToObject.Length);
            // m_InstanceData.SetData(colors, 0, (int)(byteAddressColor / kSizeOfFloat4), colors.Length);

            // Set up metadata values to point to the instance data. Set the most significant bit 0x80000000 in each
            // which instructs the shader that the data is an array with one value per instance, indexed by the instance index.
            // Any metadata values that the shader uses that are not set here will be 0. When a value of 0 is used with
            // UNITY_ACCESS_DOTS_INSTANCED_PROP (i.e. without a default), the shader interprets the
            // 0x00000000 metadata value and loads from the start of the buffer. The start of the buffer is
            // a zero matrix so this sort of load is guaranteed to return zero, which is a reasonable default value.
            var metadata = new NativeArray<MetadataValue>(3, Allocator.Temp);
            metadata[0] = new MetadataValue
                { NameID = Shader.PropertyToID("unity_ObjectToWorld"), Value = 0x80000000 | byteAddressObjectToWorld, };
            metadata[1] = new MetadataValue
                { NameID = Shader.PropertyToID("unity_WorldToObject"), Value = 0x80000000 | byteAddressWorldToObject, };
            // metadata[2] = new MetadataValue
            //     { NameID = Shader.PropertyToID("_BaseColor"), Value = 0x80000000 | byteAddressColor, };

            // Finally, create a batch for the instances and make the batch use the GraphicsBuffer with the
            // instance data as well as the metadata values that specify where the properties are.
            m_BatchID = m_BRG.AddBatch(metadata, m_InstanceData.bufferHandle);
        }

        // Raw buffers are allocated in ints. This is a utility method that calculates
        // the required number of ints for the data.
        int BufferCountForInstances(int bytesPerInstance, int numInstances, int extraBytes = 0)
        {
            // Round byte counts to int multiples
            bytesPerInstance = (bytesPerInstance + sizeof(int) - 1) / sizeof(int) * sizeof(int);
            extraBytes = (extraBytes + sizeof(int) - 1) / sizeof(int) * sizeof(int);
            int totalBytes = bytesPerInstance * numInstances + extraBytes;
            return totalBytes / sizeof(int);
        }

        public unsafe JobHandle OnPerformCulling(
            BatchRendererGroup rendererGroup,
            BatchCullingContext cullingContext,
            BatchCullingOutput cullingOutput,
            IntPtr userContext)
        {
            // UnsafeUtility.Malloc() requires an alignment, so use the largest integer type's alignment
            // which is a reasonable default.
            int alignment = UnsafeUtility.AlignOf<long>();

            // Acquire a pointer to the BatchCullingOutputDrawCommands struct so you can easily
            // modify it directly.
            var drawCommands = (BatchCullingOutputDrawCommands*)cullingOutput.drawCommands.GetUnsafePtr();

            // Allocate memory for the output arrays. In a more complicated implementation, you would calculate
            // the amount of memory to allocate dynamically based on what is visible.
            // This example assumes that all of the instances are visible and thus allocates
            // memory for each of them. The necessary allocations are as follows:
            // - a single draw command (which draws kNumInstances instances)
            // - a single draw range (which covers our single draw command)
            // - kNumInstances visible instance indices.
            // You must always allocate the arrays using Allocator.TempJob.
            drawCommands->drawCommands =
                (BatchDrawCommand*)UnsafeUtility.Malloc(UnsafeUtility.SizeOf<BatchDrawCommand>() * MeshCount, alignment,
                    Allocator.TempJob);
            drawCommands->drawRanges = (BatchDrawRange*)UnsafeUtility.Malloc(UnsafeUtility.SizeOf<BatchDrawRange>(),
                alignment, Allocator.TempJob);
            drawCommands->visibleInstances =
                (int*)UnsafeUtility.Malloc(kNumInstances * sizeof(int), alignment, Allocator.TempJob);
            drawCommands->drawCommandPickingInstanceIDs = null;

            drawCommands->drawCommandCount = MeshCount;
            drawCommands->drawRangeCount = 1;
            drawCommands->visibleInstanceCount = kNumInstances;

            // This example doens't use depth sorting, so it leaves instanceSortingPositions as null.
            drawCommands->instanceSortingPositions = null;
            drawCommands->instanceSortingPositionFloatCount = 0;

            // Configure the single draw command to draw kNumInstances instances
            // starting from offset 0 in the array, using the batch, material and mesh
            // IDs registered in the Start() method. It doesn't set any special flags.
            var drawCommandHandle = new DrawCommandsJob()
            {
                drawCommands = drawCommands,
                kNumInstances = kNumInstances,
                m_BatchID = m_BatchID,
                m_MaterialID = m_MaterialID,
                m_MeshIDs = m_MeshIDs
            }.Schedule(MeshCount, 32);
            
            
            // for (int i = 0; i < MeshCount; i++)
            // {
            //     drawCommands->drawCommands[i].visibleOffset = 0;
            //     drawCommands->drawCommands[i].visibleCount = kNumInstances;
            //     drawCommands->drawCommands[i].batchID = m_BatchID;
            //     drawCommands->drawCommands[i].materialID = m_MaterialID;
            //     drawCommands->drawCommands[i].meshID = m_MeshIDs[i];
            //     drawCommands->drawCommands[i].submeshIndex = 0;
            //     drawCommands->drawCommands[i].splitVisibilityMask = 0xff;
            //     drawCommands->drawCommands[i].flags = 0;
            //     drawCommands->drawCommands[i].sortingPosition = 0;
            // }

            // Configure the single draw range to cover the single draw command which
            // is at offset 0.
            var begin = (uint)math.max(0, 0);
            drawCommands->drawRanges[0].drawCommandsBegin = begin;
            drawCommands->drawRanges[0].drawCommandsCount = (uint)MeshCount - begin;

            // This example doesn't care about shadows or motion vectors, so it leaves everything
            // at the default zero values, except the renderingLayerMask which it sets to all ones
            // so Unity renders the instances regardless of mask settings.
            // drawCommands->drawRanges[0].filterSettings = new BatchFilterSettings { renderingLayerMask = 0xffffffff, };
            drawCommands->drawRanges[0].filterSettings = new BatchFilterSettings
            {
                renderingLayerMask = 1,
                layer = 0,
                motionMode = MotionVectorGenerationMode.ForceNoMotion,
                shadowCastingMode = ShadowCastingMode.On,
                receiveShadows = true,
                staticShadowCaster = false,
                allDepthSorted = false
            };

            // Finally, write the actual visible instance indices to the array. In a more complicated
            // implementation, this output would depend on what is visible, but this example
            // assumes that everything is visible.
            for (int i = 0; i < kNumInstances; ++i)
                drawCommands->visibleInstances[i] = i;

            // This simple example doesn't use jobs, so it returns an empty JobHandle.
            // Performance-sensitive applications are encouraged to use Burst jobs to implement
            // culling and draw command output. In this case, this function returns a
            // handle here that completes when the Burst jobs finish.
            return drawCommandHandle;
        }

        public void Dispose()
        {
            m_InstanceData.Dispose();
            m_MeshIDs.Dispose();
            m_BRG?.Dispose();
        }
    }
}
