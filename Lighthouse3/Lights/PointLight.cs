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
        public static readonly float ray_offset = 0.0005f;
        public Vector3 position;
        public PointLight(Vector3 position, Vector3 color, float intensity) : base(color, intensity)
        {
            this.position = position;
        }

        //TODO: use intensity somewhere
        public override Vector3 DirectIllumination(Intersection intersection, Scene scene)
        {
            Vector3 intersectionPoint = intersection.ray.GetPoint(intersection.distance);
            Vector3 toLight = position - intersectionPoint;
            //Light is obstructed by face that we hit
            if (Vector3.Dot(toLight, intersection.hit.Normal(intersection)) <= 0)
                return Vector3.Zero;
            //Check if light is obstructed
            Vector3 rayDirection = toLight.Normalized();
            Ray ray = new Ray(intersectionPoint + rayDirection * ray_offset, rayDirection);
            bool occluded = ray.Occluded(scene.primitives, toLight.LengthSquared);

            float distance = toLight.LengthSquared;
            return occluded ? Color.Black : color * intensity / distance;
        }
    }
}
