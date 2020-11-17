using OpenTK;
using System;
using System.Collections;

namespace Lighthouse3.Primitives
{
    public class Triangle : Primitive
    {
        public static readonly float parallel_margin = 0.000001f;
        public Vector3[] coordinates = new Vector3[3];

        public Triangle(Vector3 c1, Vector3 c2, Vector3 c3, Material material) : base(material)
        {
            this.coordinates[0] = c1;
            this.coordinates[1] = c2;
            this.coordinates[2] = c3;
        }

        // Returns null if no intersection
        public override Intersection Intersect(Ray ray)
        {
            Vector3 normal = Vector3.Cross(coordinates[1] - coordinates[0], coordinates[2] - coordinates[0]);

            float rayDot = Vector3.Dot(ray.direction, normal);
            if (rayDot > -parallel_margin)
                return null;
            float t = Vector3.Dot(coordinates[0] - ray.origin, normal);
            if (t >= 0)
                return null;

            Vector3 hit = ray.GetPoint(t / rayDot);

            Vector3 edge10 = coordinates[1] - coordinates[0];
            Vector3 edge21 = coordinates[2] - coordinates[1];
            Vector3 edge02 = coordinates[0] - coordinates[2];

            Vector3 C1 = Vector3.Cross(edge10, hit - coordinates[0]);
            if (Vector3.Dot(normal, C1) < 0) return null;

            Vector3 C2 = Vector3.Cross(edge21, hit - coordinates[1]);
            if (Vector3.Dot(normal, C2) < 0) return null;

            Vector3 C3 = Vector3.Cross(edge02, hit - coordinates[2]);
            if (Vector3.Dot(normal, C3) < 0) return null;

            return new Intersection(t / rayDot, ray, normal, material);

        }
    }
}