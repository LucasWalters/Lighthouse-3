using System;
using OpenTK;
using OpenTK.Graphics;

namespace Lighthouse3
{
    public static class Color
    {
        public static readonly Vector3 Black = Vector3.Zero;
        public static readonly Vector3 White = Vector3.One;
        public static readonly Vector3 Red = Vector3.UnitX;
        public static readonly Vector3 Blue = Vector3.UnitY;
        public static readonly Vector3 Green = Vector3.UnitZ;

        public static int ToARGB(Vector3 color)
        {
            color *= 256;
            int r = Calc.Clamp((int)color.X, 0, 255);
            int g = Calc.Clamp((int)color.Y, 0, 255);
            int b = Calc.Clamp((int)color.Z, 0, 255);

            return b + (g << 8) + (r << 16);
        }

        static float inv256 = 1 / 256f;

        public static Vector3 FromARBG(int color)
        {
            float b = (color & 255) * inv256;
            float g = ((color >> 8) & 255) * inv256;
            float r = ((color >> 16) & 255) * inv256;
            return new Vector3(r, g, b);
        }
    }
}
