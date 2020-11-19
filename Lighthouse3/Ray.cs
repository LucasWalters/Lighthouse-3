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

        public bool Occluded(Primitive[] primitives, float distanceSquared)
        {
            foreach (Primitive primitive in primitives)
            {
                float t;
                bool intersected = primitive.Intersect(this, out t);
                if (intersected && t*t < distanceSquared)
                    return true;
            }
            return false;
        }

        public Vector3 Trace(Scene scene)
        {
            Intersection intersection = NearestIntersection(scene.primitives);
            if (intersection == null)
                return scene.backgroundColor;

            Vector3 color = Color.Black;

            //Handle diffuse color
            if (intersection.hit.material.diffuse > 0)
            {
                Vector3 illumination = Color.Black;
                foreach (Light light in scene.lights)
                {
                    Vector3 lightColor = light.DirectIllumination(intersection, scene);
                    illumination = illumination + lightColor;
                }

                color = illumination * intersection.hit.material.diffuse;
            }

            //Handle specularity
            if (intersection.hit.material.specularity > 0)
            {
                Ray reflection = Reflect(intersection.distance, intersection.hit.Normal(intersection));
                color += reflection.Trace(scene) * intersection.hit.material.specularity;
            }
            return intersection.hit.material.color * color;
        }

        public Ray Reflect(float distance, Vector3 normal)
        {
            Vector3 rOrigin = GetPoint(distance);
            Vector3 rDirection = direction - 2 * normal * Vector3.Dot(direction, normal);
            return new Ray(rOrigin, rDirection.Normalized());
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