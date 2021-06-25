using Lighthouse3.BVH;
using Lighthouse3.Lights;
using Lighthouse3.Primitives;
using Lighthouse3.Scenes;
using System.Numerics;
using OpenTK.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Lighthouse3
{
    public class Ray
    {
        public Vector3 origin;
        public Vector3 direction;
        public float distance;
        public Vector3 invDir;

        // Direction should be normalized
        public Ray(Vector3 startPosition, Vector3 direction)
        {
            this.distance = float.MaxValue;
            this.origin = startPosition;
            if (direction.LengthSquared() != 0)
                direction = direction.Normalized();
            this.direction = direction;
            invDir.X = 1f / direction.X;
            invDir.Y = 1f / direction.Y;
            invDir.Z = 1f / direction.Z;
        }

        public Ray()
        {
            this.distance = float.MaxValue;
        }

        public void SetDirection(Vector3 direction)
        {
            this.direction = direction;
            invDir.X = 1f / direction.X;
            invDir.Y = 1f / direction.Y;
            invDir.Z = 1f / direction.Z;
        }

        public Vector3 GetPoint(float distance)
        {
            return origin + direction * distance;
        }

        public Intersection NearestIntersection(Scene scene, bool debug = false)
        {
            distance = float.MaxValue;
            Intersection intersection = new Intersection(int.MaxValue, this, null);

            intersection = scene.hasBVH ? 
                NearestIntersectionBVH(scene, intersection, debug) : 
                NearestIntersection(scene.primitives, intersection);

            float t;
            foreach (Light light in scene.lights)
            {
                if (light is AreaLight)
                {
                    Rectangle rect = ((AreaLight)light).rect;
                    bool intersected = rect.Intersect(this, out t);
                    if (intersected && t < intersection.distance)
                    {
                        intersection = new Intersection(t, this, rect);
                        distance = t;
                    }
                }
            }

            foreach (PlanePrim plane in scene.planes)
            {
                bool intersected = plane.Intersect(this, out t);
                if (intersected && t < intersection.distance)
                {
                    intersection = new Intersection(t, this, plane);
                    distance = t;
                }
            }

            return intersection.hit == null ? null : intersection;
        }

        private Intersection NearestIntersectionBVH(Scene scene, Intersection intersection, bool debug)
        {
            return IntersectNode(scene, scene.nodes[0], intersection, debug);
        }

        private Intersection IntersectNode(Scene scene, BVHNode node, Intersection intersection, bool debug)
        {
            //if (debug)
                //Console.WriteLine(node.bounds.Intersect(this));
            if (node.bounds.Intersect(this))
            {
                if (node.count < 0)
                {
                    int[] indices = node.childIndices;
                    for(int i = 0; i < indices.Length; i++)
                    {
                        if (indices[i] == 0) continue;
                        intersection = IntersectNode(scene, scene.nodes[indices[i]], intersection, debug);
                    }
                }
                else
                {
                    intersection = NearestIntersection(scene, node.firstOrLeft, node.count, intersection, debug);
                }
            }
            return intersection;
        }

        private Intersection NearestIntersection(Scene scene, int first, int count, Intersection intersection, bool debug)
        {
            float t;
            if (debug)
            {
                Console.WriteLine("First: " + first);
                Console.WriteLine("Count: " + count);
                Console.WriteLine(intersection.distance);
            }
            for (int i = 0; i < count; i++)
            {
                int index = scene.indices[first + i];
                bool intersected = scene.primitives[index].Intersect(this, out t);
                if (intersected && t < intersection.distance)
                {
                    if (debug)
                    {
                        Console.WriteLine("Intersected! At: " + first + i);
                    }
                    intersection = new Intersection(t, this, scene.primitives[index]);
                    distance = t;
                }
            }
            return intersection;
        }

        private Intersection NearestIntersection(Primitive[] primitives, Intersection intersection)
        {
            float t;
            foreach (Primitive primitive in primitives)
            {
                bool intersected = primitive.Intersect(this, out t);
                if (intersected && t < intersection.distance)
                {
                    intersection = new Intersection(t, this, primitive);
                    distance = t;
                }
            }
            return intersection;
        }

        public bool Occluded(Primitive[] primitives, float distance)
        {
            this.distance = distance;
            foreach (Primitive primitive in primitives)
            {
                float t;
                bool intersected = primitive.Intersect(this, out t);
                if (intersected && t > Calc.Epsilon && t < distance)
                    return true;
            }
            return false;
        }

        public bool OccludedSquared(Primitive[] primitives, float distanceSquared)
        {
            this.distance = float.MaxValue;
            foreach (Primitive primitive in primitives)
            {
                float t;
                bool intersected = primitive.Intersect(this, out t);
                if (intersected && t > Calc.Epsilon && t * t < distanceSquared)
                    return true;
            }
            return false;
        }

        public Ray GlossyReflect(float distance, Vector3 normal, float glossiness)
        {
            Ray reflection = Reflect(distance, normal);
            if (glossiness > 0)
            {
                Vector3 randomDirection = (reflection.direction + Calc.RandomInUnitSphere() * glossiness * Vector3.Dot(reflection.direction, normal)).Normalized();


               // Console.WriteLine("Old: " + reflection.direction);
                reflection.SetDirection(randomDirection);
                //Console.WriteLine("New: " + reflection.direction);
            }
            return reflection;

        }

        public Ray Reflect(float distance, Vector3 normal)
        {
            Vector3 rDirection = direction - 2 * Vector3.Dot(direction, normal) * normal;
            Vector3 rOrigin = GetPoint(distance) + rDirection * Calc.Epsilon;
            return new Ray(rOrigin, rDirection);
        }

        public Ray RandomReflect(float distance, Vector3 normal)
        {
            Vector3 rDirection = Calc.RandomOnUnitSphere();
            rDirection *= (Vector3.Dot(normal, rDirection) < 0 ? -1 : 1);
            Vector3 rOrigin = GetPoint(distance) + rDirection * Calc.Epsilon;
            return new Ray(rOrigin, rDirection);
        }

        public Ray RandomReflectCosineWeighted(float distance, Vector3 normal)
        {
            Vector3 rDirection = Calc.RandomOnHalfSphereCosineWeighted();
            rDirection = Calc.WorldToTangent(rDirection, normal);
            Vector3 rOrigin = GetPoint(distance) + rDirection * Calc.Epsilon;
            return new Ray(rOrigin, rDirection);
        }

        // From https://www.flipcode.com/archives/reflection_transmission.pdf
        // And https://seblagarde.wordpress.com/2013/04/29/memo-on-fresnel-equations/
        // And http://viclw17.github.io/2018/08/05/raytracing-dielectric-materials/
        public Ray Refract(float distance, Vector3 normal, float n1, float n2, out float reflectionChance)
        {
            float r0 = (n1 - n2) / (n1 + n2);
            r0 *= r0;
            float cosX = Vector3.Dot(normal, direction);
            float eta = n1 / n2;
            if (n1 > n2)
            {
                float inv_eta = n2 / n1;
                float int_sinT2 = inv_eta * inv_eta * (1f - cosX * cosX);
                if (int_sinT2 > 1f)
                {
                    reflectionChance = 1f;
                    return this;
                }
                cosX = Calc.Sqrt(1.0f - int_sinT2);
            } 
            else
            {
                float sinT2 = eta * eta * (1f - cosX * cosX);
                cosX = Calc.Sqrt(1.0f - sinT2);
            }
            Vector3 rDirection = eta * direction - (eta + cosX) * normal;
            Vector3 rOrigin = GetPoint(distance) + rDirection * Calc.Epsilon;
            reflectionChance = r0 + (1.0f - r0) * Calc.Pow(1.0f - cosX, 5);
            return new Ray(rOrigin, rDirection);
        }
    }
    public class Intersection
    {
        public float distance = -1;
        public Ray ray;
        public Primitive hit;
        
        public Intersection(float distance, Ray ray, Primitive hit)
        {
            this.distance = distance;
            this.ray = ray;
            this.hit = hit;
        }
    }
}