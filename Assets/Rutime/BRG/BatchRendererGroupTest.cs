using UnityEngine;

namespace LifeGame3D
{
    public class BatchRendererGroupTest : MonoBehaviour
    {
        public Mesh[] meshes;
        public Material material;
        private BatchRendererGroupSimple _brgs;

        public void Start()
        {
            _brgs = new BatchRendererGroupSimple(material);
            foreach (var mesh in meshes)
            {
                _brgs.AddMesh(mesh);
            }
        }

        private void OnDisable()
        {
            _brgs.Dispose();
        }
    }
}
