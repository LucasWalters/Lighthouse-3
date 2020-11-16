using Lighthouse3.Primitives;
using OpenTK;
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
                Intersection i = primitive.Intersect(this);
                if (i != null && (intersection == null || i.distance < intersection.distance))
                    intersection = i;
            }
            return intersection;
        }

        public Intersection Trace(Primitive[] primitives)
        {
            Intersection nearest = NearestIntersection(primitives);
            return nearest;
        }
    }
    public class Intersection
    {
        public float distance = -1;
        public Ray ray;
        public Vector3 normal;
        public Material material;
        
        public Intersection(float distance, Ray ray, Vector3 normal, Material material)
        {
            this.distance = distance;
            this.ray = ray;
            this.normal = normal;
            this.material = material;
        }
    }
}