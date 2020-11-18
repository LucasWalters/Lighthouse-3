using OpenTK;
using System;
using System.Collections;

namespace Lighthouse3.Primitives
{
    public class Triangle : Primitive
    {
        public Vector3 p1;
        public Vector3 p2;
        public Vector3 p3;

        public Vector3 e1;
        public Vector3 e2;

        public Vector3 normal;

        public Triangle(Vector3 p1, Vector3 p2, Vector3 p3, Material material) : base(material)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;

            e1 = p2 - p1;
            e2 = p3 - p1;
            normal = Vector3.Cross(e1, e2).Normalized();
        }

        // Returns null if no intersection
        public override Intersection Intersect(Ray ray)
        {


            /* 1 cross = 3 adds & 6 mults
             * 1 dot = 2 adds & 3 mults
             * 
             * current:
             * 1 division 
             * 52 adds 
             * 42 multiplications
             * 
             * improvement:
             * 1 division
             * 17 adds
             * 27 multiplies
             * + 1 cross for normal (might be optional)
             */

            //return OldIntersect(ray);
            return NewIntersect(ray);
        }


        private Intersection OldIntersect(Ray ray)
        {
            float rayDot = Vector3.Dot(ray.direction, normal);
            if (rayDot > 0)
                return null;
            float t = Vector3.Dot(p1 - ray.origin, normal);
            if (t >= 0)
                return null;
            float distance = t / rayDot;
            Vector3 hit = ray.GetPoint(distance);

            Vector3 edge32 = p3 - p2;
            Vector3 edge13 = p1 - p3;

            Vector3 C1 = Vector3.Cross(e1, hit - p1);
            if (Vector3.Dot(normal, C1) < 0) return null;

            Vector3 C2 = Vector3.Cross(edge32, hit - p2);
            if (Vector3.Dot(normal, C2) < 0) return null;

            Vector3 C3 = Vector3.Cross(edge13, hit - p3);
            if (Vector3.Dot(normal, C3) < 0) return null;

            return new Intersection(distance, ray, normal, material, false);
        }

        private Intersection NewIntersect(Ray ray)
        {
            // Compute s1
            Vector3 s1 = Vector3.Cross(ray.direction, e2);

            // Compute divisor
            float divisor = Vector3.Dot(s1, e1);
            if (divisor == 0)
                return null;
            float invDivisor = 1f / divisor;

            // Compute first barycentric coordinate
            Vector3 d = ray.origin - p1;
            float b1 = Vector3.Dot(d, s1) * invDivisor;
            if (b1 < 0 || b1 > 1)
                return null;

            // Compute second barycentric coordinate
            Vector3 s2 = Vector3.Cross(d, e1);
            float b2 = Vector3.Dot(ray.direction, s2) * invDivisor;
            if (b2 < 0 || b2 > 1)
                return null;

            float t = Vector3.Dot(e2, s2) * invDivisor;

            return new Intersection(t, ray, normal, material, false);

        }

        // 23 adds 27 mults 1 division
    }

}