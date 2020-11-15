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

        public Sphere(Material material, Vector3 position, float radius) : base(material)
        {
            this.position = position;
            this.radius = radius;
            radiusSquared = radius * radius;
        }

        // Returns null if no intersection
        public override Intersection Intersect(Ray ray)
        {
            Vector3 toCenter = position - ray.origin;
            float distance = Vector3.Dot(toCenter, ray.direction);
            Vector3 perpToCenter = toCenter - distance * ray.direction;
            float p2 = Vector3.Dot(perpToCenter, perpToCenter);
            if (p2 > radiusSquared) return null;
            distance -= (float)Math.Sqrt(radiusSquared - p2);
            if (distance <= 0) return null;
            return new Intersection(distance, Vector3.Zero, material);
        }
    }
}