using UnityEngine;
using UnityEditor;

namespace Raytrace
{
    [CustomPropertyDrawer(typeof(SceneObject))]
    public class SceneObjectDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUILayout.PropertyField(property.FindPropertyRelative("transform"));

            SerializedProperty geometry = property.FindPropertyRelative("geometry");
            EditorGUILayout.PropertyField(geometry);

            EditorGUI.indentLevel = 1;
            if (geometry.enumValueIndex == (int)Geometry.Sphere)
                EditorGUILayout.PropertyField(property.FindPropertyRelative("radius"));
            else
            {
                SerializedProperty vertex = property.FindPropertyRelative("vertex");
                SerializedProperty triangle = property.FindPropertyRelative("triangles");
                if(EditorGUILayout.PropertyField(vertex))
                {
                    EditorGUILayout.PropertyField(vertex.FindPropertyRelative("Array.size"));
                    for(int i = 0 ; i < vertex.arraySize ; i++)
                        EditorGUILayout.PropertyField(vertex.GetArrayElementAtIndex(i));
                }
                if (EditorGUILayout.PropertyField(triangle))
                {
                    EditorGUILayout.PropertyField(triangle.FindPropertyRelative("Array.size"));
                    for (int i = 0; i < triangle.arraySize; i++)
                        EditorGUILayout.PropertyField(triangle.GetArrayElementAtIndex(i));
                }

                GUILayout.Space(15);
            }

            EditorGUI.indentLevel = 0;
            EditorGUILayout.PropertyField(property.FindPropertyRelative("renderType"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("color"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("texture"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("normalMap"));

            EditorGUILayout.PropertyField(property.FindPropertyRelative("reflection"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("refraction"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("opacity"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("density"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("glossiness"));
        }
    }
}