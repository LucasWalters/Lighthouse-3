using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lighthouse3.Lights
{
    public class AreaLight : Light
    {
        public Vector3 topLeft;
        public Vector3 topRight;
        public Vector3 bottomLeft;
        public Vector3 lightNormal;
        public float area;
        public AreaLight(Vector3 topLeft, Vector3 topRight, Vector3 bottomLeft, Vector3 color, float intensity) : base(color, intensity)
        {
            this.topLeft = topLeft;
            this.topRight = topRight;
            this.bottomLeft = bottomLeft;
            Vector3 side1 = topRight - topLeft;
            Vector3 side2 = bottomLeft - topLeft;
            lightNormal = Vector3.Cross(side1, side2);
            area = side1.Length * side2.Length;
        }

        private Vector3 RandomPointOnLight()
        {
            float u = Calc.Random();
            float v = Calc.Random();

            return topLeft + u * (topRight - topLeft) + v * (bottomLeft - topLeft);
        }

        public override Vector3 DirectIllumination(Intersection intersection, Vector3 normal, Scene scene)
        {
            Vector3 intersectionPoint = intersection.ray.GetPoint(intersection.distance);
            Vector3 toLight = RandomPointOnLight() - intersectionPoint;
            float dist = toLight.Length;
            toLight /= dist;
            float cos_o = Vector3.Dot(-toLight, lightNormal);
            float cos_i = Vector3.Dot(toLight, normal);
            if (cos_o <= 0 || cos_i <= 0) 
                return Color.Black;

            // light is not behind surface point, trace shadow ray
            Ray ray = new Ray(intersectionPoint + Calc.Epsilon * toLight, toLight);
            bool occluded = ray.Occluded(scene.primitives, toLight.LengthSquared - Calc.Epsilon * 2);
            if (occluded)
                return Color.Black;

            // light is visible (V(p,p’)=1); calculate transport
            Vector3 BRDF = intersection.hit.material.color * Calc.InvPi;
            float solidAngle = (cos_o * area) / (dist * dist);
            // Multiply result with the number of lights in the scene
            return BRDF * color * solidAngle * cos_i;
        }
    }
}
