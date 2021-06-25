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
            //TODO: declare all these things only once, outside the function
            Vector3 totalColor = Color.White;
            Vector3 totalEnergy = Color.Black;
            bool sampleLight = true;
            float currentRefractiveIndex = Material.RefractiveIndex.Vacuum;
            float lastRefractiveIndex = Material.RefractiveIndex.Vacuum;
            Vector3 lastNormal = scene.mainCamera.direction;
            bool lastDiffuse = false;

            //IMPROVED: declarations of stuff is done outside the while loop so it only happends once.
            float cos_o, cos_i, solidAngle, lightPDF, misPDF;
            float brdfPDF = Calc.Inv2Pi;
            float epsilon = Calc.Epsilon;
            Rectangle rect;

            Vector3 normal, BRDF, intersectionPoint, randomPoint, toLight, illumination, point;
            bool backface = false ;
            bool killed = false;
            bool occluded, isEven, refract;
            float survivalChance, materialTypeRandom, randomTemp, dist, nDotR, reflectionChance;
            int nrLights, x, y, z;

            while (true)
            {
                Intersection intersection = ray.NearestIntersection(scene, debug);
                if (intersection == null)
                {
                    totalEnergy += totalColor * scene.backgroundColor;
                    break;
                }

                Material material = intersection.hit.material;

                //if (!MIS)
                //    break;

                if (material.emissive > 0f)
                {
                    if (debug)
                        Console.WriteLine("Light hit!");

                    // If we shouldn't sample light check if we are using MIS and if so, do sample it but with both PDFs
                    if (!sampleLight)
                    {

                        if (!MIS) //IMPROVED: This is moved outside the 2 if statements, so that a break happends as fast as possible if necessary
                            break;

                        rect = (Rectangle)intersection.hit;
                        cos_o = Vector3.Dot(-ray.direction, rect.normal);
                        cos_i = Vector3.Dot(ray.direction, lastNormal);

                        solidAngle = (cos_o * rect.area) / (intersection.distance * intersection.distance);
                        lightPDF = 1f / solidAngle;
                        //IMPROVED: as brdfPDF here is a static number, this can be declared outside the while loop
                        //brdfPDF = Calc.Inv2Pi; //cos_i * Calc.InvPi;   Currently not using cosine random reflections
                        misPDF = lightPDF + brdfPDF;// Calc.PowerHeuristic(brdfPDF, lightPDF);
                        if (lastDiffuse)
                            totalColor *= brdfPDF;

                        totalEnergy += totalColor * material.color * material.emissive * cos_i / misPDF;
                    } 
                    else
                    {
                        totalEnergy += totalColor * material.color * material.emissive;
                    }
                    
                    break;
                }


                normal = intersection.hit.Normal(intersection);

                //bool backface = false;
                if (Vector3.Dot(ray.direction, normal) > 0)
                {
                    normal = -normal;
                    backface = true;
                }
                lastNormal = normal;

                //bool killed = false;

                // Russian Roulette
                survivalChance = Calc.Clamp(Calc.Max(material.color.X, material.color.Y, material.color.Z), epsilon, 0.9f);
                //IMPROVED: call Random only once
                randomTemp = Calc.Random();
                if (randomTemp > survivalChance)
                {
                    killed = true;
                }

                materialTypeRandom = randomTemp;

                //Handle diffuse color
                if (materialTypeRandom < material.diffuse)
                {
                    //IMPROVED: calc.inv2pi = brdfPDF
                    BRDF = material.color * brdfPDF;

                    nrLights = scene.lights.Length;
                    AreaLight light = (AreaLight)scene.lights[nrLights > 1 ? Calc.RandomInt(0, nrLights) : 0];

                    // Direct Illumination
                    intersectionPoint = ray.GetPoint(intersection.distance);
                    randomPoint = light.RandomPointOnLight();
                    toLight = randomPoint - intersectionPoint;
                    dist = toLight.Length;
                    toLight /= dist;
                    cos_o = Vector3.Dot(-toLight, light.rect.normal);
                    cos_i = Vector3.Dot(toLight, normal);
                    if (cos_o > 0 && cos_i > 0)
                    {
                        // light is not behind surface point, trace shadow ray
                        Ray lightRay = new Ray(intersectionPoint + epsilon * toLight, toLight);
                        occluded = lightRay.Occluded(scene.primitives, dist - epsilon * 2);
                        if (!occluded)
                        {
                            // light is visible (V(p,p’)=1); calculate transport
                            solidAngle = (cos_o * light.rect.area) / (dist * dist);
                            lightPDF = 1f / solidAngle;
                            //brdfPDF = Calc.Inv2Pi; //cos_i * Calc.InvPi;   Currently not using cosine random reflections
                            misPDF = lightPDF + brdfPDF;// Calc.PowerHeuristic(lightPDF, brdfPDF);

                            // Multiply result with the number of lights in the scene
                            // If using MIS use both PDFs
                            illumination = totalColor * (cos_i / (MIS ?  misPDF : lightPDF)) * BRDF * light.color * nrLights * light.intensity;
                            //Render checkerboard pattern
                            if (material.checkerboard > 0)
                            {
                                point = ray.GetPoint(intersection.distance);
                                x = (int)(point.X / material.checkerboard);
                                if (point.X < 0)
                                    x++;
                                y = (int)(point.Y / material.checkerboard);
                                if (point.Y < 0)
                                    y++;
                                z = (int)(point.Z / material.checkerboard);
                                if (point.Z < 0)
                                    z++;
                                isEven = (x + y + z) % 2 == 0;
                                illumination *= (isEven ? 1 : 0.5f);
                            }
                            totalEnergy += illumination;
                        }
                    }
                    if (killed)
                        break;

                    ray = ray.RandomReflect(intersection.distance, normal);
                    lastDiffuse = true;
                    nDotR = Vector3.Dot(normal, ray.direction);
                    //IMPROVED: PDF == brdfPDF
                    //float PDF = Calc.Inv2Pi; //nDotR * Calc.InvPi;   Currently not using cosine random reflections
                    sampleLight = false;
                    totalColor *= (1f / survivalChance) * BRDF * (nDotR / brdfPDF);
                }
                //Handle specularity
                else if (materialTypeRandom < material.specularity + material.diffuse)
                {
                    if (killed)
                        break;

                    ray = ray.GlossyReflect(intersection.distance, normal, material.glossiness);
                    totalColor *= (1f / survivalChance) * material.color;
                    sampleLight = true;
                    lastDiffuse = false;
                }

                //Handle transparancy
                else
                {
                    if (killed)
                        break;

                    // Refract the ray to either go into the material or come out of the material                    
                    Ray refraction = ray.Refract(intersection.distance, normal, currentRefractiveIndex, backface ? lastRefractiveIndex : material.refractiveIndex, out reflectionChance);


                    if (reflectionChance >= 1f)
                        //Total internal reflection
                        refract = false;
                    else if (backface || reflectionChance < epsilon)
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
                    lastDiffuse = false;
                    totalColor *= (1f / survivalChance) * material.color;
                }
            }
            return totalEnergy;
        }
    }
}
