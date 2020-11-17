using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lighthouse3.Primitives
{
    public class Plane : Primitive
    {
        public static readonly float parallel_margin = 0.000001f;
        public Vector3 position;
        public Vector3 normal;

        public Plane(Vector3 position, Vector3 normal, Material material) : base(material)
        {
            this.position = position;
            this.normal = normal;
        }

        public override Intersection Intersect(Ray ray)
        {
            float rayDot = Vector3.Dot(ray.direction, normal);
            Vector3 n = normal;
            if (rayDot > 0)
            {
                n = -n;
                rayDot = Vector3.Dot(ray.direction, n);
            }
            float t = Vector3.Dot(position - ray.origin, n);
            if (t < 0)
                return new Intersection(t / rayDot, ray, n, material, false);
            return null;
        }
    }
}
