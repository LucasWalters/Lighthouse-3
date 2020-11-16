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
        public PointLight(Vector3 position, Color4 color, float intensity) : base(color, intensity)
        {
            this.position = position;
        }

        //TODO: use intensity somewhere
        public override Color4 DirectIllumination(Intersection intersection, Scene scene)
        {
            Vector3 intersectionPoint = intersection.ray.GetPoint(intersection.distance);
            Vector3 toLight = position - intersectionPoint;
            //Light is obstructed by face that we hit
            if (Vector3.Dot(toLight, intersection.normal) <= 0)
                return Color4.Black;
            //Check if light is obstructed
            Ray ray = new Ray(intersectionPoint, toLight.Normalized());
            Intersection i = ray.NearestIntersection(scene.primitives);
            if (i == null || i.distance * i.distance > toLight.LengthSquared)
                return color;

            return Color4.Black;
        }
    }
}
