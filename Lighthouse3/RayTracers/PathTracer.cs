using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lighthouse3.RayTracers
{
    public static class PathTracer
    {
        public const int MaxDepth = 10;

        public static Vector3 TraceRay(Ray ray, Scene scene, bool sampleLight, int depth = 1, float currentRefractiveIndex = Material.RefractiveIndex.Vacuum, float lastRefractiveIndex = Material.RefractiveIndex.Vacuum, bool debug = false)
        {


            Intersection intersection = ray.NearestIntersection(scene);
            if (intersection == null)
                return scene.backgroundColor;

            Material material = intersection.hit.material;

            if (material.emissive)
            {
                if (debug)
                    Console.WriteLine("Light hit!");
                return sampleLight ? material.color : Color.Black;
            }


            Vector3 normal = intersection.hit.Normal(intersection);

            if (depth > MaxDepth)
            {
                if (debug)
                    Console.WriteLine("Max reached!");

                //if (material.diffuse > 0 && scene.lights.Length > 0)
                //{
                //    Light light = scene.lights[Calc.RandomInt(0, scene.lights.Length)];
                //    return material.color * light.DirectIllumination(intersection, normal, scene, debug) * scene.lights.Length;
                //}
                return Color.Black;
            }

            Vector3 color;

            bool backface = false;
            if (Vector3.Dot(ray.direction, normal) > 0)
            {
                normal = -normal;
                backface = true;
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
                Console.WriteLine("DEPTH " + depth + " GLOSSY: " + material.glossiness + " CURRENT INDEX: " + currentRefractiveIndex + " NEW INDEX: " + material.refractiveIndex + " BACKFACE: " + backface);

            }

            float materialTypeRandom = Calc.Random();

            //Handle diffuse color
            if (materialTypeRandom < material.diffuse)
            {
                Vector3 BRDF = material.color * Calc.InvPi;

                int nrLights = scene.lights.Length;
                Light light = scene.lights[nrLights > 1 ? Calc.RandomInt(0, nrLights) : 0];
                color = BRDF * light.DirectIllumination(intersection, normal, scene, debug) * nrLights;

                Ray reflection = ray.RandomReflect(intersection.distance, normal);

                Vector3 reflectionColor = TraceRay(reflection, scene, false, depth + 1, debug: debug) * Vector3.Dot(normal, reflection.direction);

                return Calc.Pi * 2f * BRDF * reflectionColor + color;
            }

            //Handle specularity
            else if(materialTypeRandom < material.specularity + material.diffuse)
            {
                Ray reflection = ray.GlossyReflect(intersection.distance, normal, material.glossiness);
                color = TraceRay(reflection, scene, true, depth + 1, debug: debug);
            }

            //Handle transparancy
            else
            {
                // Refract the ray to either go into the material or come out of the material
                Ray refraction;
                float reflectionChance;
                refraction = ray.Refract(intersection.distance, normal, currentRefractiveIndex, backface ? lastRefractiveIndex : material.refractiveIndex, out reflectionChance);

                bool refract;

                if (refraction == null)
                    //Total internal reflection
                    refract = false;
                else if (backface || reflectionChance < Calc.Epsilon)
                    //Either backface or reflection chance too low, just ignore
                    refract = true;
                else
                    refract = Calc.Random() > reflectionChance;

                if (refract)
                    color = TraceRay(refraction, scene, true, depth + 1, backface ? lastRefractiveIndex : material.refractiveIndex, currentRefractiveIndex, debug: debug);
                else
                    color = TraceRay(ray.Reflect(intersection.distance, normal), scene, true, depth + 1, debug: debug);
            }

            //Render checkerboard pattern
            if (material.isCheckerboard)
            {
                Vector3 point = ray.GetPoint(intersection.distance);
                int x = (int)Math.Floor(point.X);
                int y = (int)Math.Floor(point.Y);
                int z = (int)Math.Floor(point.Z);
                bool isEven = (x + y + z) % 2 == 0;
                color *= (isEven ? 1 : 0.5f);
            }

            return material.color * color;
        }
    }
}
