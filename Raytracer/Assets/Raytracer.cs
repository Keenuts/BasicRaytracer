using UnityEngine;

namespace Raytrace
{
    public class Raytracer
    {
        const int MAX_DEPTH = 4;

        Texture2D output;
        int width, height;
        float aspect;
        Scene scene;

        public Raytracer(int height, float aspect, Scene scene)
        {
            this.height = height;
            this.aspect = aspect;
            width = (int)(this.height * aspect);
            this.scene = scene;
        }

        public Texture2D Render()
        {
            if(output != null)
                GameObject.DestroyImmediate(output);
            if(output == null)
                output = new Texture2D(width, height);

            CastRays();

            output.Apply();
            return output;
        }

        void CastRays()
        {
            Gizmos.color = Color.green;
            Vector3 center = scene.camera.position + scene.camera.forward * scene.closePlane;
            

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vector3 dt = scene.camera.right * (x * (2F / width) - 1F) * Mathf.Tan(0.5F * scene.focal) * aspect
                        + scene.camera.up * (1F - y * (2F / height)) * Mathf.Tan(0.5F * scene.focal);
                    Vector3 direction = (center + dt + scene.camera.forward) - scene.camera.position;
                    direction = direction.normalized;

                    Color rayColor = RenderRay(scene.camera.position, direction, 0, false);
                    output.SetPixel(x, height - y, rayColor);
                }
                output.Apply();
            }
        }

        Color RenderRay(Vector3 origin, Vector3 direction, int depth, bool show)
        {
            Color output = scene.ambiant;

            float zdepth = 999F;
            bool oneIntersection = false;
            int touch = 0;

            for(int i = 0 ; i < scene.objects.Length ; i++)
            {
                SceneObject o = scene.objects[i];
                Vector3 intersection, normal;
                bool intersect = false;

                if (o.geometry == Geometry.Sphere)
                    intersect = IntersectRaySphere(origin, direction, o.transform.position, o.radius, out intersection, out normal);
                else
                    intersect = IntersectRaySphere(origin, direction, o.transform.position, o.radius, out intersection, out normal);

                if (!intersect)
                    continue;
                touch++;
                //Depth culling
                float distance = (intersection - origin).magnitude;
                if (zdepth < distance)
                    continue;
                zdepth = distance;

                //Debug display
                oneIntersection = intersect | oneIntersection;
                if (show)
                    Debug.DrawLine(origin, intersection, Color.green * (1F - depth / MAX_DEPTH));


                //Lightning
                float lightFactor = GetLightPower(intersection + normal * 0.005F, i, show);
                Vector3 lightNormal = normal.normalized;

                if(o.normalMap != null)
                {
                    Color normalMap = ReadTexture(o.normalMap, GetUVSphere(o.transform.position, intersection, o.radius));
                    
                    Vector3 additiveNormal = new Vector3(normalMap.r, normalMap.g, normalMap.b);
                    //additiveNormal.x = (additiveNormal.x - 0.5F) * 2F;
                    //additiveNormal.z = (additiveNormal.z - 0.5F) * 2F;
                    lightNormal = Vector3.Scale(lightNormal, additiveNormal);
                    lightNormal = lightNormal.normalized;
                }

                float lightPower = Vector3.Dot(lightNormal, -scene.light.forward);
                if(lightPower < 0) lightPower = 0;

                float illumination = (lightFactor * lightPower) + scene.ambiant.a;
                if(illumination > 1F) illumination = 1F;


                //Shading
                if(o.texture == null)
                    output = o.color;
                else
                    output = ReadTexture(o.texture, GetUVSphere(o.transform.position, intersection, o.radius));
                output *= illumination;

                if (show)
                {Gizmos.color = o.color; Gizmos.DrawWireCube(intersection, Vector3.one * 0.25F);}
                
                if (depth > MAX_DEPTH)
                    continue;

                //Reflection
                if (o.reflection > 0F)
                {
                    Vector3 reflected = Reflect(intersection - origin, normal).normalized;
                    Debug.DrawRay(intersection, reflected, Color.cyan);
                    Color reflection = RenderRay(intersection + normal * 0.005F, reflected, depth + 1, show);
                    output = reflection * o.reflection + output * (1F - o.reflection);
                }
                
                //Refraction
                if(o.renderType != RenderType.Transparent)
                    continue;
                Vector3 refractionIN_point, refractionIN_normal;
                Color refractionIN_color, refractionOUT_color;
                Vector3 refractedDirection = Refract(intersection, normal, 1F, o.refraction);
                float refractedPower = refractedDirection.magnitude * (1 - o.opacity);

                if (IntersectRaySphere(intersection - normal * 0.05F, refractedDirection, o.transform.position, o.radius, out refractionIN_point, out refractionIN_normal))
                {
                    refractionIN_color = o.color * Mathf.Clamp01((refractionIN_point - intersection).magnitude / (o.radius * 2F) * o.density);
                    Vector3 refractedOUT_direction = Refract(refractedDirection, refractionIN_normal, o.refraction, 1.0F);


                    if (show)
                    {
                        Gizmos.color = Color.blue;
                        Gizmos.DrawLine(intersection, refractionIN_point);
                        Gizmos.color = Color.cyan;
                        Gizmos.DrawRay(refractionIN_point, refractedOUT_direction);
                    }

                    refractionOUT_color = RenderRay(refractionIN_point + refractedOUT_direction * 0.05F, refractedOUT_direction, depth + 1, show);
                    output = refractionIN_color * output * (1F - refractedPower) + refractedPower * refractionOUT_color;
                }
                else if(show)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(intersection, refractionIN_point);
                }
            }

            if (!oneIntersection && show)
                Debug.DrawRay(origin, direction * 500F, Color.grey);
            return Saturate(output);
        }

        float GetLightPower(Vector3 point, int self, bool show)
        {
            float intensity = 1F;
            Vector3 direction = -scene.light.forward;

            for(int i = 0 ; i < scene.objects.Length ; i++)
            {
                SceneObject o = scene.objects[i];
                Vector3 intersection, normal;
                if (IntersectRaySphere(point, direction, o.transform.position, o.radius, out intersection, out normal))
                {
                    if(Vector3.Dot(intersection - point, direction) > 0)
                    {
                        intensity = 0F;
                        if(show)
                            Debug.DrawLine(intersection, point, Color.black);
                    }
                }
            }

            return intensity;
        }

        bool IntersectRaySphere(Vector3 origin, Vector3 dir, Vector3 center, float radius)
        {
            Vector3 a, b;
            return IntersectRaySphere(origin, dir, center, radius, out a, out b);
        }

        bool IntersectRaySphere(Vector3 E, Vector3 V, Vector3 O, float radius, out Vector3 point, out Vector3 normal)
        {
            V = V.normalized;
            point = Vector3.zero;
            normal = point;

            Vector3 EO = O - E;
            float v = Vector3.Dot(EO, V);
            float disc = radius * radius - (Vector3.Dot(EO, EO) - v * v);
            if (disc < 0)
                return false;

            float d = Mathf.Sqrt(disc);
            point = E + (v - d) * V;
            float dot = Vector3.Dot(O - point, V);
            Vector3 pointB = E + (v - d) * V + V.normalized * radius * 2F * dot;
            normal = (point - O).normalized;
            
            if((E - O).magnitude < radius)
                point = pointB;
            //point = Vector3.Dot(V, point - E) > 0F ? point : pointB;
            return Vector3.Dot(point - E, V) > 0F;
        }

        Vector3 Reflect(Vector3 incident, Vector3 normal)
        {
            incident = incident.normalized;
            normal = normal.normalized;
            Vector3 refl = incident - 2F * normal * Vector3.Dot(normal, incident);
            return refl.normalized;
        }

        Vector3 Refract(Vector3 i, Vector3 n, float refracA, float refracB)
        {
            float refr = refracA / refracB;
            float cosi = Vector3.Dot(-i, n);
            float cost2 = 1F - refr * refr * (1.0f - cosi * cosi);
            Vector3 t = refr * i + ((refr * cosi - Mathf.Sqrt(Mathf.Abs(cost2))) * n);
            return t.normalized * (cost2 > 0 ? 1F : 0F);
        }

        Color Saturate(Color c)
        {
            c.r = Mathf.Clamp01(c.r);
            c.g = Mathf.Clamp01(c.g);
            c.b = Mathf.Clamp01(c.b);
            c.a = Mathf.Clamp01(c.a);
            return c;
        }

        public void ShowRay(Vector3 target)
        {
            Vector3 origin = scene.camera.position;
            Vector3 direction = (target - origin).normalized;
            RenderRay(origin, direction, 0, true);
        }

        Vector2 GetUVSphere(Vector3 center, Vector3 point, float radius)
        {
            Vector3 vp = (point - center).normalized;
            float phi = Mathf.Acos(-Vector3.Dot(Vector3.up, vp));
            float v = phi / Mathf.PI;
            float u = 0;

            float theta = (Mathf.Acos(Vector3.Dot(vp, Vector3.right) / Mathf.Sin(phi))) / (2 * Mathf.PI);
            if(Vector3.Dot(Vector3.Cross(Vector3.up, Vector3.right), vp) > 0)
                u = theta;
            else
                u = 1 - theta;

            return new Vector2(u, v);
        }

        Color ReadTexture(Texture2D tex, Vector2 uv)
        {
            int x = (int)((uv.x % 1F) * tex.width);
            int y = (int)((uv.y % 1F) * tex.height);

            return tex.GetPixel(x, y);
        }
    }
}
