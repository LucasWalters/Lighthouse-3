using System;
using OpenTK.Graphics;
using System.Drawing;

namespace Lighthouse3
{
    public class Material
    {
        public Color4 color;

        public float specularity;
        public float diffuse;

        // Texture?

        // Constructor for testing purposes only
        private Material()
        {
        }

        public Material(float r, float g, float b, float a, float specularity) : this(new Color4(r, g, b, a), specularity)
        {
        }

        public Material(Color4 color, float specularity)
        {
            this.color = color;
            this.specularity = specularity;
            this.diffuse = 1 - specularity;
        }

    }
}
