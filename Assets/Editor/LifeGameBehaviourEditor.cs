using LifeGame3D;
using UnityEditor;

namespace _20220912_DrawMesh.LifeGame.LifeGameEditor
{
    [CustomEditor(typeof(LifeGameBehaviour))]
    public class LifeGameBehaviourEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var behaviour = target as LifeGameBehaviour;
            var width = behaviour.initialWidth;
            
            if (behaviour.initialData.Length != behaviour.initialWidth * behaviour.initialWidth)
            {
                behaviour.initialData = new bool[behaviour.initialWidth * behaviour.initialWidth];
            }

            for (int y = 0; y < width; y++)
            {
                EditorGUILayout.BeginHorizontal();
                for (int x = 0; x < width; x++)
                {
                    var index = x + y * width;
                    behaviour.initialData[index] = EditorGUILayout.Toggle(behaviour.initialData[index]);
                }

                EditorGUILayout.EndHorizontal();
            }
        }
    }
}
