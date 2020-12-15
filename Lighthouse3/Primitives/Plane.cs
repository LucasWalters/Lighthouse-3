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
        public Vector3 position;
        public Vector3 normal;

        public Plane(Vector3 position, Vector3 normal, Material material) : base(material)
        {
            this.position = position;
            this.normal = normal;
        }

        public override bool Intersect(Ray ray, out float t)
        {
            float rayDot = Vector3.Dot(ray.direction, normal);
            Vector3 n = normal;
            if (rayDot > 0)
            {
                n = -n;
                rayDot = Vector3.Dot(ray.direction, n);
            }
            t = Vector3.Dot(position - ray.origin, n);
            if (t < 0)
            {
                t /= rayDot;
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
            return position;
        }

        public override Vector3 Min()
        {
            throw new InvalidOperationException();
        }

        public override Vector3 Max()
        {
            throw new InvalidOperationException();
        }
    }
}
