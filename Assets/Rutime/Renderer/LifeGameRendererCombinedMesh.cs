using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace LifeGame3D
{
    public interface ILifeGameRenderer : IDisposable
    {
        void AddRenderBuffer(DataChunk input, int height);
        void RenderInstance(Bounds bounds);
    }

    /// <summary>
    /// メッシュを結合して描画する
    /// </summary>
    public class LifeGameRendererCombinedMesh : ILifeGameRenderer
    {
        private readonly MaterialPropertyBlock _mpb;
        private GraphicsBuffer _gb;

        private NativeList<uint> _renderBuffers;

        private readonly uint[] _indirectArgs = new uint[]
        {
            0u, 0u, 0u, 0u, 0u
        };
        private readonly GraphicsBuffer _argsBuffer;
        private readonly List<Mesh> _meshes = new List<Mesh>();
        private readonly RenderParams _renderParams;

        private LifeGameUtil.LifeGameMesh _lifeGameMesh;
        private readonly Material _material;
        private readonly Matrix4x4 _localToWorld;


        public LifeGameRendererCombinedMesh(Mesh mesh, Material material, Matrix4x4 localToWorld)
        {
            _material = material;
            _renderBuffers = new NativeList<uint>(Allocator.Persistent);
            _argsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, _indirectArgs.Length,
                sizeof(uint) * _indirectArgs.Length);
            _renderParams = new RenderParams(material);
            _renderParams.shadowCastingMode = ShadowCastingMode.On;
            _renderParams.reflectionProbeUsage = ReflectionProbeUsage.BlendProbesAndSkybox;
            _localToWorld = localToWorld;

            var vertices = new NativeArray<float3>(mesh.vertices.Length, Allocator.Persistent);
            for (var i = 0; i < mesh.vertices.Length; i++)
            {
                vertices[i] = mesh.vertices[i] + new Vector3(0.5f, 0.5f,0.5f);
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
            _meshes.Add(mesh);
        }

        public void RenderInstance(Bounds bounds)
        {
            foreach (var mesh in _meshes)
            {
                // Graphics.DrawMesh(mesh, _localToWorld * Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one), _material,
                //     LayerMask.NameToLayer("Default"));
                
                Graphics.RenderMesh(_renderParams, mesh, 0, _localToWorld * Matrix4x4.identity);
            }
        }

        public void Dispose()
        {
            _gb?.Dispose();
            _argsBuffer?.Dispose();
            _renderBuffers.Dispose();
            _lifeGameMesh.vertices.Dispose();
            _lifeGameMesh.triangles.Dispose();
            _lifeGameMesh.uv.Dispose();
            _lifeGameMesh.normals.Dispose();
        }
    }
}
