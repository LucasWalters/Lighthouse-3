using System.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lighthouse3.Primitives
{
    public class Rectangle : Primitive
    {
        public Vector3 topLeft;
        public Vector3 topRight;
        public Vector3 bottomLeft;
        public Vector3 side1;
        public Vector3 side2;
        public Vector3 normal;
        public float area;

        private readonly float invSide1LengthSquared;
        private readonly float invSide2LengthSquared;

        public Rectangle(Vector3 topLeft, Vector3 topRight, Vector3 bottomLeft, Material material) : base(material)
        {
            this.topLeft = topLeft;
            this.topRight = topRight;
            this.bottomLeft = bottomLeft;
            side1 = topRight - topLeft;
            side2 = bottomLeft - topLeft;
            normal = Vector3.Cross(side1, side2).Normalized();
            area = side1.Length() * side2.Length();
            bounds.min = Min();
            bounds.max = Max();
            bounds = bounds.ResetCenter();
            invSide1LengthSquared = 1f / side1.LengthSquared();
            invSide2LengthSquared = 1f / side2.LengthSquared();
        }

        public override bool Intersect(Ray ray, out float t)
        {
            float rayDot = Vector3.Dot(ray.direction, normal);
            Vector3 n = normal;
            if (rayDot > 0)
            {
                n = -n;
                rayDot = -rayDot;
            }
            t = Vector3.Dot(topLeft - ray.origin, n);
            if (t < 0)
            {
                t /= rayDot;
                if (t >= ray.distance)
                    return false;
                Vector3 toPoint = ray.GetPoint(t) - topLeft;
                float dotSide1 = Vector3.Dot(side1, toPoint);
                float dotSide2 = Vector3.Dot(side2, toPoint);
                if (dotSide1 < 0 || dotSide2 < 0)
                    return false;
                float proj = dotSide1 * invSide1LengthSquared;
                if (proj > 1)
                    return false;
                proj = dotSide2 * invSide2LengthSquared;
                if (proj > 1)
                    return false;
                return true;
            }
            return false;
        }

        public override Vector3 Normal(Intersection intersection = null)
        {
            return normal;
        }

        public override Vector3 Center()
        {
            return topLeft + side1 * 0.5f + side2 * 0.5f;
        }

        public override Vector3 Min()
        {
            Vector3 bottomRight = bottomLeft + side1;

            Vector3 min = topLeft;
            if (topRight.X < min.X)
                min.X = topRight.X;
            if (bottomLeft.X < min.X)
                min.X = bottomLeft.X;
            if (bottomRight.X < min.X)
                min.X = bottomRight.X;

            if (topRight.Y < min.Y)
                min.Y = topRight.Y;
            if (bottomLeft.Y < min.Y)
                min.Y = bottomLeft.Y;
            if (bottomRight.Y < min.Y)
                min.Y = bottomRight.Y;

            if (topRight.Z < min.Z)
                min.Z = topRight.Z;
            if (bottomLeft.Z < min.Z)
                min.Z = bottomLeft.Z;
            if (bottomRight.Z < min.Z)
                min.Z = bottomRight.Z;
            return min;
        }

        public override Vector3 Max()
        {
            Vector3 bottomRight = bottomLeft + side1;

            Vector3 max = topLeft;
            if (topRight.X > max.X)
                max.X = topRight.X;
            if (bottomLeft.X > max.X)
                max.X = bottomLeft.X;
            if (bottomRight.X > max.X)
                max.X = bottomRight.X;

            if (topRight.Y > max.Y)
                max.Y = topRight.Y;
            if (bottomLeft.Y > max.Y)
                max.Y = bottomLeft.Y;
            if (bottomRight.Y > max.Y)
                max.Y = bottomRight.Y;

            if (topRight.Z > max.Z)
                max.Z = topRight.Z;
            if (bottomLeft.Z > max.Z)
                max.Z = bottomLeft.Z;
            if (bottomRight.Z > max.Z)
                max.Z = bottomRight.Z;
            return max;
        }
    }
}
