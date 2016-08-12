using UnityEngine;

namespace Raytrace
{
    [System.Serializable]
    public class Scene
    {
        public Transform camera;
        public float focal = 60;
        public float closePlane = 1F;

        public Transform light;
        public Color ambiant;

        public SceneObject[] objects;

        public void DrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(camera.position, Vector3.one * 0.5F);
            Gizmos.DrawRay(camera.position, camera.forward);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(light.position, 0.5F);
            Gizmos.DrawRay(light.position, light.forward);

            foreach(SceneObject o in objects)
                o.DrawGizmos();
        }
    }
    
    [System.Serializable]
    public class SceneObject
    {
        public Transform transform;
        public Geometry geometry = Geometry.Sphere;
        public float radius;
        public Vector3[] vertex;
        public int[] triangles;

        public RenderType renderType = RenderType.Opaque;
        public Color color = new Color(1,1,1,1);
        public Texture2D texture;
        public Texture2D normalMap;


        public float reflection = 0.2F;
        public float refraction = 1.0F;
        public float opacity = 1.0F;
        public float density = 1.0F;
        public float glossiness = 1F;

        public void DrawGizmos()
        {
            Gizmos.color = color;
            if(geometry == Geometry.Sphere)
                Gizmos.DrawWireSphere(transform.position, radius);
            else
            {
                for(int i = 0 ; i < triangles.Length ; i += 3)
                {
                    Vector3 l2w = transform.position;
                    Gizmos.DrawLine(l2w + vertex[triangles[i]], l2w + vertex[triangles[i + 1]]);
                    Gizmos.DrawLine(l2w + vertex[triangles[i + 1]], l2w + vertex[triangles[i + 2]]);
                    Gizmos.DrawLine(l2w + vertex[triangles[i + 2]], l2w + vertex[triangles[i]]);
                }
            }
        }
    }

    public enum Geometry
    {
        Sphere, Mesh
    }

    public enum RenderType
    {
        Opaque, Transparent
    }
}
