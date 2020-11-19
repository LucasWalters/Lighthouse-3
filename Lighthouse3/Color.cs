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
            //Temporary cast to Color4
            return new Color4(color.X, color.Y, color.Z, 1).ToArgb();
        }
    }
}
