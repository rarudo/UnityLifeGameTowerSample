using LifeGame3D.Job;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace LifeGame3D
{
    public class LifeGameUtil
    {
        public struct LifeGameMesh 
        {
            public NativeArray<float3> vertices;
            public NativeArray<int> triangles;
            public NativeArray<float2> uv;
            public NativeArray<float3> normals;
        }

        public struct VerticesData
        {
            public float3 positions;
            public float3 normals;
            public float2 uv;
        }

        public static Mesh CreateMesh(
            ref DataChunk input, 
            ref LifeGameMesh lifeGameMesh,
            int height)
        {
            NativeList<VerticesData> verticesData = new NativeList<VerticesData>(Allocator.TempJob);
            NativeList<int> triangles = new NativeList<int>(Allocator.TempJob);
            
            // GreedyMeshでMeshを結合
            var handle = new GreedyMeshJob()
            {
                input = input,
                height = height,
                referenceMesh = lifeGameMesh,
                verticesData = verticesData,
                triangles = triangles
            }.Schedule();
            handle.Complete();

            var mesh = new Mesh
            {
                hideFlags = HideFlags.DontSave 
            };

            // できる限りGCを発生させないようにメッシュを構成する
            mesh.SetVertexBufferParams(verticesData.Length,
                new VertexAttributeDescriptor(VertexAttribute.Position),
                new VertexAttributeDescriptor(VertexAttribute.Normal),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2));
            mesh.SetVertexBufferData(verticesData.AsArray(), 0, 0, verticesData.Length);
            mesh.SetIndexBufferParams(triangles.Length, IndexFormat.UInt32);
            mesh.SetIndexBufferData(triangles.AsArray(), 0, 0, triangles.Length, MeshUpdateFlags.DontValidateIndices);
            
            SubMeshDescriptor desc = new SubMeshDescriptor(0, triangles.Length);
            mesh.SetSubMesh(0, desc, MeshUpdateFlags.DontValidateIndices);
            mesh.RecalculateBounds();
            
            triangles.Dispose();
            verticesData.Dispose();
            return mesh;
        }
    }
}
