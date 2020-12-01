using Lighthouse3.Primitives;
using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections;

namespace Lighthouse3
{
    public class Ray
    {
        public Vector3 origin;
        public Vector3 direction;

        // Direction should be normalized
        public Ray(Vector3 startPosition, Vector3 direction)
        {
            this.origin = startPosition;
            if (direction.LengthSquared != 0)
                direction = direction.Normalized();
            this.direction = direction;
        }

        public Vector3 GetPoint(float distance)
        {
            return origin + direction * distance;
        }

        public Intersection NearestIntersection(Primitive[] primitives)
        {
            Intersection intersection = null;
            foreach(Primitive primitive in primitives)
            {
                float t;
                bool intersected = primitive.Intersect(this, out t);
                if (intersected && (intersection == null || t < intersection.distance))
                {
                    intersection = new Intersection(t, this, primitive);
                }
            }
            return intersection;
        }

        public bool Occluded(Primitive[] primitives, float distance)
        {
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
                reflection.direction = randomDirection;
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
                    return null;
                }
                cosX = (float)Math.Sqrt(1.0f - int_sinT2);
            } 
            else
            {
                float sinT2 = eta * eta * (1f - cosX * cosX);
                cosX = (float)Math.Sqrt(1.0f - sinT2);
            }
            Vector3 rDirection = eta * direction - (eta + cosX) * normal;
            Vector3 rOrigin = GetPoint(distance) + rDirection * Calc.Epsilon;
            reflectionChance = r0 + (1.0f - r0) * (float)Math.Pow(1.0f - cosX, 5.0f);
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