using Lighthouse3.Lights;
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

        public static Vector3 TraceRay(Ray ray, Scene scene, bool debug = false)
        {
            Vector3 totalColor = Color.White;
            Vector3 totalEnergy = Color.Black;
            bool sampleLight = true;
            int depth = 0;
            float currentRefractiveIndex = Material.RefractiveIndex.Vacuum;
            float lastRefractiveIndex = Material.RefractiveIndex.Vacuum;
            while (true)
            {
                Intersection intersection = ray.NearestIntersection(scene);
                if (intersection == null)
                {
                    totalEnergy += totalColor * scene.backgroundColor;
                    break;
                }

                Material material = intersection.hit.material;

                if (material.emissive)
                {
                    if (debug)
                        Console.WriteLine("Light hit!");
                    if (sampleLight)
                        totalEnergy += totalColor * material.color;
                    break;
                }


                Vector3 normal = intersection.hit.Normal(intersection);

                if (++depth >= MaxDepth)
                {
                    if (debug)
                        Console.WriteLine("Max reached!");
                    break;
                }

                bool backface = false;
                if (Vector3.Dot(ray.direction, normal) > 0)
                {
                    normal = -normal;
                    backface = true;
                }

                float materialTypeRandom = Calc.Random();

                //Handle diffuse color
                if (materialTypeRandom < material.diffuse)
                {
                    Vector3 BRDF = material.color * Calc.InvPi;

                    int nrLights = scene.lights.Length;
                    AreaLight light = (AreaLight)scene.lights[nrLights > 1 ? Calc.RandomInt(0, nrLights) : 0];
                    //totalEnergy += totalColor * BRDF * light.DirectIllumination(intersection, normal, scene, debug) * nrLights;


                    Vector3 intersectionPoint = ray.GetPoint(intersection.distance);
                    Vector3 randomPoint = light.RandomPointOnLight();
                    Vector3 toLight = randomPoint - intersectionPoint;
                    float dist = toLight.Length;
                    toLight /= dist;
                    float cos_o = Vector3.Dot(-toLight, light.rect.normal);
                    float cos_i = Vector3.Dot(toLight, normal);
                    if (cos_o > 0 && cos_i > 0)
                    {
                        // light is not behind surface point, trace shadow ray
                        Ray lightRay = new Ray(intersectionPoint + Calc.Epsilon * toLight, toLight);
                        bool occluded = lightRay.Occluded(scene.primitives, dist - Calc.Epsilon * 2);
                        if (!occluded)
                        {
                            // light is visible (V(p,p’)=1); calculate transport
                            float solidAngle = (cos_o * light.rect.area) / (dist * dist);
                            float lightPDF = 1f / solidAngle;
                            float brdfPDF = cos_i * Calc.InvPi;
                            float misPDF = lightPDF + brdfPDF;
                            // Multiply result with the number of lights in the scene
                            totalEnergy += totalColor * (cos_i / misPDF) * BRDF * light.color * nrLights * light.intensity;
                        }
                    }

                    ray = ray.RandomReflectCosineWeighted(intersection.distance, normal);
                    float nDotR = Vector3.Dot(normal, ray.direction);
                    float PDF = nDotR * Calc.InvPi;
                    totalColor *= BRDF * nDotR / PDF;
                    sampleLight = false;
                }
                //Handle specularity
                else if (materialTypeRandom < material.specularity + material.diffuse)
                {
                    ray = ray.GlossyReflect(intersection.distance, normal, material.glossiness);
                    totalColor *= material.color;
                    sampleLight = true;
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
                    {
                        ray = refraction;
                        lastRefractiveIndex = currentRefractiveIndex;
                        currentRefractiveIndex = backface ? lastRefractiveIndex : material.refractiveIndex;
                    }
                    else
                    {
                        ray = ray.Reflect(intersection.distance, normal);
                    }
                    totalColor *= material.color;
                    sampleLight = true;
                }

            }
            return totalEnergy;

            

            

            

            //Render checkerboard pattern
            //if (material.isCheckerboard)
            //{
            //    Vector3 point = ray.GetPoint(intersection.distance);
            //    int x = (int)Math.Floor(point.X);
            //    int y = (int)Math.Floor(point.Y);
            //    int z = (int)Math.Floor(point.Z);
            //    bool isEven = (x + y + z) % 2 == 0;
            //    color *= (isEven ? 1 : 0.5f);
            //}

            //return material.color * color;
        }
    }
}
