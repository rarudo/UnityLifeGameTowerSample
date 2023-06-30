using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace LifeGame3D
{
    /// <summary>
    /// BatchRendererGroupを使ってレンダリングする
    /// </summary>
    public class LifeGameRendererBatchRendererGroup : ILifeGameRenderer
    {
        private readonly BatchRendererGroupSimple _brgs;
        private LifeGameUtil.LifeGameMesh _lifeGameMesh;

        public LifeGameRendererBatchRendererGroup(Mesh mesh, Material material)
        {
            _brgs = new BatchRendererGroupSimple(material);

            var vertices = new NativeArray<float3>(mesh.vertices.Length, Allocator.Persistent);
            for (var i = 0; i < mesh.vertices.Length; i++)
            {
                vertices[i] = mesh.vertices[i];
            }

            var triangles = new NativeArray<int>(mesh.triangles.Length, Allocator.Persistent);
            for (var i = 0; i < mesh.triangles.Length; i++)
            {
                triangles[i] = mesh.triangles[i];
            }

            var uv = new NativeArray<float2>(mesh.uv.Length, Allocator.Persistent);
            for (var i = 0; i < mesh.uv.Length; i++)
            {
                uv[i] = mesh.uv[i];
            }

            var normals = new NativeArray<float3>(mesh.uv.Length, Allocator.Persistent);
            for (var i = 0; i < mesh.normals.Length; i++)
            {
                normals[i] = mesh.normals[i];
            }

            _lifeGameMesh = new LifeGameUtil.LifeGameMesh()
            {
                vertices = vertices,
                triangles = triangles,
                uv = uv,
                normals = normals
            };
        }


        public void AddRenderBuffer(DataChunk input, int height)
        {
            var mesh = LifeGameUtil.CreateMesh(ref input, ref _lifeGameMesh, height);
            _brgs.AddMesh(mesh);
        }


        public void RenderInstance(Bounds bounds)
        {
        }

        public void Dispose()
        {
            _brgs.Dispose();
            _lifeGameMesh.vertices.Dispose();
            _lifeGameMesh.triangles.Dispose();
            _lifeGameMesh.uv.Dispose();
            _lifeGameMesh.normals.Dispose();
        }
    }
}
