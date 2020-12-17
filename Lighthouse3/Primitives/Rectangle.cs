using OpenTK;
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

        public Rectangle(Vector3 topLeft, Vector3 topRight, Vector3 bottomLeft, Material material) : base(material)
        {
            this.topLeft = topLeft;
            this.topRight = topRight;
            this.bottomLeft = bottomLeft;
            side1 = topRight - topLeft;
            side2 = bottomLeft - topLeft;
            normal = Vector3.Cross(side1, side2);
            area = side1.Length * side2.Length;
            bounds.min = Min();
            bounds.max = Max();
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
                Vector3 toPoint = ray.GetPoint(t) - topLeft;
                float dotSide1 = Vector3.Dot(side1, toPoint);
                float dotSide2 = Vector3.Dot(side2, toPoint);
                if (dotSide1 < 0 || dotSide2 < 0)
                    return false;
                float proj = dotSide1 / side1.LengthSquared;
                if (proj > 1)
                    return false;
                proj = dotSide2 / side2.LengthSquared;
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
            for (int xyz = 0; xyz < 3; xyz++)
            {
                if (topRight[xyz] < min[xyz])
                    min[xyz] = topRight[xyz];
                if (bottomLeft[xyz] < min[xyz])
                    min[xyz] = bottomLeft[xyz];
                if (bottomRight[xyz] < min[xyz])
                    min[xyz] = bottomRight[xyz];
            }
            return min;
        }

        public override Vector3 Max()
        {
            Vector3 bottomRight = bottomLeft + side1;

            Vector3 max = topLeft;
            for (int xyz = 0; xyz < 3; xyz++)
            {
                if (topRight[xyz] > max[xyz])
                    max[xyz] = topRight[xyz];
                if (bottomLeft[xyz] > max[xyz])
                    max[xyz] = bottomLeft[xyz];
                if (bottomRight[xyz] > max[xyz])
                    max[xyz] = bottomRight[xyz];
            }
            return max;
        }
    }
}
