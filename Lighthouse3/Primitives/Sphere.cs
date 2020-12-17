using OpenTK;
using System;
using System.Collections;

namespace Lighthouse3.Primitives
{
    public class Sphere : Primitive
    {
        public Vector3 position;
        public float radius;
        public float radiusSquared;

        public Sphere(Vector3 position, float radius, Material material) : base(material)
        {
            this.position = position;
            this.radius = radius;
            radiusSquared = radius * radius;
            bounds.min = Min();
            bounds.max = Max();
            bounds = bounds.ResetCenter();
        }

        // From https://www.scratchapixel.com/lessons/3d-basic-rendering/minimal-ray-tracer-rendering-simple-shapes/ray-sphere-intersection
        public override bool Intersect(Ray ray, out float t)
        {
            t = -1;
            // geometric solution
            Vector3 L = position - ray.origin; 
            float tca = Vector3.Dot(L, ray.direction); 
            // if (tca < 0) return false;
            float d2 = Vector3.Dot(L, L) - tca * tca; 
            if (d2 > radiusSquared) return false; 
            float thc = (float)Math.Sqrt(radiusSquared - d2); 
            float t0 = tca - thc; 
            float t1 = tca + thc;
            if (t0 > t1)
            {
                float temp = t0;
                t0 = t1;
                t1 = temp;
            }

            if (t0 < 0)
            {
                t0 = t1; // if t0 is negative, let's use t1 instead 
                if (t0 < 0) return false; // both t0 and t1 are negative 
            }

            t = t0;
            return true;
        }

        public override Vector3 Normal(Intersection intersection = null)
        {
            return (intersection.ray.GetPoint(intersection.distance) - position).Normalized();
        }

        public override Vector3 Center()
        {
            return position;
        }

        public override Vector3 Min()
        {
            return position - new Vector3(radius, radius, radius);
        }

        public override Vector3 Max()
        {
            return position + new Vector3(radius, radius, radius);
        }
    }
}