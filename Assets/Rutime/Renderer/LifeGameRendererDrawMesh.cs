using System.Collections.Generic;
using UnityEngine;

namespace LifeGame3D
{
    /// <summary>
    /// DrawMeshで描画する
    /// </summary>
    public class LifeGameRendererDrawMesh : ILifeGameRenderer
    {
        private readonly Mesh _mesh;
        private readonly Material _material;

        private readonly List<Matrix4x4> _matrix4X4S = new List<Matrix4x4>();
        private readonly Matrix4x4 _localToWorld;

        public LifeGameRendererDrawMesh(Mesh mesh, Material material, Matrix4x4 localToWorld)
        {
            _mesh = mesh;
            _material = material;
            _localToWorld = localToWorld;
        }

        public void AddRenderBuffer(DataChunk input, int height)
        {
            for (int i = 0; i < input.Length ; i++)
            {
                if(!input.IsFlag(i, LifeGameFlags.Alive)) continue;
                input.GetIndex(i, out int x, out _, out int z);
                var position = new Vector3(x, height, z);
                var matrix4X4 = _localToWorld * Matrix4x4.TRS(position, Quaternion.identity, Vector3.one);
                _matrix4X4S.Add(matrix4X4);
            }
        }

        public void RenderInstance(Bounds bounds)
        {
            foreach (var matrix4X4 in _matrix4X4S)
            {
                Graphics.DrawMesh(_mesh, matrix4X4, _material, 0);
            }
        }

        public void Dispose()
        {
        }

        
    }
}
