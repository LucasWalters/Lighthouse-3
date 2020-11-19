using OpenTK;
using System;
using System.Collections;

namespace Lighthouse3.Primitives
{
    public class Triangle : Primitive
    {
        public Vector3 p0;
        public Vector3 p1;
        public Vector3 p2;

        public Vector3 e1;
        public Vector3 e2;

        public Vector3 normal;

        public Triangle(Vector3 p1, Vector3 p2, Vector3 p3, Material material) : base(material)
        {
            this.p0 = p1;
            this.p1 = p2;
            this.p2 = p3;

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
            //return NewIntersect(ray);
            return NewestIntersect(ray);
        }


        private Intersection OldIntersect(Ray ray)
        {
            float rayDot = Vector3.Dot(ray.direction, normal);
            if (rayDot > 0)
                return null;
            float t = Vector3.Dot(p0 - ray.origin, normal);
            if (t >= 0)
                return null;
            float distance = t / rayDot;
            Vector3 hit = ray.GetPoint(distance);

            Vector3 edge32 = p2 - p1;
            Vector3 edge13 = p0 - p2;

            Vector3 C1 = Vector3.Cross(e1, hit - p0);
            if (Vector3.Dot(normal, C1) < 0) return null;

            Vector3 C2 = Vector3.Cross(edge32, hit - p1);
            if (Vector3.Dot(normal, C2) < 0) return null;

            Vector3 C3 = Vector3.Cross(edge13, hit - p2);
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
            Vector3 d = ray.origin - p0;
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

        private Intersection NewestIntersect(Ray ray)
        {
            // Translate vertices based on ray origin
            Vector3 p0t = p0 - ray.origin;
            Vector3 p1t = p1 - ray.origin;
            Vector3 p2t = p2 - ray.origin;

            // Permute components of triangle vertices and ray direction
            int kz = Calc.MaxDimension(Calc.Abs(ray.direction));
            int kx = kz + 1; if (kx == 3) kx = 0;
            int ky = kx + 1; if (ky == 3) ky = 0;
            Vector3 d = Calc.Permute(ray.direction, kx, ky, kz);
            p0t = Calc.Permute(p0t, kx, ky, kz);
            p1t = Calc.Permute(p1t, kx, ky, kz);
            p2t = Calc.Permute(p2t, kx, ky, kz);

            // Apply shear transformation to translated vertex positions
            float Sx = -d.X / d.Z;
            //float Sy = -d.Y / d.Z;
            float Sz = 1f / d.Z;
            p0t.X += Sx * p0t.Z;
            p0t.Y += Sx * p0t.Z;
            p1t.X += Sx * p1t.Z;
            p1t.Y += Sx * p1t.Z;
            p2t.X += Sx * p2t.Z;
            p2t.Y += Sx * p2t.Z;

            // Compute edge function coefficients e0, e1, and e2
            float e0 = p1t.X * p2t.Y - p1t.Y * p2t.X;
            float e1 = p2t.X * p0t.Y - p2t.Y * p0t.X;
            float e2 = p0t.X * p1t.Y - p0t.Y * p1t.X;

            // Perform triangle edge and determinant tests
            if ((e0 < 0 || e1 < 0 || e2 < 0) && (e0 > 0 || e1 > 0 || e2 > 0))
                return null;
            float det = e0 + e1 + e2;
            if (det == 0)
                return null;

            // Compute scaled hit distance to triangle and test against ray t range
            p0t.Z *= Sz;
            p1t.Z *= Sz;
            p2t.Z *= Sz;
            float tScaled = e0 * p0t.Z + e1 * p1t.Z + e2 * p2t.Z;
            if (det < 0 && tScaled >= 0)
                return null;
            else if (det > 0 && tScaled <= 0)
                return null;

            // Compute barycentric coordinates and t value for triangle intersection
            float invDet = 1 / det;
            //float b0 = e0 * invDet;
            //float b1 = e1 * invDet;
            //float b2 = e2 * invDet;
            float t = tScaled * invDet;

            //Vector3 pHit = b0 * p0 + b1 * p1 + b2 * p2;

            return new Intersection(t, ray, Vector3.Cross(p0 - p2, p1 - p2).Normalized(), material, false);

        }

        // 23 adds 27 mults 1 division
    }

}