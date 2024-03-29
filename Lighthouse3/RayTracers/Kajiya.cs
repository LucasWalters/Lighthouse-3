﻿using Lighthouse3.Scenes;
using System.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lighthouse3.RayTracers
{
    public static class Kajiya
    {
        public const int MaxDepth = 500;

        public static Vector3 TraceRay(Ray ray, Scene scene, int depth = 1, float currentRefractiveIndex = Material.RefractiveIndex.Vacuum, float lastRefractiveIndex = Material.RefractiveIndex.Vacuum, bool debug = false)
        {


            Intersection intersection = ray.NearestIntersection(scene, debug);
            if (intersection == null)
                return scene.backgroundColor;

            Material material = intersection.hit.material;

            if (material.emissive > 0f)
            {
                if (debug)
                    Console.WriteLine("Light hit!");
                return material.color * material.emissive;
            }


            Vector3 normal = intersection.hit.Normal(intersection);

            //if (depth > MaxDepth)
            //{
            //    if (debug)
            //        Console.WriteLine("Max reached!");
            //    return Color.Black;
            //}

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
                if (intersection.hit is Primitives.PlanePrim)
                    Console.WriteLine("Plane hit: " + ((Primitives.PlanePrim)intersection.hit).position);
                Console.WriteLine("Hit location: " + intersection.ray.GetPoint(intersection.distance));
                Console.WriteLine("DEPTH " + depth + " GLOSSY: " + material.glossiness + " CURRENT INDEX: " + currentRefractiveIndex + " NEW INDEX: " + material.refractiveIndex + " BACKFACE: " + backface);

            }

            float materialTypeRandom = Calc.Random();

            // Russian Roulette
            float survivalChance = Calc.Clamp(Calc.Max(material.color.X, material.color.Y, material.color.Z), Calc.Epsilon, 0.9f);
            if (Calc.Random() > survivalChance)
            {
                return Color.Black;
            }

            //Handle diffuse color
            if (materialTypeRandom < material.diffuse)
            {
                Ray reflection = ray.RandomReflect(intersection.distance, normal);

                Vector3 reflectionColor = TraceRay(reflection, scene, depth + 1, debug: debug) * Vector3.Dot(normal, reflection.direction);

                color = 2f * reflectionColor;
            }

            //Handle specularity
            else if(materialTypeRandom < material.specularity + material.diffuse)
            {
                Ray reflection = ray.GlossyReflect(intersection.distance, normal, material.glossiness);
                color = TraceRay(reflection, scene, depth + 1, debug: debug);
            }

            //Handle transparancy
            else
            {
                // Refract the ray to either go into the material or come out of the material
                Ray refraction;
                float reflectionChance;
                refraction = ray.Refract(intersection.distance, normal, currentRefractiveIndex, backface ? lastRefractiveIndex : material.refractiveIndex, out reflectionChance);

                bool refract;

                if (reflectionChance >= 1f)
                    //Total internal reflection
                    refract = false;
                else if (backface || reflectionChance < Calc.Epsilon)
                    //Either backface or reflection chance too low, just ignore
                    refract = true;
                else
                    refract = Calc.Random() > reflectionChance;

                if (refract)
                    color = TraceRay(refraction, scene, depth + 1, backface ? lastRefractiveIndex : material.refractiveIndex, currentRefractiveIndex, debug: debug);
                else
                    color = TraceRay(ray.Reflect(intersection.distance, normal), scene, depth + 1, debug: debug);
            }

            //Render checkerboard pattern

            if (material.checkerboard > 0)
            {
                Vector3 point = ray.GetPoint(intersection.distance);
                int x = (int)(point.X / material.checkerboard);
                if (point.X < 0)
                    x++;
                int y = (int)(point.Y / material.checkerboard);
                if (point.Y < 0)
                    y++;
                int z = (int)(point.Z / material.checkerboard);
                if (point.Z < 0)
                    z++;
                bool isEven = (x + y + z) % 2 == 0;
                color *= (isEven ? 1 : 0.5f);
            }

            return (1f / survivalChance) * material.color * color;
        }
    }
}
