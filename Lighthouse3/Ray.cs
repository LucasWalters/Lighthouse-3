using Lighthouse3.Primitives;
using OpenTK;
using System;
using System.Collections;

namespace Lighthouse3
{
    public struct Ray
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

        public Intersection NearestIntersection(Sphere[] spheres)
        {
            Intersection intersection = null;
            foreach(Sphere sphere in spheres)
            {
                Intersection i = sphere.Intersect(this);
                if (i != null && (intersection == null || i.distance < intersection.distance))
                    intersection = i;
            }
            return intersection;
        }

        public float Trace(Sphere[] spheres)
        {
            Intersection nearest = NearestIntersection(spheres);
            if (nearest == null)
                return -1;

            return nearest.distance;
        }
    }
    public class Intersection
    {
        public float distance = -1;
        public Vector3 normal;
        //Material

        private Intersection() { }
        public Intersection(float distance, Vector3 normal)
        {
            this.distance = distance;
            this.normal = normal;
        }
    }
}