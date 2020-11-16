using System;
using OpenTK.Graphics;
using System.Drawing;

namespace Lighthouse3
{
    public class Material
    {

        public Color4 color;

        // Could call it absorption, which is the inverse of reflectiveness
        public float reflectiveness;
        // Texture?

        // Constructor for testing purposes only
        public Material()
        {
            this.color = new Color4(255, 0, 0, 1);
            this.reflectiveness = 0;
        }

        public Material(float r, float g, float b, float a, float reflectiveness) : this(new Color4(r, g, b, a), reflectiveness)
        {
        }

        public Material(Color4 color, float reflectiveness)
        {
            this.color = color;
            this.reflectiveness = reflectiveness;
        }

    }
}
