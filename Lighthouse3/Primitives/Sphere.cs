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
        }

        // Returns null if no intersection
        public override bool Intersect(Ray ray, out float t)
        {
            //if((ray.origin-position).LengthSquared < radiusSquared)
            //{
                // ABC formula
            //} else
            //{
            t = -1f;
            Vector3 toCenter = position - ray.origin;
            t = Vector3.Dot(toCenter, ray.direction);
            Vector3 perpToCenter = toCenter - t * ray.direction;
            float p2 = perpToCenter.LengthSquared;
            if (p2 > radiusSquared) return false;
            t -= (float)Math.Sqrt(radiusSquared - p2);
            if (t <= 0) return false;
            //Vector3 normal = (ray.GetPoint(distance) - position).Normalized();
            return true;
                //return new Intersection(distance, ray, normal, material, false);
            //}
        }

        public override Vector3 Normal(Intersection intersection = null)
        {
            return (intersection.ray.GetPoint(intersection.distance) - position).Normalized();
        }
    }
}