using UnityEngine;
using UnityEditor;

namespace Raytrace
{
    [CustomEditor(typeof(RaytracerHost))]
    public class RaytracerHostEditor : Editor
    {
        SerializedProperty outputMaterial;
        SerializedProperty scene;


        SerializedProperty outputHeight, aspect;
        SerializedProperty raytarget;
        RaytracerHost raytracer;

        float lastRenderTime;

        void OnEnable()
        {
            outputMaterial = serializedObject.FindProperty("outputMaterial");
            outputHeight = serializedObject.FindProperty("outputHeight");
            aspect = serializedObject.FindProperty("aspect");

            scene = serializedObject.FindProperty("scene");
            raytarget = serializedObject.FindProperty("raytarget");
            raytracer = (RaytracerHost)target;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(outputMaterial);
            EditorGUILayout.PropertyField(outputHeight);
            EditorGUILayout.PropertyField(aspect);
            GUILayout.Label("Resolution : " + (int)(outputHeight.intValue * aspect.floatValue) + "x" + outputHeight.intValue);
            EditorGUILayout.PropertyField(scene);
            EditorGUILayout.PropertyField(raytarget);


            serializedObject.ApplyModifiedProperties();
            GUILayout.Space(10);


            if(GUILayout.Button("Render"))
            {
                System.DateTime start = System.DateTime.Now;
                raytracer.Render(false);
                lastRenderTime = (System.DateTime.Now - start).Milliseconds;
            }
            /*
            if (GUILayout.Button("Render & Repaint"))
            {
                float start = Time.time;
                raytracer.Render(true);
                lastRenderTime = Time.time - start;
            }
            */
            GUILayout.Label("Render time : " + lastRenderTime + " ms");
    }
    }
}
