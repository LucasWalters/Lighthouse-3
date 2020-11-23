using System;
using OpenTK.Graphics;
using System.Drawing;
using OpenTK;

namespace Lighthouse3
{
    public class Material
    {

        //Statics
        public static Material Red = new Material(Color4.Red, 0);
        public static Material Yellow = new Material(Color4.Yellow, 0);
        public static Material Green = new Material(Color4.Green, 0);
        public static Material Blue = new Material(Color4.PowderBlue, 0);
        public static Material Mirror = new Material(Color4.White, 1);


        public Vector3 color;
        public bool isCheckerboard;
        public float specularity;
        public float diffuse;

        // Texture?

        private Material() { }

        public Material(float r, float g, float b, float specularity) : this(new Vector3(r, g, b), specularity) { }

        public Material(Color4 color, float specularity) : this(new Vector3(color.R, color.G, color.B), specularity) { }

        public Material(Vector3 color, float specularity)
        {
            this.color = color;
            this.specularity = specularity;
            this.diffuse = 1 - specularity;
        }


    }
}
