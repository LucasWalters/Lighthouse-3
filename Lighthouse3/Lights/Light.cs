using OpenTK;
using OpenTK.Graphics;

namespace Lighthouse3
{
    public abstract class Light
    {
        public Vector3 color;
        public float intensity;

        public Light(Vector3 color, float intensity)
        {
            this.color = color;
            this.intensity = intensity;
        }

        public abstract Vector3 DirectIllumination(Intersection intersection, Vector3 normal, Scene scene);
    }
}