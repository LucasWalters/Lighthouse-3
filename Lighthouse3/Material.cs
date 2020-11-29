using System;
using OpenTK.Graphics;
using System.Drawing;
using OpenTK;

namespace Lighthouse3
{
    public class Material
    {

        //Statics
        public static Material Black = new Material(Color4.Black, 0);
        public static Material Red = new Material(Color4.Red, 0);
        public static Material Yellow = new Material(Color4.Yellow, 0);
        public static Material Green = new Material(Color4.Green, 0);
        public static Material Blue = new Material(Color4.Blue, 0);
        public static Material Mirror = new Material(Color4.White, 1);
        public static Material Glass = new Material(Color4.White, 0, 1, Material.RefractiveIndex.Glass);
        public static Material Vacuum = new Material(Color4.White, 0, 1, Material.RefractiveIndex.Vacuum);


        public Vector3 color;
        public bool isCheckerboard;
        public float specularity;
        public float diffuse;
        public float transparancy;
        public float refractiveIndex;

        // Texture?

        private Material() { }

        public Material(float r, float g, float b, float specularity = 0f, float transparancy = 0f, float refractiveIndex = RefractiveIndex.Vacuum) : this(new Vector3(r, g, b), specularity, transparancy, refractiveIndex) { }

        public Material(Color4 color, float specularity = 0f, float transparancy = 0f, float refractiveIndex = RefractiveIndex.Vacuum) : this(new Vector3(color.R, color.G, color.B), specularity, transparancy, refractiveIndex) { }

        public Material(Vector3 color, float specularity = 0f, float transparancy = 0f, float refractiveIndex = RefractiveIndex.Vacuum)
        {
            this.color = color;
            this.specularity = Calc.Clamp(specularity);
            this.diffuse = Calc.Clamp(1f - specularity - transparancy);
            this.transparancy = Calc.Clamp(transparancy);
            this.refractiveIndex = refractiveIndex;
            if (this.specularity + this.transparancy + this.diffuse > 1)
            {
                Console.WriteLine("Error! Material specularity and transparancy can not be more than 1 combined.");
            }
        }

        public static class RefractiveIndex
        {
            public const float Vacuum = 1f;
            public const float Air = 1.0003f;
            public const float Water = 1.333f;
            public const float Glass = 1.517f;
        }

    }
}
