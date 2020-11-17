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
        public override Intersection Intersect(Ray ray)
        {
            //if((ray.origin-position).LengthSquared < radiusSquared)
            //{
                // ABC formula
            //} else
            {
                Vector3 toCenter = position - ray.origin;
                float distance = Vector3.Dot(toCenter, ray.direction);
                Vector3 perpToCenter = toCenter - distance * ray.direction;
                float p2 = perpToCenter.LengthSquared;
                if (p2 > radiusSquared) return null;
                distance -= (float)Math.Sqrt(radiusSquared - p2);
                if (distance <= 0) return null;
                Vector3 normal = (ray.GetPoint(distance) - position).Normalized();
                return new Intersection(distance, ray, normal, material, false);
            }
        }
    }
}