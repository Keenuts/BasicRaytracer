using UnityEngine;

namespace Raytrace
{
    public class SceneObjectHost : MonoBehaviour
    {
        [SerializeField]
        public SceneObject sceneObject;

        public void Setup()
        {
            sceneObject.transform = transform;
        }
    }
}
