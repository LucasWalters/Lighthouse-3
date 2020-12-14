using Lighthouse3.Primitives;
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
        public Rectangle rect;

        public AreaLight(Vector3 topLeft, Vector3 topRight, Vector3 bottomLeft, Vector3 color, float intensity) : base(color, intensity)
        {
            Material mat = new Material(color, emissive: true);
            rect = new Rectangle(topLeft, topRight, bottomLeft, mat);
        }

        public Vector3 RandomPointOnLight()
        {
            float u = Calc.Random();
            float v = Calc.Random();

            return rect.topLeft + u * rect.side1 + v * rect.side2;
        }

        public override Vector3 DirectIllumination(Intersection intersection, Vector3 normal, Scene scene, bool debug = false)
        {
            Vector3 intersectionPoint = intersection.ray.GetPoint(intersection.distance);
            Vector3 randomPoint = RandomPointOnLight();
            Vector3 toLight = randomPoint - intersectionPoint;
            float dist = toLight.Length;
            toLight /= dist;
            float cos_o = Vector3.Dot(-toLight, rect.normal);
            float cos_i = Vector3.Dot(toLight, normal);
            if (cos_o <= 0 || cos_i <= 0) 
                return Color.Black;

            // light is not behind surface point, trace shadow ray
            Ray ray = new Ray(intersectionPoint + Calc.Epsilon * toLight, toLight);
            bool occluded = ray.Occluded(scene.primitives, dist - Calc.Epsilon * 2);
            if (occluded)
                return Color.Black;

            // light is visible (V(p,p’)=1); calculate transport
            float solidAngle = (cos_o * rect.area) / (dist * dist);
            float lightPDF = 1f / solidAngle;
            // Multiply result with the number of lights in the scene
            return color * (cos_i / lightPDF) * intensity;
        }
    }
}
