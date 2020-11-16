using OpenTK;
using OpenTK.Graphics;

namespace Lighthouse3
{

    public class Pointlight
    {
        public Vector3 position;
        public Color4 color;
        public float intensity;

        public Pointlight(Vector3 position, Color4 color, float intensity)
        {
            this.position = position;
            this.color = color;
            this.intensity = intensity;
        }
    }
}