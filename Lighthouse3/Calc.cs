using OpenTK;
using OpenTK.Graphics;
using System.Collections;

namespace Lighthouse3
{
    public static class Calc
    {
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
    }
}