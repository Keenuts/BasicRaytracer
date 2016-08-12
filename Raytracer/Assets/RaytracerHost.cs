using UnityEngine;
using System.Collections;

namespace Raytrace
{
    public class RaytracerHost : MonoBehaviour
    {
        [SerializeField]
        Material outputMaterial;
        [SerializeField]
        Texture2D output;

        [SerializeField]
        int outputHeight = 256;
        [SerializeField]
        float aspect = 1.798F;

        [SerializeField]
        Scene scene;
        [SerializeField]
        Transform raytarget;

        Raytracer r;

        public void Render(bool repaint)
        {
            SceneObjectHost[] objectHost = FindObjectsOfType<SceneObjectHost>();
            SceneObject[] sceneObjects = new SceneObject[objectHost.Length];
            for (int i = 0; i < objectHost.Length; i++)
                sceneObjects[i] = objectHost[i].sceneObject;

            scene.objects = sceneObjects;
            r = new Raytracer(outputHeight, aspect, scene);
            output = r.Render();
            outputMaterial.SetTexture("_MainTex", output);

            if (repaint)
                UnityEditor.SceneView.RepaintAll();
        }

        public void OnDrawGizmos()
        {
            scene.DrawGizmos();

            if(r == null)
                r = new Raytracer(outputHeight, aspect, scene);

            if(r != null && raytarget)
            {
                UnityEditor.SceneView.RepaintAll();
                r.ShowRay(raytarget.position);
            }
        }
    }
}
