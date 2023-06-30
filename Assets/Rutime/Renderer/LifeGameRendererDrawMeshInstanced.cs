using System.Collections.Generic;
using UnityEngine;

namespace LifeGame3D
{
    /// <summary>
    /// DrawMeshInstancedで描画する
    /// </summary>
    public class LifeGameRendererDrawMeshInstanced : ILifeGameRenderer
    {
        private Mesh _mesh;
        private Material _material;

        private readonly List<List<Matrix4x4>> _matrix4X4S = new List<List<Matrix4x4>>();
        private readonly Matrix4x4 _localToWorld;

        public LifeGameRendererDrawMeshInstanced(Mesh mesh, Material material, Matrix4x4 localToWorld)
        {
            _mesh = mesh;
            _material = material;
            _localToWorld = localToWorld;
            _matrix4X4S.Add(new List<Matrix4x4>());
        }

        public void AddRenderBuffer(DataChunk input, int height)
        {
            var last = _matrix4X4S[^1];
            
            // 1024個以内になるように調整
            List<Matrix4x4> target;
            if (last.Count + input.Length < 1024)
            {
                target = last;
            }
            else
            {
                target = new List<Matrix4x4>(1024);
                _matrix4X4S.Add(target);
            }
            
            for (int i = 0; i < input.Length; i++)
            {
                if(!input.IsFlag(i, LifeGameFlags.Alive)) continue;
                
                input.GetIndex(i, out int x, out _, out int z);
                var position = new Vector3(x, height, z);
                target.Add(_localToWorld * Matrix4x4.TRS(position, Quaternion.identity, Vector3.one));
            }
        }

        public void RenderInstance(Bounds bounds)
        {
            foreach (var matrix4X4List in _matrix4X4S)
            {
                Graphics.DrawMeshInstanced(_mesh, 0, _material, matrix4X4List);
            }
        }

        public void Dispose()
        {
        }

        
    }
}
