using Lighthouse3.Lights;
using Lighthouse3.Primitives;
using Lighthouse3.Scenes;
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
        public static bool MIS = true;

        public static Vector3 TraceRay(Ray ray, Scene scene, bool debug = false)
        {
            Vector3 totalColor = Color.White;
            Vector3 totalEnergy = Color.Black;
            bool sampleLight = true;
            float currentRefractiveIndex = Material.RefractiveIndex.Vacuum;
            float lastRefractiveIndex = Material.RefractiveIndex.Vacuum;
            Vector3 lastNormal = scene.mainCamera.direction;
            while (true)
            {
                Intersection intersection = ray.NearestIntersection(scene, debug);
                if (intersection == null)
                {
                    totalEnergy += totalColor * scene.backgroundColor;
                    break;
                }

                Material material = intersection.hit.material;

                if (material.emissive > 0f)
                {
                    if (debug)
                        Console.WriteLine("Light hit!");

                    Vector3 c = totalColor * material.color * material.emissive;

                    if (!sampleLight)
                    {
                        if (!MIS)
                            break;

                        Rectangle rect = (Rectangle)intersection.hit;
                        float cos_o = Vector3.Dot(-ray.direction, rect.normal);
                        float cos_i = Vector3.Dot(ray.direction, lastNormal);

                        float solidAngle = (cos_o * rect.area) / (intersection.distance * intersection.distance);
                        float lightPDF = 1f / solidAngle;
                        float brdfPDF = Calc.Inv2Pi; //cos_i * Calc.InvPi;   Currently not using cosine random reflections
                        float misPDF = lightPDF + brdfPDF;// Calc.PowerHeuristic(brdfPDF, lightPDF);
                        
                        c *= cos_i / misPDF;
                    }
                    totalEnergy += c;
                    break;
                }


                Vector3 normal = intersection.hit.Normal(intersection);

                bool backface = false;
                if (Vector3.Dot(ray.direction, normal) > 0)
                {
                    normal = -normal;
                    backface = true;
                }
                lastNormal = normal;

                bool killed = false;

                // Russian Roulette
                float survivalChance = Calc.Clamp(Calc.Max(material.color.X, material.color.Y, material.color.Z), Calc.Epsilon, 0.9f);
                if (Calc.Random() > survivalChance)
                {
                    killed = true;
                }

                float materialTypeRandom = Calc.Random();

                //Handle diffuse color
                if (materialTypeRandom < material.diffuse)
                {
                    Vector3 BRDF = material.color * Calc.InvPi;

                    int nrLights = scene.lights.Length;
                    AreaLight light = (AreaLight)scene.lights[nrLights > 1 ? Calc.RandomInt(0, nrLights) : 0];


                    // Direct Illumination
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
                            float brdfPDF = Calc.Inv2Pi; //cos_i * Calc.InvPi;   Currently not using cosine random reflections
                            float misPDF = lightPDF + brdfPDF;// Calc.PowerHeuristic(lightPDF, brdfPDF);
                            // Multiply result with the number of lights in the scene

                            Vector3 c = totalColor * (cos_i / (MIS ?  misPDF : lightPDF)) * BRDF * light.color * nrLights * light.intensity;
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
                                c *= (isEven ? 1 : 0.5f);
                            }
                            totalEnergy += c;
                        }
                    }
                    if (killed)
                        break;

                    ray = ray.RandomReflect(intersection.distance, normal);

                    float nDotR = Vector3.Dot(normal, ray.direction);
                    float PDF = Calc.Inv2Pi; //nDotR * Calc.InvPi;   Currently not using cosine random reflections
                    sampleLight = false;
                    totalColor *= (1f / survivalChance) * BRDF * (nDotR / PDF);
                }
                //Handle specularity
                else if (materialTypeRandom < material.specularity + material.diffuse)
                {
                    if (killed)
                        break;

                    ray = ray.GlossyReflect(intersection.distance, normal, material.glossiness);
                    totalColor *= (1f / survivalChance) * material.color;
                    sampleLight = true;
                }

                //Handle transparancy
                else
                {
                    if (killed)
                        break;

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
                    {
                        ray = refraction;
                        float temp = currentRefractiveIndex;
                        currentRefractiveIndex = backface ? lastRefractiveIndex : material.refractiveIndex;
                        lastRefractiveIndex = temp;
                    }
                    else
                    {
                        ray = ray.Reflect(intersection.distance, normal);
                    }
                    sampleLight = true;
                    totalColor *= (1f / survivalChance) * material.color;
                }

            }
            return totalEnergy;
        }
    }
}
