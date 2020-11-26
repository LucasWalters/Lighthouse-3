using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections;

namespace Lighthouse3
{
    public static class Calc
    {

        static Random random = new Random();

        //Inverse lerp, maps value to [0, 1] based on min and max values
        public static float ILerp(float min, float max, float at)
        {
            return (at - min) / (max - min);
        }
        
        public static float Lerp(float min, float max, float t)
        {
            return min + (max - min) * t;
        }

        public static float Clamp(float value, float min = 0, float max = 1)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }

        public static int Clamp(int value, int min = 0, int max = 1)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }

        public static float Min(float a, float b)
        {
            if (a > b)
                return b;
            return a;
        }

        public static float Max(float a, float b)
        {
            if (a < b)
                return b;
            return a;
        }

        public static int MaxDimension(Vector3 vector)
        {
            return (vector.X > vector.Y) ? ((vector.X > vector.Z) ? 0 : 2) : ((vector.Y > vector.Z) ? 1 : 2);
        }

        public static Vector3 Abs(Vector3 vector)
        {
            return new Vector3(Math.Abs(vector.X), Math.Abs(vector.Y), Math.Abs(vector.Z));
        }

        public static Vector3 Permute(Vector3 vector, int x, int y, int z)
        {
            return new Vector3(vector[x], vector[y], vector[z]);
        }

        public static float Random()
        {
            return (float)random.NextDouble();
        }
    }

    public static class Extensions
    {
        public static Color4 Add(this Color4 left, Color4 right)
        {
            float r = Calc.Max(left.R + right.R, 0);
            float g = Calc.Max(left.G + right.G, 0);
            float b = Calc.Max(left.B + right.B, 0);
            return new Color4(r, g, b, left.A);
        }

        public static Color4 Subtract(this Color4 left, Color4 right)
        {
            float r = Calc.Max(left.R - right.R, 0);
            float g = Calc.Max(left.G - right.G, 0);
            float b = Calc.Max(left.B - right.B, 0);
            return new Color4(r, g, b, left.A);
        }

        public static Color4 Multiply(this Color4 left, Color4 right)
        {
            float r = Calc.Max(left.R * right.R, 0);
            float g = Calc.Max(left.G * right.G, 0);
            float b = Calc.Max(left.B * right.B, 0);
            return new Color4(r, g, b, left.A);
        }

        public static Color4 Multiply(this Color4 color, float scalar)
        {
            float r = Calc.Max(color.R * scalar, 0);
            float g = Calc.Max(color.G * scalar, 0);
            float b = Calc.Max(color.B * scalar, 0);
            return new Color4(r, g, b, color.A);
        }

        public static Vector3 Sqrt(this Vector3 vector)
        {
            float x = (float)Math.Sqrt(vector.X);
            float y = (float)Math.Sqrt(vector.Y);
            float z = (float)Math.Sqrt(vector.Z);
            return new Vector3(x, y, z);
        }
    }
}