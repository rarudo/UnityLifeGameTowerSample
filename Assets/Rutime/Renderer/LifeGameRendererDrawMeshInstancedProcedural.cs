using System.Runtime.InteropServices;
using LifeGame3D.Job;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace LifeGame3D
{
    /// <summary>
    /// DrawMeshInstancedProceduralで描画する
    /// </summary>
    public class LifeGameRendererInstancedProcedural : ILifeGameRenderer
    {
        private readonly Mesh _mesh;
        private readonly Material _material;

        private readonly MaterialPropertyBlock _mpb;
        private GraphicsBuffer _gb;

        private readonly Vector3 _size;
        private NativeList<uint> _renderBuffers;
        
        private static readonly int LifeGameElementBufferID = Shader.PropertyToID("_LifeGameBuffer");
        private static readonly int SizeID = Shader.PropertyToID("_Size");
        private static readonly int LocalToWorldID = Shader.PropertyToID("_LocalToWorld");
        private readonly Matrix4x4 _localToWorld;

        public LifeGameRendererInstancedProcedural(Mesh mesh, Material material, Vector3Int size, Matrix4x4 localToWorld)
        {
            _mesh = mesh;
            _material = material;
            _renderBuffers = new NativeList<uint>(Allocator.Persistent);
            _size = size;
            _localToWorld = localToWorld;
        }

        public void AddRenderBuffer(DataChunk input, int height)
        {
            _gb?.Dispose();

            var maxCount = _renderBuffers.Length + input.Length;
            if (_renderBuffers.Capacity - maxCount < 0)
            {
                _renderBuffers.SetCapacity(maxCount * 2);
            }

            var handle = new LifeGameConvertJob()
            {
                input = input,
                addictive = (uint)(_size.x * _size.z * height),
                output = _renderBuffers.AsParallelWriter(),
            }.Schedule(input.Length, 32);
            handle.Complete();

            if (_renderBuffers.Length == 0) return;

            _gb = new GraphicsBuffer(GraphicsBuffer.Target.Structured, _renderBuffers.Length,
                Marshal.SizeOf<uint>());
            _gb.SetData(_renderBuffers.AsArray());
            _material.SetBuffer(LifeGameElementBufferID, _gb);
            _material.SetVector(SizeID, _size);
            _material.SetMatrix(LocalToWorldID, _localToWorld);
        }

        public void RenderInstance(Bounds bounds)
        {
            if (_gb == null || _gb.IsValid() == false) return;

            Graphics.DrawMeshInstancedProcedural(_mesh, 0, _material, bounds, _gb.count);
        }


        public void Dispose()
        {
            _gb?.Dispose();
            _renderBuffers.Dispose();
        }
    }
}
