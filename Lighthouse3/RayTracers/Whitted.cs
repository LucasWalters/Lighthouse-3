using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lighthouse3.RayTracers
{
    public static class Whitted
    {
        public const int MaxDepth = 10;

        // Returns a color
        public static Vector3 TraceRay(Ray ray, Scene scene, int depth = 1, float currentRefractiveIndex = Material.RefractiveIndex.Vacuum, float lastRefractiveIndex = Material.RefractiveIndex.Vacuum, bool debug = false)
        {

            if (depth > MaxDepth)
            {
                if (debug)
                    Console.WriteLine("Max reached!");
                return Color.White;
            }

            Intersection intersection = ray.NearestIntersection(scene);
            if (intersection == null)
                return scene.backgroundColor;
            

            Vector3 color = Color.Black;
            Material material = intersection.hit.material;
            Vector3 normal = intersection.hit.Normal(intersection);
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
                foreach (Light light in scene.lights)
                {
                    Vector3 lightColor = light.DirectIllumination(intersection, normal, scene);
                    illumination += lightColor;
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
            if (material.transparency > 0)
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

                if (refractionChance > 0)
                {
                    refractionColor += TraceRay(refraction, scene, depth + 1, backface ? lastRefractiveIndex : material.refractiveIndex, currentRefractiveIndex, debug: debug) * refractionChance;
                }
                // If the Fresnel Schlick approximation defines some light should reflect instead of refract
                if (reflectionChance > 0)
                {
                    Ray reflection = ray.Reflect(intersection.distance, normal);
                    refractionColor += TraceRay(reflection, scene, depth + 1, debug: debug) * reflectionChance;
                }

                color += refractionColor * material.transparency;
            }

            //Render checkerboard pattern
            if (material.isCheckerboard)
            {
                Vector3 point = ray.GetPoint(intersection.distance);
                int x = (int)Math.Floor(point.X);
                int y = (int)Math.Floor(point.Y);
                int z = (int)Math.Floor(point.Z);
                bool isEven = (x + y + z) % 2 == 0;
                return (material.color * (isEven ? 1 : 0.5f)) * color;
            }
            return material.color * color;

        }
    }
}
