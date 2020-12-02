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
        public static Material Gray = new Material(Color4.Gray, 0);
        public static Material Red = new Material(Color4.Red, 0);
        public static Material Yellow = new Material(Color4.Yellow, 0);
        public static Material Green = new Material(Color4.Green, 0);
        public static Material Blue = new Material(Color4.Blue, 0);
        public static Material MediumVioletRed = new Material(Color4.MediumVioletRed, 0);
        public static Material YellowGreen = new Material(Color4.YellowGreen, 0);
        public static Material DarkOliveGreen = new Material(Color4.DarkOliveGreen, 0);
        public static Material BlueViolet = new Material(Color4.BlueViolet, 0);
        public static Material Mirror = new Material(Color4.White, specularity: 1f);
        public static Material GlossyMirror = new Material(Color4.White, specularity: 1f, glossiness: 0.15f);
        public static Material Light = new Material(Color4.White, emissive: true);
        public static Material Glass = new Material(Color4.White, 0, 1, Material.RefractiveIndex.Glass);
        public static Material Vacuum = new Material(Color4.White, 0, 1, Material.RefractiveIndex.Vacuum);


        public Vector3 color;
        public bool isCheckerboard;
        public float specularity;
        public float glossiness;
        public float diffuse;
        public float transparency;
        public float refractiveIndex;
        public bool emissive = false;


        // Texture?

        private Material() { }

        public Material(float r, float g, float b, float specularity = 0f, float transparency = 0f, float refractiveIndex = RefractiveIndex.Vacuum, bool emissive = false, float glossiness = 0f) : 
            this(new Vector3(r, g, b), specularity, transparency, refractiveIndex, emissive, glossiness) { }

        public Material(Color4 color, float specularity = 0f, float transparency = 0f, float refractiveIndex = RefractiveIndex.Vacuum, bool emissive = false, float glossiness = 0f) : 
            this(new Vector3(color.R, color.G, color.B), specularity, transparency, refractiveIndex, emissive, glossiness) { }

        public Material(Vector3 color, float specularity = 0f, float transparency = 0f, float refractiveIndex = RefractiveIndex.Vacuum, bool emissive = false, float glossiness = 0f)
        {
            this.color = color;
            this.specularity = Calc.Clamp(specularity);
            this.diffuse = Calc.Clamp(1f - specularity - transparency);
            this.transparency = Calc.Clamp(transparency);
            this.refractiveIndex = refractiveIndex;
            this.emissive = emissive;
            this.glossiness = glossiness;
            if (this.specularity + this.transparency + this.diffuse > 1)
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
