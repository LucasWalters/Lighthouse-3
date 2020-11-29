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
        public bool inverted;

        public Sphere(Vector3 position, float radius, Material material, bool inverted = false) : base(material)
        {
            this.position = position;
            this.radius = radius;
            radiusSquared = radius * radius;
            this.inverted = inverted;
        }

        // From https://www.scratchapixel.com/lessons/3d-basic-rendering/minimal-ray-tracer-rendering-simple-shapes/ray-sphere-intersection
        public override bool Intersect(Ray ray, out float t)
        {
            t = -1;
            // geometric solution
            Vector3 L = position - ray.origin; 
            float tca = Vector3.Dot(L, ray.direction); 
            // if (tca < 0) return false;
            float d2 = Vector3.Dot(L, L) - tca * tca; 
            if (d2 > radiusSquared) return false; 
            float thc = (float)Math.Sqrt(radiusSquared - d2); 
            float t0 = tca - thc; 
            float t1 = tca + thc;
            if (t0 > t1)
            {
                float temp = t0;
                t0 = t1;
                t1 = temp;
            }

            if (t0 < 0)
            {
                t0 = t1; // if t0 is negative, let's use t1 instead 
                if (t0 < 0) return false; // both t0 and t1 are negative 
            }

            t = t0;
            return true;



            //t = -1f;
            //backface = false;
            //Vector3 toCenter = position - ray.origin;
            //float tca = Vector3.Dot(toCenter, ray.direction);
            //Vector3 perpToCenter = toCenter - tca * ray.direction;
            //float p2 = perpToCenter.LengthSquared;
            //if (p2 > radiusSquared) return false;
            //float thc = (float)Math.Sqrt(radiusSquared - p2);
            //float t0 = tca - thc;
            //float t1 = tca + thc;
            //if (t0 <= 0)
            //{
            //    t = t1;
            //    backface = true;
            //}
            //else if (t1 <= 0)
            //{
            //    t = t0;
            //    backface = true;
            //}
            //else
            //{
            //    t = Calc.Min(t0, t1);
            //    return true;
            //}

            //if ((t0 <= 0 || t1 <= 0) && t > 0)
            //    Console.WriteLine(t);

            //if (t <= 0) return false;
            //return true;
        }

        public override Vector3 Normal(Intersection intersection = null)
        {
            return (intersection.ray.GetPoint(intersection.distance) - position).Normalized() * (inverted ? -1 : 1);
        }
    }
}