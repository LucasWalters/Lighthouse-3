﻿using System.Numerics;
using System;
using System.Collections;

namespace Lighthouse3.Primitives
{
    public class Triangle : Primitive
    {
        public Vector3 p0;
        public Vector3 p1;
        public Vector3 p2;

        public Triangle(Vector3 p0, Vector3 p1, Vector3 p2, Material material) : base(material)
        {
            this.p0 = p0;
            this.p1 = p1;
            this.p2 = p2;
            bounds.min = Min();
            bounds.max = Max();
            bounds = bounds.ResetCenter();
        }

        // Returns null if no intersection
        public override bool Intersect(Ray ray, out float t)
        {
            t = -1f;
            Vector3 v0v1 = p1 - p0;
            Vector3 v0v2 = p2 - p0;
            Vector3 pvec = Vector3.Cross(ray.direction, v0v2);
            float det = Vector3.Dot(v0v1, pvec);
            // ray and triangle are parallel if det is close to 0
            if (Math.Abs(det) < Calc.Epsilon)
                return false;

            float invDet = 1 / det;

            Vector3 tvec = ray.origin - p0;
            float u = Vector3.Dot(tvec, pvec) * invDet;
            if (u < 0 || u > 1) return false;

            Vector3 qvec = Vector3.Cross(tvec, v0v1);
            float v = Vector3.Dot(ray.direction, qvec) * invDet;
            if (v < 0 || u + v > 1) return false;

            t = Vector3.Dot(v0v2, qvec) * invDet;
            return t > 0 && t < ray.distance;
        }

        public override Vector3 Normal(Intersection intersection = null)
        {
            return Vector3.Cross(p1 - p0, p2 - p0).Normalized();
        }

        public override Vector3 Center()
        {
            return (p0 + p1 + p2) / 3f;
        }

        public override Vector3 Min()
        {
            Vector3 min = p0;

            if (p1.X < min.X)
                min.X = p1.X;
            if (p2.X < min.X)
                min.X = p2.X;

            if (p1.Y < min.Y)
                min.Y = p1.Y;
            if (p2.Y < min.Y)
                min.Y = p2.Y;

            if (p1.Z < min.Z)
                min.Z = p1.Z;
            if (p2.Z < min.Z)
                min.Z = p2.Z;

            return min;
        }

        public override Vector3 Max()
        {
            Vector3 max = p0;

            if (p1.X > max.X)
                max.X = p1.X;
            if (p2.X > max.X)
                max.X = p2.X;

            if (p1.Y > max.Y)
                max.Y = p1.Y;
            if (p2.Y > max.Y)
                max.Y = p2.Y;

            if (p1.Z > max.Z)
                max.Z = p1.Z;
            if (p2.Z > max.Z)
                max.Z = p2.Z;

            return max;
        }
    }
}