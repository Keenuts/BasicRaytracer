using UnityEngine;
using UnityEditor;

namespace Raytrace
{
    [CustomEditor(typeof(SceneObjectHost))]
    public class SceneObjectHostEditor : Editor
    {
        SerializedProperty sceneObject;

        void OnEnable()
        {
            sceneObject = serializedObject.FindProperty("sceneObject");
            ((SceneObjectHost)target).Setup();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(sceneObject);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
