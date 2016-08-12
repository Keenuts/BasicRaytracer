using UnityEngine;
using UnityEditor;

namespace Raytrace
{
    [CustomPropertyDrawer(typeof(Scene))]
    public class SceneDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.indentLevel = 0;
            GUILayout.Label("Environment");
            EditorGUI.indentLevel = 1;
            EditorGUILayout.PropertyField(property.FindPropertyRelative("camera"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("focal"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("closePlane"));
            GUILayout.Space(5);
            EditorGUILayout.PropertyField(property.FindPropertyRelative("light"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("ambiant"));

            GUILayout.Space(10);
            EditorGUI.indentLevel = 0;
            GUILayout.Label("Scene Objects");
            EditorGUI.indentLevel = 1;
        }
    }
}