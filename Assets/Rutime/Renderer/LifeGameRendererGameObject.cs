using UnityEngine;

namespace LifeGame3D
{
    /// <summary>
    /// GameObjectで描画する
    /// </summary>
    public class LifeGameRendererGameObject : ILifeGameRenderer
    {
        private Mesh _mesh;
        private Material _material;
        private float _scale;
        private readonly Matrix4x4 _localToWorld;

        public LifeGameRendererGameObject(Mesh mesh, Material material, Matrix4x4 localToWorld)
        {
            _mesh = mesh;
            _material = material;
            _localToWorld = localToWorld;
        }

        public void AddRenderBuffer(DataChunk input, int height)
        {
            for (int i = 0; i < input.Length; i++)
            {
                if (!input.IsFlag(i, LifeGameFlags.Alive)) continue;
                
                input.GetIndex(i, out int x, out _, out int z);
                var position = new Vector3(x, height, z);
                
                var g = new GameObject();
                var meshFilter = g.AddComponent<MeshFilter>();
                var meshRenderer = g.AddComponent<MeshRenderer>();
                meshRenderer.material = _material;
                meshFilter.mesh = _mesh;
                
                g.transform.position = _localToWorld.MultiplyPoint(position);
                g.transform.localScale = _localToWorld.lossyScale;

            }
        }

        public void RenderInstance(Bounds bounds)
        {
        }

        public void Dispose()
        {
        }
    }
}
