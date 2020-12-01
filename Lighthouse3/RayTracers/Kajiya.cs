using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lighthouse3.RayTracers
{
    public static class Kajiya
    {

        public static Vector3 TraceRay(Ray ray, Scene scene, int depth = 1, float currentRefractiveIndex = Material.RefractiveIndex.Vacuum, float lastRefractiveIndex = Material.RefractiveIndex.Vacuum, bool debug = false, int rayCount = 10)
        {
            Intersection intersection = ray.NearestIntersection(scene.primitives);
            Material material = intersection.hit.material;
            Vector3 normal = intersection.hit.Normal(intersection);
            Vector3 color = Color.Black;

            bool backface = false;
            if (Vector3.Dot(ray.direction, normal) > 0)
            {
                normal = -normal;
                backface = true;
            }

            //Handle diffuse color
            if (material.diffuse > 0)
            {
                Vector3 illumination = Color.Black;
                for (int i = 0; i < rayCount; i++) 
                {
                    // Select a light at random (not taking into account relative light importance)
                    Light light = scene.lights[Calc.RandomInt(0, scene.lights.Length)];
                    Vector3 lightColor = light.DirectIllumination(intersection, normal, scene);
                    illumination = illumination + lightColor;
                }

                color = illumination * material.diffuse;
            }

            //Handle specularity
            if (material.specularity > 0)
            {
                Ray reflection = ray.Reflect(intersection.distance, normal);
                color += TraceRay(reflection, scene, depth + 1, debug: debug) * material.specularity;
            }

            if (debug)
            {
                Console.WriteLine("====");
                Console.WriteLine("Ray direction: " + intersection.ray.direction);
                if (intersection.hit is Primitives.Sphere)
                    Console.WriteLine("Sphere hit: " + ((Primitives.Sphere)intersection.hit).radius);
                if (intersection.hit is Primitives.Plane)
                    Console.WriteLine("Plane hit: " + ((Primitives.Plane)intersection.hit).position);
                Console.WriteLine("Hit location: " + intersection.ray.GetPoint(intersection.distance));
                Console.WriteLine("DEPTH " + depth + " LAST INDEX: " + lastRefractiveIndex + " CURRENT INDEX: " + currentRefractiveIndex + " NEW INDEX: " + material.refractiveIndex + " BACKFACE: " + backface);

            }

            //Handle transparancy
            if (material.transparancy > 0)
            {
                // Refract the ray to either go into the material or come out of the material
                Ray refraction;
                float reflectionChance;
                Vector3 refractionColor = Vector3.Zero;
                refraction = ray.Refract(intersection.distance, normal, currentRefractiveIndex, backface ? lastRefractiveIndex : material.refractiveIndex, out reflectionChance);

                float refractionChance = (1 - reflectionChance);
                if (debug)
                    Console.WriteLine(reflectionChance);

                if (refraction == null)
                {
                    //Total internal reflection
                    reflectionChance = 1;
                    refractionChance = 0;
                }
                else if (backface || reflectionChance < Calc.Epsilon)
                {
                    //Either backface or reflection chance too low, just ignore
                    reflectionChance = 0;
                    refractionChance = 1;
                }

                if (refractionChance >= Calc.Random())
                {
                    refractionColor += TraceRay(refraction, scene, depth + 1, backface ? lastRefractiveIndex : material.refractiveIndex, currentRefractiveIndex, debug: debug) * refractionChance;
                } else
                {
                    Ray reflection = ray.Reflect(intersection.distance, normal);
                    refractionColor += TraceRay(reflection, scene, depth + 1, debug: debug) * reflectionChance;
                } 

                color += refractionColor * material.transparancy;
            }

            return color;
        }
    }
}
