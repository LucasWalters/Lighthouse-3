using OpenTK;
using OpenTK.Graphics;

namespace Lighthouse3
{
    public abstract class Light
    {
        public Color4 color;
        public float intensity;

        public Light(Color4 color, float intensity)
        {
            this.color = color;
            this.intensity = intensity;
        }

        public abstract Color4 DirectIllumination(Intersection intersection, Scene scene);
    }
}