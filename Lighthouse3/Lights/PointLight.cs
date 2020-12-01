using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lighthouse3.Lights
{
    public class PointLight : Light
    {
        public Vector3 position;
        public PointLight(Vector3 position, Vector3 color, float intensity) : base(color, intensity)
        {
            this.position = position;
        }

        //TODO: use intensity somewhere
        public override Vector3 DirectIllumination(Intersection intersection, Vector3 normal, Scene scene, bool debug = false)
        {
            if (debug)
            {
                Console.WriteLine(intersection.hit.material.color);
            }
            Vector3 intersectionPoint = intersection.ray.GetPoint(intersection.distance);
            Vector3 toLight = position - intersectionPoint;
            //Light is obstructed by face that we hit
            if (Vector3.Dot(toLight, normal) <= 0)
                return Vector3.Zero;
            //Check if light is obstructed
            Vector3 rayDirection = toLight.Normalized();
            Ray ray = new Ray(intersectionPoint + rayDirection * Calc.Epsilon, rayDirection);
            bool occluded = ray.OccludedSquared(scene.primitives, toLight.LengthSquared); //Note: Normalizing the length and then using lengthsquared doesn't help
            if (occluded)
                return Color.Black;
            float distance = toLight.LengthSquared;
            return color * Vector3.Dot(normal, rayDirection) * (intensity / distance);
        }
    }
}
